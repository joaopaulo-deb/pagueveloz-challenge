
namespace PagueVeloz.Application.Common
{
    public class Response<T> where T : class
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
