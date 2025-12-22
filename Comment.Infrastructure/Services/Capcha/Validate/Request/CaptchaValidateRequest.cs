namespace Comment.Infrastructure.Services.Capcha.Validate.Request
{
    public record CaptchaValidateRequest(string CaptchaId, string CaptchaValue)
    {
    }
}
