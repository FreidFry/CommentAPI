using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Interfaces;
using Comment.Infrastructure.Services.User.DTOs.Request;
using Comment.Infrastructure.Services.User.DTOs.Response;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace Comment.Infrastructure.Services.User
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _AppDbContext;
        private readonly IFileStorage _fileStorage;
        private readonly IValidator<UserUpdateAvatarDTO> _avatarValidator;
        private readonly IMapper _mapper;

        public UserService(AppDbContext appDbContext, IFileStorage fileStorage, IValidator<UserUpdateAvatarDTO> avatarValidator, IMapper mapper)
        {
            _AppDbContext = appDbContext;
            _fileStorage = fileStorage;
            _avatarValidator = avatarValidator;
            _mapper = mapper;
        }

        public async Task<CommonUserDataDTO?> GetByIdAsync(UserFindDto dto, CancellationToken cancellationToken)
        {
            await _avatarValidator.ValidateAsync(dto);
            var user = await _AppDbContext.Users
                .Where(u => u.Id == dto.UserId)
                .FirstOrDefaultAsync(cancellationToken);
            return _mapper.Map<CommonUserDataDTO>(user);
        }

        public async Task<CommonUserDataDTO?> GetCurrentAsync(HttpContext httpContext, CancellationToken cancellationToken)
        {
            var userIdClaim = httpContext.User.FindFirst("uid");
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out Guid userId))
            {
                return null;
            }

            var userDto = await _AppDbContext.Users.Where(u => u.Id == userId)
                .Select(u => new CommonUserDataDTO
                {
                    UserName = u.UserName,
                    AvatarTumbnailUrl = u.AvatarTumbnailUrl,
                    CreatedAt = u.CreatedAt,
                    LastActive = u.LastActive
                })
                .FirstOrDefaultAsync(cancellationToken);

            return userDto;
        }

        public async Task<IActionResult> UpdateProfileAvatarAsync(UserUpdateAvatarDTO dto, HttpContext httpContext, CancellationToken cancellationToken)
        {
            var validationResult = await _avatarValidator.ValidateAsync(dto, cancellationToken);
            if (!validationResult.IsValid)
                return new BadRequestObjectResult(validationResult.Errors);

            var userId = httpContext.User.FindFirst("uid")?.Value;
            if (userId == null || !Guid.TryParse(userId, out Guid userGuid))
                return new BadRequestObjectResult("Invalid user ID.");

            var user = await _AppDbContext.Users.FindAsync(userGuid, cancellationToken);
            if (user == null)
                return new NotFoundObjectResult("User not found.");
            if (user.Id != userGuid)
                return new BadRequestObjectResult("Invalid user ID.");

            user.AvatarUrl = GetAvatarUrlById(dto.AvatarId);

            _AppDbContext.Users.Update(user);
            await _AppDbContext.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(user);
        }
    }
}
