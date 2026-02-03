
using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.Accounts
{
    public class AccountCreateInputDto
    {
        [Required(ErrorMessage = "clientId é obrigatório")]
        public required string ClientId { get; set; }

        [Required(ErrorMessage = "creditLimit é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "creditLimit deve ser 0 ou maior")]
        public int? CreditLimit { get; set; }

        [Required(ErrorMessage = "availableBalance é obrigatório")]
        [Range(0, int.MaxValue, ErrorMessage = "availableBalance deve ser 0 ou maior")]
        public int? AvailableBalance { get; set; }
    }
}
