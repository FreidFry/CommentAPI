using Comment.Infrastructure.Services.Capcha.CapchaGenerate;
using Comment.Infrastructure.Services.Capcha.Validate;
using Microsoft.AspNetCore.Mvc;

namespace CommentAPI.Controllers
{
    [ApiController]
    [Route("api/captcha")]
    public class CaptchaController : ControllerBase
    {
        private readonly ICapchaGenerateHandler _capchaGenerateHandler;
        private readonly ICaptchaValidateHandler _validateCaptchaHandler;


        public CaptchaController(ICapchaGenerateHandler capchaGenerateHandler, ICaptchaValidateHandler validateCaptchaHandler)
        {
            _capchaGenerateHandler = capchaGenerateHandler;
            _validateCaptchaHandler = validateCaptchaHandler;
        }

        [HttpGet("generate")]
        public async Task<IActionResult> GetCaptcha(CancellationToken cancellationToken)
        {
            return await _capchaGenerateHandler.Handle(new(), cancellationToken);
        }
    }
}
