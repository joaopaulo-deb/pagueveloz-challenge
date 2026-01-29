namespace PagueVeloz.Domain.Entities
{
    public sealed class Customer : BaseEntity
    {
        public required string Name { get; set; }
        public required string Email { get; set; }

    }
}
