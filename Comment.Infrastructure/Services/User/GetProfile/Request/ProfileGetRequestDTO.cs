namespace Comment.Infrastructure.Services.User.GetProfile.Request
{
    public record ProfileGetRequestDTO(Guid? UserId, Guid? callerId);

}
