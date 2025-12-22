using Comment.Infrastructure.Services.Capcha.CapchaGenerate.Request;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.Capcha.CapchaGenerate
{
    public interface ICapchaGenerateHandler
    {
        Task<IActionResult> Handle(CaptchaGenerateRequest request, CancellationToken cancellationToken);
    }
}