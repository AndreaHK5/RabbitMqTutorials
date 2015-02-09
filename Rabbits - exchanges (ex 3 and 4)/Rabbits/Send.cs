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
            
                    // create the exchange, fanout means pub sub with no filter
                    //channel.ExchangeDeclare(exchangeName, "fanout");
                    // in this second case we go for a direct exchange, like a fanout BUT can take filters.


                    channel.ExchangeDeclare(exchangeName, "direct");

                    // get and set up the messages

                    var severity = (args.Length > 0) ? args[0] : "green";
                    var msg = (args.Length > 1) ? string.Join("", args.Skip(1).ToArray()) : "Yolo!";
                    var body = Encoding.UTF8.GetBytes(msg);                    
                    // publishing to the exchange (it will then forward to all queues that are bound to him).
                    //channel.BasicPublish(exchangeName, "", null, body);

                    // in this case we publish to an exchange that is direct, meanning it can take a filter parameter.
                    channel.BasicPublish(exchangeName, severity, null, body);
 
                    Console.WriteLine("sent message {0} : {1}", msg, severity);

                }
            }


        }
    }
}
