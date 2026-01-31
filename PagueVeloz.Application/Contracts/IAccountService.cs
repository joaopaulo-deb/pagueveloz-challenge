

using PagueVeloz.Application.Accounts;
using PagueVeloz.Application.Common;

namespace PagueVeloz.Application.Contracts
{
    public interface IAccountService
    {
        Task<Response<AccountCreateOutputDto>> Create(AccountCreateInputDto model);
    }
}
