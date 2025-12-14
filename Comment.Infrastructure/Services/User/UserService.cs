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
        private readonly AppDbContext _appDbContext;
        private readonly IFileProvider _fileStorage;
        private readonly IValidator<UserUpdateAvatarDTO> _avatarValidator;
        private readonly IValidator<UserFindDto> _userFindValidator;
        private readonly IMapper _mapper;

        public UserService(AppDbContext appDbContext, IFileProvider fileStorage, IValidator<UserUpdateAvatarDTO> avatarValidator,  IMapper mapper, IValidator<UserFindDto> userFindValidator)
        {
            _appDbContext = appDbContext;
            _fileStorage = fileStorage;
            _avatarValidator = avatarValidator;
            _mapper = mapper;
            _userFindValidator = userFindValidator;
        }

        public async Task<CommonUserDataDTO?> GetByIdAsync(UserFindDto dto, CancellationToken cancellationToken)
        {
            await _userFindValidator.ValidateAsync(dto);
            var user = await _appDbContext.Users
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

            var userDto = await _appDbContext.Users.Where(u => u.Id == userId)
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

            var user = await _appDbContext.Users.FindAsync(userGuid, cancellationToken);
            if (user == null)
                return new NotFoundObjectResult("User not found.");
            if (user.Id != userGuid)
                return new BadRequestObjectResult("Invalid user ID.");

            //user.AvatarUrl = GetAvatarUrlById(dto.AvatarId);

            _appDbContext.Users.Update(user);
            await _appDbContext.SaveChangesAsync(cancellationToken);
            return new OkObjectResult(user);
        }
    }
}
