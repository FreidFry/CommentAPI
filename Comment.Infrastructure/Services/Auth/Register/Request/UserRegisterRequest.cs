namespace Comment.Infrastructure.Services.Auth.Register.Request
{
    public record UserRegisterRequest(string UserName, string Email, string Password, string ConfirmPassword, string? HomePage);
}
