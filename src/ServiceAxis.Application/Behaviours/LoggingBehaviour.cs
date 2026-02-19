using MediatR;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace ServiceAxis.Application.Behaviours;

/// <summary>
/// MediatR pipeline behaviour that logs the start, end, and duration of every command/query.
/// Long-running operations (>500 ms) are emitted as warnings.
/// </summary>
public sealed class LoggingBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehaviour<TRequest, TResponse>> _logger;

    public LoggingBehaviour(ILogger<LoggingBehaviour<TRequest, TResponse>> logger) =>
        _logger = logger;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var sw = Stopwatch.StartNew();

        _logger.LogInformation("ServiceAxis — Handling {RequestName}", requestName);

        try
        {
            var response = await next();
            sw.Stop();

            if (sw.ElapsedMilliseconds > 500)
                _logger.LogWarning("ServiceAxis — Long-running request {RequestName} took {Elapsed} ms", requestName, sw.ElapsedMilliseconds);
            else
                _logger.LogInformation("ServiceAxis — Handled {RequestName} in {Elapsed} ms", requestName, sw.ElapsedMilliseconds);

            return response;
        }
        catch
        {
            sw.Stop();
            _logger.LogError("ServiceAxis — {RequestName} failed after {Elapsed} ms", requestName, sw.ElapsedMilliseconds);
            throw;
        }
    }
}
