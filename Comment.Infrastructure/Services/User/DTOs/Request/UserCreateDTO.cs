namespace Comment.Infrastructure.Services.User.DTOs.Request
{
    public record UserCreateDTO(string UserName, string Email, string HomePage, string HashPassword);
}
