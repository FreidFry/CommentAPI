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
