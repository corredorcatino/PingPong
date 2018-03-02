using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace PongConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };
            using (var connection = factory.CreateConnection())
            using (var channel = connection.CreateModel())
            {
                channel.QueueDeclare(queue: "PingQueue", durable: false, exclusive: false, autoDelete: false, arguments: null);
                channel.BasicQos(0, 1, false);

                var consumer = new EventingBasicConsumer(channel);
                channel.BasicConsume(queue: "PingQueue", autoAck: false, consumer: consumer);

                Console.WriteLine("Esperando peticiones...");

                consumer.Received += (model, ea) =>
                {
                    string response = null;

                    var body = ea.Body;
                    var properties = ea.BasicProperties;
                    var replyProperties = channel.CreateBasicProperties();
                    replyProperties.CorrelationId = properties.CorrelationId;

                    try
                    {
                        var message = Encoding.UTF8.GetString(body);
                        Console.WriteLine($"Mensaje: {message}");

                        Thread.Sleep(2000);

                        response = "PongMessage";
                    }
                    catch (System.Exception e)
                    {
                        Console.WriteLine($"Error: {e.Message}");
                        response = "Error";
                    }
                    finally
                    {
                        var responseBytes = Encoding.UTF8.GetBytes(response);
                        channel.BasicPublish(exchange: "", routingKey: properties.ReplyTo, basicProperties: replyProperties, body: responseBytes);
                        channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                    }
                };

                Console.WriteLine("Presione [enter] para salir.");
                Console.ReadLine();
            }
        }
    }
}
