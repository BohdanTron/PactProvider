namespace Provider.Contract.Tests.Fakes
{
    public class EventPublisherFake : IEventPublisher
    {
        public Task Publish<T>(T message, string queue)
        {
            return Task.CompletedTask;
        }
    }
}
