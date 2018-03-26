using System;
using System.Text;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ping
{
    public class PingMessageClient
    {
        private readonly IConnection _connection;
        private readonly IModel _channel;
        private readonly string _pongQueueName;
        private readonly EventingBasicConsumer _consumer;
        private readonly IBasicProperties _properties;
        private string _pongMessage;
        private readonly string _correlationId;

        public PingMessageClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();
            _pongQueueName = _channel.QueueDeclare().QueueName;
            _consumer = new EventingBasicConsumer(_channel);

            _properties = _channel.CreateBasicProperties();
            _correlationId = Guid.NewGuid().ToString();
            _properties.CorrelationId = _correlationId;
            _properties.ReplyTo = _pongQueueName;

            _consumer.Received += OnResponseReceived;
        }

        public async Task<string> SendPingMessage()
        {
            var pingMessageBytes = Encoding.UTF8.GetBytes("PingMessage");
            _channel.BasicPublish(exchange: "", routingKey: "PingQueue", basicProperties: _properties, body: pingMessageBytes);

            _channel.BasicConsume(consumer: _consumer, queue: _pongQueueName, autoAck: true);

            await WaitForResponse();

            return _pongMessage;
        }

        private async Task WaitForResponse()
        {
            int waitingIntervals = 10;
            int numberOfRetries = 0;
            while (string.IsNullOrEmpty(_pongMessage))
            {
                if (numberOfRetries == waitingIntervals)
                {
                    _pongMessage = "Tiempo de espera agotado";
                    break;
                }

                numberOfRetries++;
                await Task.Delay(1000);
            }
        }

        private void OnResponseReceived(object model, BasicDeliverEventArgs eventArgs)
        {
            var body = eventArgs.Body;
            var response = Encoding.UTF8.GetString(body);
            if (eventArgs.BasicProperties.CorrelationId == _correlationId)
            {
                Console.WriteLine("Respuesta recibida.");
                _pongMessage = response;
            }
        }

        public void Close()
        {
            _connection.Close();
        }
    }
}