using Comment.Infrastructure.Services.Capcha.Validate.Request;
using Comment.Infrastructure.Services.Capcha.Validate.Response;
using MassTransit;

namespace Comment.Infrastructure.Services.Capcha.Validate
{
    public class CaptchaValidateHandler : ICaptchaValidateHandler
    {
        private readonly IRequestClient<CaptchaValidateRequest> _client;
        public CaptchaValidateHandler(IRequestClient<CaptchaValidateRequest> client)
        {
            _client = client;
        }

        public async Task<bool> Handle(CaptchaValidateRequest request)
        {
            var response = await _client.GetResponse<CaptchaValidateResponse>(request);

            if (response is Response<CaptchaValidateResponse> validResult)
                return response.Message.IsValid;
            return false;
        }
    }
}
