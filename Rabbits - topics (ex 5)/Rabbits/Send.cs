using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using RabbitMQ.Client;

namespace Rabbits
{
    class Send
    {
        private static string exchangeName = "someExchange";

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory(){HostName = "localhost"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, "topic");

                    // the only difference in this is that the routing key has the dot format + the exchange is specified as a topic
                    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                    var msg = (args.Length > 1) ? string.Join("", args.Skip(1).ToArray()) : "Yolo!";
                    var body = Encoding.UTF8.GetBytes(msg);                    
                    channel.BasicPublish(exchangeName, routingKey, null, body);
                    Console.WriteLine("sent message {0} : {1}", msg, routingKey);
                }
            }


        }
    }
}
