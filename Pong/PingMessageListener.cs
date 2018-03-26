using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Pong.Models;
using Microsoft.Extensions.DependencyInjection;

namespace Pong
{
    public class PingMessageListener : IDisposable
    {
        private readonly ConnectionFactory _connectionFactory;
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly EventingBasicConsumer _consumer;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public PingMessageListener(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;

            Console.WriteLine("Constructor PingMessageListener");
            _connectionFactory = new ConnectionFactory() { HostName = "localhost" };
            _connection = _connectionFactory.CreateConnection();
            _channel = _connection.CreateModel();

            _channel.QueueDeclare(queue: "PingQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
            _channel.BasicQos(0, 1, false);

            _consumer = new EventingBasicConsumer(_channel);
            _channel.BasicConsume(queue: "PingQueue", autoAck: false, consumer: _consumer);

            _consumer.Received += OnPingMessageReceived;
        }

        private async void OnPingMessageReceived(object mode, BasicDeliverEventArgs eventArgs)
        {
            Console.WriteLine("PingMessage recibido.");
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<PingPongContext>();
                context.PingMessages.Add(new PingMessage { Date = DateTime.Now });
                context.SaveChanges();
            }

            string response = null;

            var body = eventArgs.Body;
            var properties = eventArgs.BasicProperties;
            var replyProperties = _channel.CreateBasicProperties();
            replyProperties.CorrelationId = properties.CorrelationId;

            try
            {
                var message = Encoding.UTF8.GetString(body);

                await Task.Delay(2000);

                response = "PongMessage";
            }
            catch (Exception e)
            {
                response = "Error";
            }
            finally
            {
                SendResponse(response, properties, replyProperties);
                SendAck(eventArgs.DeliveryTag);
            }
        }

        private void SendResponse(string response, IBasicProperties properties, IBasicProperties replyProperties)
        {
            if (response == "PongMessage")
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var context = scope.ServiceProvider.GetRequiredService<PingPongContext>();
                    context.PongMessages.Add(new PongMessage { Date = DateTime.Now });
                    context.SaveChanges();
                }
            }

            var responseBytes = Encoding.UTF8.GetBytes(response);
            _channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo, basicProperties: replyProperties, body: responseBytes);
            Console.WriteLine("Respuesta enviada.");
        }

        private void SendAck(ulong deliveryTag)
        {
            _channel.BasicAck(deliveryTag: deliveryTag, multiple: false);
        }

        public void Dispose()
        {
            _connection.Close();
        }
    }
}