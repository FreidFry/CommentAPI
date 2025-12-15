namespace Comment.Infrastructure.Services.Auth.DTOs
{
    public record AuthInitDTO(Guid? Id, string? UserName, List<string>? Roles);
}
