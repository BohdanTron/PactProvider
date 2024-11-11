using System.Text;
using System.Text.Json;
using RabbitMQ.Client;

namespace Provider
{
    public interface IEventPublisher
    {
        Task Publish<T>(T message, string queue);
    }

    public class EventPublisher : IEventPublisher
    {
        private readonly IModel _channel;

        public EventPublisher()
        {
            var factory = new ConnectionFactory { HostName = "localhost" };
            var connection = factory.CreateConnection();

            _channel = connection.CreateModel();
        }

        public Task Publish<T>(T message, string queue)
        {
            _channel.QueueDeclare(queue: queue,
                durable: false,
                exclusive: false,
                autoDelete: false,
                arguments: null);

            var body = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(message));

            _channel.BasicPublish(exchange: string.Empty,
                routingKey: queue,
                basicProperties: null,
                body: body);

            return Task.CompletedTask;
        }
    }
}
