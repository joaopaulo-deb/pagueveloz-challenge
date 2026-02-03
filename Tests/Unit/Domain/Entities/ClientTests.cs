using PagueVeloz.Domain.Entities;

namespace Tests.Unit.Domain.Entities
{
    public sealed class ClientTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Ctor_WhenCodeIsNullOrWhitespace_ThrowsApplicationException(string? code)
        {
            var ex = Assert.Throws<ApplicationException>(() => new Client(code!));
            Assert.Equal("Client_id é obrigatório", ex.Message);
        }

        [Fact]
        public void Ctor_WhenCodeIsValid_SetsCode()
        {
            var client = new Client("CLI-123");
            Assert.Equal("CLI-123", client.Code);
        }

        [Fact]
        public void Accounts_InitiallyEmpty()
        {
            var client = new Client("CLI-123");
            Assert.NotNull(client.Accounts);
            Assert.Empty(client.Accounts);
        }
    }
}
