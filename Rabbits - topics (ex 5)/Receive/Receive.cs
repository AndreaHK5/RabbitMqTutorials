using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Receive
{
    class Receive
    {
        private static string exchangeName = "someExchange";
        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, "topic");
                    var queueName = channel.QueueDeclare().QueueName;
                    Console.WriteLine("the queue name is " + queueName);

                    if (args.Length < 1)
                    {
                        Console.WriteLine("need an argument, what do you want to bind this queue to???");
                        Console.WriteLine("about to walk out of here");
                        Environment.ExitCode = 1;
                        Console.ReadLine();

                        return;
                    }

                    foreach (var routingKey in args)
                    {
                        channel.QueueBind(queueName, exchangeName, routingKey);
                        Console.WriteLine("Q bound to {0}", routingKey);
                    }

                    // this is where I bind the queue to the exchange (in all effects, allowing the receiver to be pub/sub.
                    // this is the subscriber
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    Console.WriteLine("consumer up and running, waiting for logs // push start gutton (ctrl c) to exit");

                    while (true)
                    {
                        // de queue is basically the pop method
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);

                        // in addition, for the direct queue
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine("received message: {0}: {1}", routingKey, message);
                    }
                }
            }
        }

    }
}
