namespace ServiceAxis.Application.Contracts.Infrastructure;

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

public interface ISmsService
{
    Task SendAsync(string to, string message, CancellationToken ct = default);
}
