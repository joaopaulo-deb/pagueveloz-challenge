

using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Common;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Contracts
{
    public interface IAccountService
    {
        Task<Response<AccountCreateOutputDto>> Create(AccountCreateInputDto model);
    }
}
