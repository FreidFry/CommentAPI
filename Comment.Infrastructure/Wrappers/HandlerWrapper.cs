using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Diagnostics;

namespace Comment.Infrastructure.Wrappers
{
    /// <summary>
    /// Provides a base wrapper for handling common cross-cutting concerns such as logging, 
    /// performance monitoring, and exception handling for API or message handlers.
    /// </summary>
    public abstract class HandlerWrapper
    {
        protected readonly ILogger<HandlerWrapper> _logger;

        protected HandlerWrapper(ILogger<HandlerWrapper> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// Executes a given action within a safe context, providing automated logging, 
        /// stopwatch timing, and specialized exception handling for common failure modes.
        /// </summary>
        /// <param name="action">The asynchronous operation to execute.</param>
        /// <param name="actionName">A descriptive name of the action for logging purposes.</param>
        /// <param name="requestData">Optional payload or parameters associated with the request for diagnostic tracing.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> corresponding to the operation result, or an appropriate 
        /// error response (504 for timeouts, 499 for cancellations, 500 for internal errors).
        /// </returns>
        protected async Task<IActionResult> SafeExecute(Func<Task<IActionResult>> action, string actionName, object? requestData = null)
        {
            using (_logger.BeginScope(new Dictionary<string, object> { ["ActionName"] = actionName, ["RequestData"] = requestData ?? "none"}))
            {
                var sw = Stopwatch.StartNew();
                try
                {
                    var result = await action();
                    sw.Stop();

                    if (sw.ElapsedMilliseconds > 500)
                        _logger.LogWarning("Slow Action {ActionName} completed in {ElapsedMS}ms", actionName, sw.ElapsedMilliseconds);
                    else
                        _logger.LogDebug("Action {ActionName} completed in {ElapsedMS}ms", actionName, sw.ElapsedMilliseconds);

                    return result;
                }
                catch (RequestTimeoutException ex)
                {
                    sw.Stop();
                    _logger.LogError(ex, "MassTransit Timeout in {ActionName} after {ElapsedMS}ms", actionName, sw.ElapsedMilliseconds);
                    return new ObjectResult(new { error = "Service timeout" }) { StatusCode = 504 };
                }
                catch (OperationCanceledException)
                {
                    sw.Stop();
                    _logger.LogWarning("Action {ActionName} was cancelled by user (Client Closed Connection)", actionName);
                    return new StatusCodeResult(499);
                }
                catch (Exception ex)
                {
                    sw.Stop();
                    var traceId = Activity.Current?.Id ?? "n/a";
                    _logger.LogError(ex, "Unhandled Exception in {ActionName} | TraceId: {TraceId}", actionName, traceId);
                    return new ObjectResult(new { error = "Internal error", details = ex.Message }) { StatusCode = 500 };
                }
            }
        }
    }
}
