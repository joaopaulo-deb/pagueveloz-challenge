using Moq;
using PagueVeloz.Application.Publisher;
using PagueVeloz.Domain.Contracts;
using PagueVeloz.Domain.Entities;
using PagueVeloz.Domain.Enums;

namespace Tests.Unit.Application
{
    public class RetryEventPublisherServiceTests
    {
        private readonly Mock<IEventPublisher> _inner = new();
        private readonly Mock<IEventRepository> _eventRepository = new();

        private RetryEventPublisher CreateSut(int maxAttempts = 3, int baseDelayMs = 1)
            => new RetryEventPublisher(
                _inner.Object,
                _eventRepository.Object,
                maxAttempts,
                baseDelayMs
            );

        private static TransactionProcessedEvent CreateMessage()
            => new TransactionProcessedEvent(
                TransactionId: "TX-1",
                AccountId: 1,
                Operation: OperationType.credit,
                Status: "success",
                Amount: 100,
                Currency: Currency.BRL,
                Timestamp: DateTime.UtcNow
            );

        [Fact]
        public async Task PublishAsync_SuccessOnFirstAttempt_CreatesSingleSuccessEvent()
        {
            var events = new List<Event>();

            _inner.Setup(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Create(It.IsAny<Event>()))
                .Callback<Event>(e => events.Add(e));

            var sut = CreateSut(maxAttempts: 3);

            await sut.PublishAsync(CreateMessage(), "queue");

            _inner.Verify(x => x.PublishAsync(It.IsAny<object>(), "queue", It.IsAny<CancellationToken>()), Times.Once);

            Assert.Single(events);

            var evt = events[0];
            Assert.Equal(EventStatus.Success, evt.Status);
            Assert.Equal(1, evt.Attempt);
            Assert.Equal("Sucesso ao publicar evento", evt.Description);
        }

        [Fact]
        public async Task PublishAsync_FailsOnce_ThenSucceeds_CreatesFailedThenSuccessEvents()
        {
            var events = new List<Event>();

            _inner.SetupSequence(x => x.PublishAsync(It.IsAny<object>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                  .ThrowsAsync(new Exception("erro"))
                  .Returns(Task.CompletedTask);

            _eventRepository
                .Setup(x => x.Create(It.IsAny<Event>()))
                .Callback<Event>(e => events.Add(e));

            var sut = CreateSut(maxAttempts: 3);

            await sut.PublishAsync(CreateMessage(), "queue");

            Assert.Equal(2, events.Count);

            Assert.Equal(EventStatus.Failed, events[0].Status);
            Assert.Equal(1, events[0].Attempt);
            Assert.Equal("Falha na publicação do evento", events[0].Description);

            Assert.Equal(EventStatus.Success, events[1].Status);
            Assert.Equal(2, events[1].Attempt);
            Assert.Equal("Sucesso após retry", events[1].Description);
        }
    }
}
