using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.Clients
{
    public class ClientsCreateInputDto
    {
        [Required]
        public string ClientId { get; set; }
    }
}
