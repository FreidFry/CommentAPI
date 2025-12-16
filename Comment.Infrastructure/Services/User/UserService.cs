using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.DTOs.Response;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Comment.Infrastructure.Extensions.ClaimsExtensions;

namespace Comment.Infrastructure.Services.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _appDbContext;
        private readonly IFileProvider _fileStorage;
        private readonly IValidator<UserUpdateAvatarDTO> _avatarValidator;
        private readonly IMapper _mapper;

        public UserService(AppDbContext appDbContext, IFileProvider fileStorage, IValidator<UserUpdateAvatarDTO> avatarValidator, IMapper mapper)
        {
            _appDbContext = appDbContext;
            _fileStorage = fileStorage;
            _avatarValidator = avatarValidator;
            _mapper = mapper;
        }

        public async Task<IActionResult> GetByIdAsync(Guid? UserId, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(httpContext);
            if (callerId == null && UserId == null)
                return new BadRequestResult();

            var isOwner = UserId == callerId;

            var userQuery = _appDbContext.Users
                .Where(u => u.Id == UserId)
                .AsQueryable();

            if (isOwner) userQuery = userQuery.Include(u => u.Threads.OrderByDescending(t => t.CreatedAt));
            else userQuery = userQuery.Include(u => u.Threads.Where(t => !t.IsDeleted && !t.IsBanned).OrderByDescending(t => t.CreatedAt));

            var user = await _mapper.ProjectTo<CommonUserDataDTO>(userQuery)
                .FirstOrDefaultAsync(cancellationToken);
            return new OkObjectResult(user);
        }

        public async Task<IActionResult> GetCurrentAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var callerId = GetCallerId(httpContext);
            var userDto = await _appDbContext.Users.Where(u => u.Id == callerId)
                .Select(u => new CommonUserDataDTO
                {
                    UserName = u.UserName,
                    AvatarTumbnailUrl = u.AvatarTumbnailUrl,
                    CreatedAt = u.CreatedAt,
                    LastActive = u.LastActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            return new OkObjectResult(userDto);
        }

        public async Task<IActionResult> UpdateProfileAvatarAsync(UserUpdateAvatarDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _avatarValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var callerId = GetCallerId(httpContext);
            if (callerId == null)
                return new BadRequestObjectResult("Invalid user ID.");

            var user = await _appDbContext.Users.FindAsync(callerId, cancellationToken);
            if (user == null)
                return new NotFoundObjectResult("User not found.");
            if (user.Id != callerId)
                return new BadRequestObjectResult("Invalid user ID.");

            //user.AvatarUrl = GetAvatarUrlById(dto.AvatarId);

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(user);
        }
    }
}
