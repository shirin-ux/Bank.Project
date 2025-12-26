using RabbitMQ.Client;
using System.Text.Json;
using System.Text.Json.Serialization;
using LoanService.Application.Contracts;
using LoanService.Domain.Entities.Loan;
using Microsoft.Extensions.Options;



namespace LoanService.Infrastructure.Services
{
    public class RabbitMqLoanNotificationBus : ILoanNotificationBus, IDisposable
    {
        private readonly RabbitMqOptions _options;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly JsonSerializerOptions _jsonOptions;
        public RabbitMqLoanNotificationBus(IOptions<RabbitMqOptions> options)
        {
           _options=options.Value;
            var factory = new ConnectionFactory()
            {
              
                Port = _options.Port,
                HostName = _options.HostName,
                VirtualHost = _options.VirtualHost,
                UserName = _options.UserName,
                Password = _options.Password,
                DispatchConsumersAsync = true
            };
            //_connection = factory.CreateConnection();   
            //_channel = _connection.CreateModel();

            //_channel.ExchangeDeclare(exchange: _options.ExchangeName, type: _options.ExchangeType, durable: true, autoDelete: false);

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
        public void Dispose()
        {
            try
            {
                _channel?.Close();
                _channel?.Dispose();
            }
            catch { }

            try
            {
                _connection?.Close();
                _connection?.Dispose();
            }
            catch { }
        }

        public Task PublishAsync(LoanNotificationMessage message, CancellationToken ct)
        {
            if (message == null)
                throw new ArgumentException(nameof(message));

            if (string.IsNullOrEmpty(message.EventType))
                throw new ArgumentException("");

            var routingKey = $"loan.{message.EventType}".ToLowerInvariant();

            if (ct.IsCancellationRequested)
                      return Task.FromCanceled(ct);

            var body = JsonSerializer.SerializeToUtf8Bytes(message, _jsonOptions);

            var props = _channel.CreateBasicProperties();
            props.ContentType = "application/json";
            props.DeliveryMode = 2;
            props.MessageId = Guid.NewGuid().ToString("N");
            props.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

            _channel.BasicPublish(
                exchange: _options.ExchangeName,
                routingKey: routingKey,
                basicProperties: props,
                body: body
                );
            return Task.CompletedTask;
        }
    }
}
