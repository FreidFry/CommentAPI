using Comment.Infrastructure.Services.Capcha.Validate.Request;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Capcha.Validate
{
    public interface ICaptchaValidateHandler
    {
        Task<bool> Handle(CaptchaValidateRequest request);
    }
}