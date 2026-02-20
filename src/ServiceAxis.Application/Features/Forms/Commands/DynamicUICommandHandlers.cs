using MediatR;
using ServiceAxis.Application.Contracts.Persistence;
using ServiceAxis.Domain.Entities.Forms;
using ServiceAxis.Shared.Exceptions;
using System.Linq;
using Microsoft.Extensions.Caching.Memory;

namespace ServiceAxis.Application.Features.Forms.Commands;

public class DynamicUICommandHandlers :
    IRequestHandler<CreateUiPolicyCommand, Guid>,
    IRequestHandler<UpdateUiPolicyCommand, Unit>,
    IRequestHandler<DeleteUiPolicyCommand, Unit>,
    IRequestHandler<CreateFieldRuleCommand, Guid>,
    IRequestHandler<UpdateFieldRuleCommand, Unit>,
    IRequestHandler<DeleteFieldRuleCommand, Unit>,
    IRequestHandler<CreateClientScriptCommand, Guid>,
    IRequestHandler<UpdateClientScriptCommand, Unit>,
    IRequestHandler<DeleteClientScriptCommand, Unit>
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _memoryCache;

    public DynamicUICommandHandlers(IUnitOfWork uow, IMemoryCache memoryCache)
    {
        _uow = uow;
        _memoryCache = memoryCache;
    }

    // ─── UI Policy ──────────────────────────────────────────────────────────

    public async Task<Guid> Handle(CreateUiPolicyCommand request, CancellationToken cancellationToken)
    {
        var policy = new UiPolicy
        {
            Name = request.Name,
            Description = request.Description,
            TableId = request.TableId,
            ExecutionOrder = request.ExecutionOrder,
            FormContext = request.FormContext,
            IsActive = request.IsActive,
            Version = 1,
            Conditions = request.Conditions.Select(c => new UiPolicyCondition
            {
                FieldId = c.FieldId,
                Operator = c.Operator,
                Value = c.Value,
                LogicalGroup = c.LogicalGroup
            }).ToList(),
            Actions = request.Actions.Select(a => new UiPolicyAction
            {
                TargetFieldId = a.TargetFieldId,
                ActionType = a.ActionType
            }).ToList()
        };

        await _uow.Repository<UiPolicy>().AddAsync(policy, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(request.TableId);

        return policy.Id;
    }

    public async Task<Unit> Handle(UpdateUiPolicyCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<UiPolicy>();
        var policy = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (policy == null) throw new NotFoundException(nameof(UiPolicy), request.Id);

        policy.Name = request.Name;
        policy.Description = request.Description;
        policy.ExecutionOrder = request.ExecutionOrder;
        policy.FormContext = request.FormContext;
        policy.IsActive = request.IsActive;
        policy.Version = request.Version + 1;

        var condRepo = _uow.Repository<UiPolicyCondition>();
        var conditions = await condRepo.FindAsync(c => c.UiPolicyId == request.Id, cancellationToken);
        foreach (var c in conditions) condRepo.Delete(c);

        policy.Conditions = request.Conditions.Select(c => new UiPolicyCondition
        {
            FieldId = c.FieldId,
            Operator = c.Operator,
            Value = c.Value,
            LogicalGroup = c.LogicalGroup
        }).ToList();

        var actionRepo = _uow.Repository<UiPolicyAction>();
        var actions = await actionRepo.FindAsync(a => a.UiPolicyId == request.Id, cancellationToken);
        foreach (var a in actions) actionRepo.Delete(a);

        policy.Actions = request.Actions.Select(a => new UiPolicyAction
        {
            TargetFieldId = a.TargetFieldId,
            ActionType = a.ActionType
        }).ToList();

        repo.Update(policy);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(policy.TableId);

        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteUiPolicyCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<UiPolicy>();
        var policy = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (policy == null) throw new NotFoundException(nameof(UiPolicy), request.Id);

        repo.SoftDelete(policy);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(policy.TableId);

        return Unit.Value;
    }

    // ─── Field Rule ─────────────────────────────────────────────────────────

    public async Task<Guid> Handle(CreateFieldRuleCommand request, CancellationToken cancellationToken)
    {
        var rule = new FieldRule
        {
            Name = request.Name,
            TableId = request.TableId,
            TriggerFieldId = request.TriggerFieldId,
            ConditionJson = request.Condition.RootElement.ToString(),
            TargetFieldId = request.TargetFieldId,
            ActionType = request.ActionType,
            ValueExpression = request.ValueExpression,
            ExecutionOrder = request.ExecutionOrder,
            IsActive = request.IsActive,
            Version = 1
        };

        await _uow.Repository<FieldRule>().AddAsync(rule, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(request.TableId);

        return rule.Id;
    }

    public async Task<Unit> Handle(UpdateFieldRuleCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<FieldRule>();
        var rule = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (rule == null) throw new NotFoundException(nameof(FieldRule), request.Id);

        rule.Name = request.Name;
        rule.TriggerFieldId = request.TriggerFieldId;
        rule.ConditionJson = request.Condition.RootElement.ToString();
        rule.TargetFieldId = request.TargetFieldId;
        rule.ActionType = request.ActionType;
        rule.ValueExpression = request.ValueExpression;
        rule.ExecutionOrder = request.ExecutionOrder;
        rule.IsActive = request.IsActive;
        rule.Version = request.Version + 1;

        repo.Update(rule);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(rule.TableId);

        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteFieldRuleCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<FieldRule>();
        var rule = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (rule == null) throw new NotFoundException(nameof(FieldRule), request.Id);

        repo.SoftDelete(rule);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(rule.TableId);

        return Unit.Value;
    }

    // ─── Client Script ──────────────────────────────────────────────────────

    public async Task<Guid> Handle(CreateClientScriptCommand request, CancellationToken cancellationToken)
    {
        ValidateClientScript(request.ScriptCode);

        var script = new ClientScript
        {
            Name = request.Name,
            Description = request.Description,
            TableId = request.TableId,
            EventType = request.EventType,
            TriggerFieldId = request.TriggerFieldId,
            ScriptCode = request.ScriptCode,
            ExecutionOrder = request.ExecutionOrder,
            IsActive = request.IsActive,
            Version = 1
        };

        await _uow.Repository<ClientScript>().AddAsync(script, cancellationToken);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(request.TableId);

        return script.Id;
    }

    public async Task<Unit> Handle(UpdateClientScriptCommand request, CancellationToken cancellationToken)
    {
        ValidateClientScript(request.ScriptCode);

        var repo = _uow.Repository<ClientScript>();
        var script = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (script == null) throw new NotFoundException(nameof(ClientScript), request.Id);

        script.Name = request.Name;
        script.Description = request.Description;
        script.EventType = request.EventType;
        script.TriggerFieldId = request.TriggerFieldId;
        script.ScriptCode = request.ScriptCode;
        script.ExecutionOrder = request.ExecutionOrder;
        script.IsActive = request.IsActive;
        script.Version = request.Version + 1;

        repo.Update(script);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(script.TableId);

        return Unit.Value;
    }

    public async Task<Unit> Handle(DeleteClientScriptCommand request, CancellationToken cancellationToken)
    {
        var repo = _uow.Repository<ClientScript>();
        var script = await repo.GetByIdAsync(request.Id, cancellationToken);

        if (script == null) throw new NotFoundException(nameof(ClientScript), request.Id);

        repo.SoftDelete(script);
        await _uow.SaveChangesAsync(cancellationToken);

        InvalidateTableCache(script.TableId);

        return Unit.Value;
    }

    private void InvalidateTableCache(Guid tableId)
    {
        var contexts = new[] { "default", "create", "edit", "view", "all" };
        foreach (var ctx in contexts)
        {
            _memoryCache.Remove($"ui-metadata-{tableId}-{ctx}");
        }
    }

    private static void ValidateClientScript(string code)
    {
        if (code.Length > 10000)
            throw new BusinessException("Script exceeds maximum allowed length of 10,000 characters.");

        var restricted = new[] { "window", "document", "eval", "localStorage", "sessionStorage", "fetch", "XMLHttpRequest" };
        foreach (var keyword in restricted)
        {
            if (code.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException($"Script contains restricted keyword: {keyword}. Use the 'form' API sandbox instead.");
        }
    }
}
