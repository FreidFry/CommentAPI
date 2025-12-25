using Comment.Infrastructure.Services.Capcha.CapchaGenerate.Request;
using Comment.Infrastructure.Services.Capcha.CapchaGenerate.Response;
using Comment.Infrastructure.Wrappers;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Capcha.CapchaGenerate
{
    public class CapchaGenerateHandler : HandlerWrapper, ICapchaGenerateHandler
    {
        private readonly IRequestClient<CaptchaGenerateRequest> _client;
        public CapchaGenerateHandler(IRequestClient<CaptchaGenerateRequest> client)
        {
            _client = client;
        }

        public async Task<IActionResult> Handle(CaptchaGenerateRequest request, CancellationToken cancellationToken)
        => await SafeExecute(async () =>
        {
            var response = await _client.GetResponse<CaptchaGenerateResponse>(request, cancellationToken);
            if (response is Response<CaptchaGenerateResponse> capcha) return new OkObjectResult(capcha.Message);
            return new ObjectResult(new { error = "Unexpected service response" }) { StatusCode = 502 };
        });
    }
}
