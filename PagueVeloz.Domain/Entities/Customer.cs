namespace PagueVeloz.Domain.Entities
{
    public sealed class Customer : BaseEntity
    {
        public string Code { get; private set; }

        private readonly List<Account> _accounts = new();
        public IReadOnlyCollection<Account> Accounts => _accounts;

        public Customer(string code)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ApplicationException("Client_id é obrigatório");

            Code = code;
        }
    }
}
