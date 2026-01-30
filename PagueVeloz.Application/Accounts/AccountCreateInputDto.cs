
namespace PagueVeloz.Application.Accounts
{
    public class AccountCreateInputDto
    {
        public required string ClientId { get; set; }
        public int CreditLimit { get; set; }
        public int AvailableBalance { get; set; }
    }
}
