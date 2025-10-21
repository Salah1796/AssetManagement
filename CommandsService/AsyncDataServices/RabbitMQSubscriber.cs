using CommandsService.EventProcessing;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;

namespace CommandsService.AsyncDataServices
{
    public class RabbitMQSubscriber(IConfiguration configuration, IEventProcessor eventProcessor) : BackgroundService, IAsyncDisposable
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly IEventProcessor _eventProcessor = eventProcessor;
        private IConnection? _connection;
        private IChannel? _channel;
        private string? _queueName;
        private readonly string _exchangeName = "trigger";

        private async Task<bool> InitializeAsync()
        {
            var factory = new ConnectionFactory()
            {
                HostName = _configuration["RabbitMQHost"]!,
                Port = int.Parse(_configuration["RabbitMQPort"]!)
            };
            try
            {
                _connection = await factory.CreateConnectionAsync();
                _channel = await _connection.CreateChannelAsync();

                await _channel.ExchangeDeclareAsync(exchange: _exchangeName, type: ExchangeType.Fanout);

                var queueDeclareOk = await _channel.QueueDeclareAsync();
                _queueName = queueDeclareOk.QueueName;

                await _channel.QueueBindAsync(queue: _queueName, exchange: _exchangeName, routingKey: string.Empty);

                _connection.ConnectionShutdownAsync += OnConnectionShutdown;

                Console.WriteLine("--> Listenting on the Message Bus...");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"--> Could not connect to the Message Bus: {ex.Message}");
                return false;
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (await InitializeAsync())
            {
                var consumer = new AsyncEventingBasicConsumer(_channel!);

                consumer.ReceivedAsync += ReceivedAsync;

                await _channel!.BasicConsumeAsync(
                    queue: _queueName!,
                    autoAck: true,
                    consumer: consumer,
                    cancellationToken: stoppingToken);
            }
        }

        private Task ReceivedAsync(object sender, BasicDeliverEventArgs basicDeliverEventArgs)
        {
            Console.WriteLine("--> Event Received!");

            var body = basicDeliverEventArgs.Body;
            var notificationMessage = Encoding.UTF8.GetString(body.ToArray());

            _eventProcessor.ProcessEvent(notificationMessage);

            return Task.CompletedTask;
        }

        private Task OnConnectionShutdown(object sender, ShutdownEventArgs args)
        {
            Console.WriteLine("--> RabbitMQ Connection Shutdown");
            return Task.CompletedTask;
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
            base.Dispose();
        }
    }
}