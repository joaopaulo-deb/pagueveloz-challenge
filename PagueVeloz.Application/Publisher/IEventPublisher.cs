namespace PagueVeloz.Application.Publisher
{
    public interface IEventPublisher
    {
        Task PublishAsync<T>(T message, string queueOrExchange, CancellationToken ct = default);
    }
}
