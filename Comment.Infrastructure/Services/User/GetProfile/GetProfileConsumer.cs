using AutoMapper;
using Comment.Core.Data;
using Comment.Core.Persistence;
using Comment.Infrastructure.CommonDTOs;
using Comment.Infrastructure.Services.User.GetProfile.Request;
using Comment.Infrastructure.Services.User.GetProfile.Response;
using FluentValidation;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
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
