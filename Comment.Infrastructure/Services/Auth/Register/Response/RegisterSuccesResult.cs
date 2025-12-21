using Comment.Core.Persistence;
using System;
using System.Collections.Generic;
using System.Text;

namespace Comment.Infrastructure.Services.Auth.Register.Response
{
    public record RegisterSuccesResult(Guid Id, string UserName, List<string> Roles, UserModel UserModel)
    {
    }
}
