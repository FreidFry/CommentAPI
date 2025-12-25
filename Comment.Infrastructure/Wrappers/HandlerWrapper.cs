using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Wrappers
{
    public abstract class HandlerWrapper
    {
        protected async Task<IActionResult> SafeExecute(Func<Task<IActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (RequestTimeoutException)
            {
                return new ObjectResult(new { error = "Service timeout" }) { StatusCode = 504 };
            }
            catch (OperationCanceledException)
            {
                return new StatusCodeResult(499);
            }
            catch (Exception ex)
            {
                return new ObjectResult(new { error = "Internal error", details = ex.Message }) { StatusCode = 500 };
            }
        }
    }
}
