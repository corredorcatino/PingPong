using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Ping
{
    public class PingClient
    {
        private readonly IConnection connection;
        private readonly IModel channel;
        private readonly string pongQueueName;
        private readonly EventingBasicConsumer consumer;
        private readonly IBasicProperties properties;
        private string pongMessage;

        public PingClient()
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            connection = factory.CreateConnection();
            channel = connection.CreateModel();
            pongQueueName = channel.QueueDeclare().QueueName;
            consumer = new EventingBasicConsumer(channel);

            properties = channel.CreateBasicProperties();
            var correlationId = Guid.NewGuid().ToString();
            properties.CorrelationId = correlationId;
            properties.ReplyTo = pongQueueName;

            consumer.Received += (model, ea) =>
            {
                var body = ea.Body;
                var response = Encoding.UTF8.GetString(body);
                if (ea.BasicProperties.CorrelationId == correlationId)
                {
                    pongMessage = response;
                }                
            };
        }

        public string Call()
        {
            var pingMessageBytes = Encoding.UTF8.GetBytes("PingMessage");
            channel.BasicPublish(exchange: "", routingKey: "PingQueue", basicProperties: properties, body: pingMessageBytes);

            channel.BasicConsume(consumer: consumer, queue: pongQueueName, autoAck: true);

            while (string.IsNullOrEmpty(pongMessage))
            {
                Thread.Sleep(1000);
            }
            
            return pongMessage;
        }

        public void Close()
        {
            connection.Close();
        }
    }
}