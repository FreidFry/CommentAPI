using Comment.Infrastructure.Services.User.GetProfile.Request;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Comment.Infrastructure.Services.User.GetProfile
{
    public interface IGetProfileHandler
    {
        Task<IActionResult> Handle(ProfileGetRequest request, HttpContext httpContext, CancellationToken cancellationToken);
    }
}