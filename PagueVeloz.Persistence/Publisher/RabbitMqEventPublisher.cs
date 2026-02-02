using PagueVeloz.Application.Publisher;
using RabbitMQ.Client;
using System.Text;
using System.Text.Json;

namespace PagueVeloz.Repository.Publisher
{
    public class RabbitMqEventPublisher : IEventPublisher
    {
        private readonly IConnection _connection;

        public RabbitMqEventPublisher(IConnection connection)
        {
            _connection = connection;
        }

        public async Task PublishAsync<T>(T message, string queueOrExchange, CancellationToken ct = default)
        {
            await using var channel = await _connection.CreateChannelAsync();
            await channel.QueueDeclareAsync(
                queue: queueOrExchange,
                durable: true,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            var props = new BasicProperties
            {
                DeliveryMode = DeliveryModes.Persistent,
                ContentType = "application/json"
            };

            await channel.BasicPublishAsync(
                exchange: "",
                routingKey: queueOrExchange,
                mandatory: false,
                basicProperties: props,
                body: body);
        }
    }
}
