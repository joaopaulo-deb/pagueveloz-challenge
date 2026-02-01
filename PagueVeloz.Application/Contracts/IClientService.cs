using PagueVeloz.Application.Common;
using PagueVeloz.Application.Clients;

namespace PagueVeloz.Application.Contracts
{
    public interface IClientService
    {
        Task<Response<ClientsCreateOutputDto>> Create(ClientsCreateInputDto input);
    }
}
