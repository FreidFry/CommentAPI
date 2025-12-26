using Comment.Infrastructure.Services.Capcha.Validate.Request;
using Comment.Infrastructure.Services.Capcha.Validate.Response;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;

namespace Comment.Infrastructure.Services.Capcha.Validate
{
    public class ValidateCaptchaConsumer : IConsumer<CaptchaValidateRequest>
    {
        private readonly IDistributedCache _cache;
        public ValidateCaptchaConsumer(IDistributedCache cache)
        {
            _cache = cache;
        }

        /// <summary>
        /// Validates the user-provided CAPTCHA code against the value stored in the distributed cache.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="CaptchaValidateRequest"/>.</param>
        /// <returns>
        /// A <see cref="CaptchaValidateResponse"/> where the result is <c>true</c> if the codes match; otherwise, <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Validation Logic:
        /// <list type="bullet">
        /// <item>Verifies that both <c>CaptchaId</c> and <c>CaptchaValue</c> are provided.</item>
        /// <item>Retrieves the expected code from Redis using the provided ID.</item>
        /// <item>Immediately removes the code from the cache to prevent replay attacks (One-Time Use policy).</item>
        /// <item>Performs a case-insensitive comparison of the values.</item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<CaptchaValidateRequest> context)
        {
            if (string.IsNullOrWhiteSpace(context.Message.CaptchaId) || string.IsNullOrWhiteSpace(context.Message.CaptchaValue))
            {
                await context.RespondAsync(new CaptchaValidateResponse(false));
                return;
            }

            var cacheKey = $"captcha:{context.Message.CaptchaId}";
            var actualCode = await _cache.GetStringAsync(cacheKey);

            if (actualCode == null)
            {
                await context.RespondAsync(new CaptchaValidateResponse(false));
                return;
            }

            await _cache.RemoveAsync(cacheKey);

            var result = actualCode.Equals(context.Message.CaptchaValue, StringComparison.OrdinalIgnoreCase);
                await context.RespondAsync(new CaptchaValidateResponse(result));
        }
    }
}
