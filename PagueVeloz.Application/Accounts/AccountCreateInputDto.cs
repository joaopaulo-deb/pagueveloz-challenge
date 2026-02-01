
using System.ComponentModel.DataAnnotations;

namespace PagueVeloz.Application.Accounts
{
    public class AccountCreateInputDto
    {
        [Required]
        public required string ClientId { get; set; }
        [Required]
        public int CreditLimit { get; set; }
        [Required]
        public int AvailableBalance { get; set; }
    }
}
