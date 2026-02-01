using PagueVeloz.Application.Common;
using PagueVeloz.Application.Contracts;
using PagueVeloz.Domain.Entities;

namespace PagueVeloz.Application.Clients
{
    public class ClientService : IClientService
    {
        private readonly IClientRepository _clientRepository;

        public ClientService(IClientRepository clientRepository)
        {
            _clientRepository = clientRepository;
        }

        public async Task<Response<ClientsCreateOutputDto>> Create(ClientsCreateInputDto input)
        {
           try
           {
                var client = new Client(input.ClientId);

                _clientRepository.Create(client);
                await _clientRepository.SaveChangesAsync();

                return Response<ClientsCreateOutputDto>.Ok(ToOutputDto(client), "Cliente criado com sucesso");
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Erro ao criar cliente", ex);
            }
        }

        private ClientsCreateOutputDto ToOutputDto(Client client)
        {
            return new ClientsCreateOutputDto
            {
                ClientId = client.Code
            };
        }
    }
}
