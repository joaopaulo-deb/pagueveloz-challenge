
namespace PagueVeloz.Application.Common
{
    public class ConcurrencyConflictException : Exception
    {
        public ConcurrencyConflictException()
            : base("Conflito de concorrência")
        {
        }

        public ConcurrencyConflictException(string message)
            : base(message)
        {
        }
        public ConcurrencyConflictException(string message, Exception innerException)
        : base(message, innerException)
        {
        }
    }

}
