namespace PagueVeloz.Domain.Entities
{
    public sealed class Client : BaseEntity
    {
        public string Code { get; private set; }

        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts;

        public Client(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ApplicationException("Client_id é obrigatório");

            Code = code;
        }
    }
}
