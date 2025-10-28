using RabbitMQ.Client;
using System.Text;
using System.Text.Json;
using SalesService.Models;

namespace SalesService.Services
{
    public class MessageProducerService : IDisposable
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private const string ExchangeName = "ecommerce_exchange";

        public MessageProducerService(IConfiguration configuration)
        {
            var factory = new ConnectionFactory()
            {
                HostName = configuration["RABBITMQ_HOST"] ?? "localhost",
                Port = int.Parse(configuration["RABBITMQ_PORT"] ?? "5672"),
                UserName = configuration["RABBITMQ_USER"] ?? "guest",
                Password = configuration["RABBITMQ_PASS"] ?? "guest"
            };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            // Declare a durable exchange
            _channel.ExchangeDeclare(exchange: ExchangeName, type: ExchangeType.Direct, durable: true);
        }

        public void PublishOrderCreated(Order order)
        {
            var eventData = new OrderCreatedEvent
            {
                OrderId = order.Id,
                CustomerName = order.CustomerName,
                Items = order.Items.Select(item => new OrderItemEvent
                {
                    ProductId = item.ProductId,
                    Quantity = item.Quantity
                }).ToList()
            };

            var message = JsonSerializer.Serialize(eventData);
            var body = Encoding.UTF8.GetBytes(message);

            _channel.BasicPublish(
                exchange: ExchangeName,
                routingKey: "order.created", // Routing key for order creation
                basicProperties: null,
                body: body);
        }

        public void Dispose()
        {
            _channel?.Close();
            _connection?.Close();
        }
    }
}
