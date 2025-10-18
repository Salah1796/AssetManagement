using PlatformService.Dtos;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;

namespace PlatformService.AsyncDataServices
{
    public class RabbitMQClient(IConfiguration configuration) : IMessageBusClient, IAsyncDisposable
    {
        private readonly IConfiguration _configuration = configuration;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly string _exchangeName = "trigger";

        public async Task InitializeAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"],
                Port = int.Parse(_configuration["RabbitMQPort"])
            };
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout);

                _connection.ConnectionShutdownAsync += OnConnectionShutdown;

                Console.WriteLine("--> Connected to MessageBus");

            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
            }
        }

        public async Task PublishNewPlatform(PlatformPublishedDto platformPublishedDto)
        {
            if (_connection == null || !_connection.IsOpen)
            {
                Console.WriteLine("--> RabbitMQ connection is closed, skipping message send.");
                return;
            }

            if (_channel == null)
            {
                Console.WriteLine("--> RabbitMQ channel is not initialized yet, skipping message send.");
                return;
            }

            Console.WriteLine("--> RabbitMQ Connection Open, sending message...");

            try
            {
                var json = JsonSerializer.Serialize(platformPublishedDto);
                var body = Encoding.UTF8.GetBytes(json);

                await _channel!.BasicPublishAsync(
                    exchange: _exchangeName,
                    routingKey: string.Empty,
                    body: body
                );

                Console.WriteLine($"--> Sent async message: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Error sending RabbitMQ message: {ex.Message}");
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel != null)
            {
                await _channel.CloseAsync();
                _channel = null;
            }

            if (_connection != null)
            {
                await _connection.CloseAsync();
                _connection = null;
            }
        }

        private Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
            return Task.CompletedTask;
        }
    }
}