using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Comment.Infrastructure.Services.User.GetProfile.Response;
using FluentValidation;
using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace Comment.Infrastructure.Services.User.GetProfile
{
    public class GetProfileConsumer : IConsumer<ProfileGetRequestDTO>
    {
        private readonly AppDbContext _appDbContext;
        private readonly IMapper _mapper;
        private readonly IValidator<ProfileGetRequestDTO> _validator;
        public GetProfileConsumer(AppDbContext appDbContext, IMapper mapper, IValidator<ProfileGetRequestDTO> validator)
        {
            _appDbContext = appDbContext;
            _mapper = mapper;
            _validator = validator;
        }

        /// <summary>
        /// Retrieves detailed profile information for a specific user.
        /// Implements access control logic based on whether the requester is the profile owner.
        /// </summary>
        /// <param name="context">The consume context containing the <see cref="ProfileGetRequestDTO"/>.</param>
        /// <returns>
        /// A <see cref="ProfileDetailedResponse"/> if the user is found and accessible, 
        /// or a <see cref="StatusCodeResponse"/> (404) if the user is missing or restricted.
        /// </returns>
        /// <remarks>
        /// Access Control Policy:
        /// <list type="bullet">
        /// <item>
        /// <term>Owner Mode:</term>
        /// <description>If <c>callerId</c> matches <c>UserId</c>, the profile is returned regardless of its <c>IsDeleted</c> or <c>IsBanned</c> status.</description>
        /// </item>
        /// <item>
        /// <term>Public Mode:</term>
        /// <description>For third-party requests, the profile is only visible if the user is active (not banned and not deleted).</description>
        /// </item>
        /// <item>
        /// <term>Projection:</term>
        /// <description>Uses <c>ProjectTo</c> to fetch only the necessary fields for the detailed profile view directly from the database.</description>
        /// </item>
        /// </list>
        /// </remarks>
        public async Task Consume(ConsumeContext<ProfileGetRequestDTO> context)
        {
            var dto = context.Message;
            var cancellationToken = context.CancellationToken;
            var isValid = await _validator.ValidateAsync(dto);
            if (!isValid.IsValid)
            {
                await context.RespondAsync(new ValidatorErrorResponse(isValid.Errors));
                return;
            }
            var isOwner = dto.callerId == dto.UserId;
            IQueryable<UserModel> userQuery = _appDbContext.Users;

            if (isOwner)
                userQuery = userQuery.Where(u => u.Id == dto.UserId);
            else
                userQuery = userQuery
                    .Where(u => u.Id == dto.UserId && !u.IsDeleted && !u.IsBanned)
                    .Where(t => !t.IsDeleted && !t.IsBanned);

            var profile = await _mapper.ProjectTo<ProfileDetailedResponse>(userQuery)
                .FirstOrDefaultAsync(cancellationToken);
            if (profile != null)
            {
                await context.RespondAsync(profile);
                return;
            }

            await context.RespondAsync(new StatusCodeResponse("User not found.", 404));
        }
    }
}
