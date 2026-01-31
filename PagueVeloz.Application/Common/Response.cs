
namespace PagueVeloz.Application.Common
{
    public class Response<T> where T : class
    {
        public T? Data { get; set; }
        public string Message { get; set; } = string.Empty;

        private Response(T? data, string? message)
        {
            Data = data;
            Message = message;
        }

        public static Response<T> Ok(T data, string? message = null)
            => new(data, message);

        public static Response<T> Fail(string message)
            => new(default, message);
    }
}
