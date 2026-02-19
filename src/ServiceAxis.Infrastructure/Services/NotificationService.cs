using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ServiceAxis.Application.Contracts.Infrastructure;
using ServiceAxis.Domain.Entities.Notifications;
using ServiceAxis.Domain.Enums;
using ServiceAxis.Infrastructure.Persistence;

namespace ServiceAxis.Infrastructure.Services;

public class NotificationService : INotificationService
{
    private readonly ServiceAxisDbContext _db;
    private readonly IEmailService _emailService;
    private readonly ISmsService _smsService;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        ServiceAxisDbContext db,
        IEmailService emailService,
        ISmsService smsService,
        ILogger<NotificationService> logger)
    {
        _db = db;
        _emailService = emailService;
        _smsService = smsService;
        _logger = logger;
    }

    public async Task SendAsync(
        string templateCode,
        IReadOnlyDictionary<string, string> variables,
        IEnumerable<string> recipients,
        NotificationChannelType channel = NotificationChannelType.Email,
        Guid? recordId = null,
        CancellationToken ct = default)
    {
        var template = await _db.NotificationTemplates
            .FirstOrDefaultAsync(t => t.Name == templateCode && t.IsActive, ct);

        if (template == null)
        {
            _logger.LogError("Notification template '{TemplateCode}' not found or inactive.", templateCode);
            return;
        }

        string subject = ReplaceVariables(template.Subject, variables);
        string body = ReplaceVariables(template.Body, variables);

        await DispatchAsync(subject, body, recipients, channel, recordId, ct);
    }

    public async Task SendDirectAsync(
        string subject,
        string body,
        IEnumerable<string> recipients,
        NotificationChannelType channel = NotificationChannelType.Email,
        CancellationToken ct = default)
    {
        await DispatchAsync(subject, body, recipients, channel, null, ct);
    }

    public async Task ProcessPendingAsync(CancellationToken ct = default)
    {
        var pendingLogs = await _db.NotificationLogs
            .Where(x => x.Status == NotificationStatus.Pending)
            .OrderBy(x => x.CreatedAt)
            .Take(50)
            .ToListAsync(ct);

        foreach (var log in pendingLogs)
        {
            try
            {
                // Simple re-dispatch logic based on log content
                var recipients = log.Recipients.Split(',', StringSplitOptions.RemoveEmptyEntries);
                foreach (var recipient in recipients)
                {
                    if (log.Channel == NotificationChannelType.Email)
                    {
                        await _emailService.SendAsync(recipient, log.Subject, log.Body ?? string.Empty, ct);
                    }
                    else if (log.Channel == NotificationChannelType.Sms)
                    {
                        await _smsService.SendAsync(recipient, log.Body ?? string.Empty, ct);
                    }
                }
                
                log.Status = NotificationStatus.Sent;
                log.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing pending notification {LogId}", log.Id);
                log.Status = NotificationStatus.Failed;
                log.ErrorMessage = ex.Message;
                // Increment attempt count
                log.AttemptCount++;
            }
        }
        
        await _db.SaveChangesAsync(ct);
    }

    private async Task DispatchAsync(
        string subject,
        string body,
        IEnumerable<string> recipients,
        NotificationChannelType channel,
        Guid? recordId,
        CancellationToken ct)
    {
        foreach (var recipient in recipients)
        {
            var log = new NotificationLog
            {
                Recipients = recipient, // Store single recipient per log for this loop
                Channel = channel,
                Subject = subject,
                Body = body,
                Status = NotificationStatus.Pending,
                RecordId = recordId,
                CreatedAt = DateTime.UtcNow
            };

            _db.NotificationLogs.Add(log);
            
            try
            {
                if (channel == NotificationChannelType.Email)
                {
                    await _emailService.SendAsync(recipient, subject, body, ct);
                }
                else if (channel == NotificationChannelType.Sms)
                {
                    await _smsService.SendAsync(recipient, body, ct);
                }

                log.Status = NotificationStatus.Sent;
                log.SentAt = DateTime.UtcNow;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send notification to {Recipient}", recipient);
                log.Status = NotificationStatus.Failed;
                log.ErrorMessage = ex.Message;
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private static string ReplaceVariables(string template, IReadOnlyDictionary<string, string> variables)
    {
        if (string.IsNullOrEmpty(template)) return string.Empty;
        
        var result = template;
        foreach (var kvp in variables)
        {
            result = result.Replace($"{{{{{kvp.Key}}}}}", kvp.Value);
        }
        return result;
    }
}
