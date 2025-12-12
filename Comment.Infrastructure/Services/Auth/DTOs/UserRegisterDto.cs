namespace Comment.Infrastructure.Services.Auth.DTOs
{
    public record UserRegisterDto(string UserName, string Email, string Password, string ConfirmPassword, string? HomePage);
}
