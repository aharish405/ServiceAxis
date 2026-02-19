using ServiceAxis.Application.Contracts.Infrastructure;
using Microsoft.Extensions.Logging;

namespace ServiceAxis.Infrastructure.Services;

/// <summary>
/// Placeholder email service implementation.
/// Replace with a real provider (SMTP / SendGrid / Azure Communication Services)
/// by implementing <see cref="IEmailService"/> and updating the DI registration.
/// </summary>
internal sealed class LoggingEmailService : IEmailService
{
    private readonly ILogger<LoggingEmailService> _logger;

    public LoggingEmailService(ILogger<LoggingEmailService> logger) => _logger = logger;

    public Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL] To={To} Subject={Subject}", to, subject);
        return Task.CompletedTask;
    }

    public Task SendTemplatedAsync(
        string to, string subject, string templateName, object model,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[EMAIL-TEMPLATE] To={To} Template={Template}", to, templateName);
        return Task.CompletedTask;
    }
}

/// <summary>
/// Placeholder SMS service implementation.
/// </summary>
internal sealed class LoggingSmsService : ISmsService
{
    private readonly ILogger<LoggingSmsService> _logger;

    public LoggingSmsService(ILogger<LoggingSmsService> logger) => _logger = logger;

    public Task SendAsync(string to, string message, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("[SMS] To={To} Message={Message}", to, message);
        return Task.CompletedTask;
    }
}
