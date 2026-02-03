using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

namespace PagueVeloz.Application.Publisher
{
    public class RetryEventPublisher : IEventPublisher
    {
        private readonly IEventPublisher _inner;
        public IEventRepository _eventRepository;
        private readonly int _maxAttempts;
        private readonly int _baseDelayMs;

        public RetryEventPublisher(IEventPublisher inner, IEventRepository eventRepository, int maxAttempts = 5, int baseDelayMs = 200)
        {
            _inner = inner;
            _eventRepository = eventRepository;
            _maxAttempts = maxAttempts;
            _baseDelayMs = baseDelayMs;
        }

        public async Task PublishAsync<T>(T message, string queueOrExchange, CancellationToken ct = default)
        {
            var operation = message is TransactionProcessedEvent evt ? evt.Operation : default;

            for (var attempt = 1; attempt <= _maxAttempts; attempt++)
            {
                try
                {
                    //Simulção de retry
                    /*if (attempt < 2)
                    {
                        throw new Exception("Erro");
                    }*/
                    await _inner.PublishAsync(message, queueOrExchange, ct);
                    

                    var successEvent = new Event(
                        operation,
                        attempt,
                        EventStatus.Success,
                        attempt == 1 ? "Sucesso ao publicar evento" : "Sucesso após retry"
                    );

                    _eventRepository.Create(successEvent);
                    return;
                }
                catch (Exception ex)
                {
                    var explanation = attempt switch
                    {
                        1 => "Falha na publicação do evento",
                        _ when attempt < _maxAttempts => "Retry com backoff",
                        _ => "Falha após esgotar tentativas"
                    };

                    var failedEvent = new Event(
                        operation,
                        attempt,
                        EventStatus.Failed,
                        explanation
                    );

                    _eventRepository.Create(failedEvent);

                    if (attempt < _maxAttempts)
                    {
                        var delayMs = _baseDelayMs * (int)Math.Pow(2, attempt - 1);
                        await Task.Delay(delayMs, ct);
                        continue;
                    }

                    return;
                } 
            }
        }
    }
}
