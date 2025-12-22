using Comment.Infrastructure.Services.Capcha.CapchaGenerate;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/captcha")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICapchaGenerateHandler _capchaGenerateHandler;


        public CaptchaController(ICapchaGenerateHandler capchaGenerateHandler)
        {
            _capchaGenerateHandler = capchaGenerateHandler;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GetCaptcha(CancellationToken cancellationToken)
        {
            return await _capchaGenerateHandler.Handle(new(), cancellationToken);
        }
    }
}
