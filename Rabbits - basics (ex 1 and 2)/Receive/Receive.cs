using System;
using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;


namespace Receive
{
    class Receive
    {
        private static string queueName = "someQueue";
        static void Main()
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.QueueDeclare(queueName, false, false, false, null);

                    // the subscriber in rabbitMq is called consumer
                    var consumer = new QueueingBasicConsumer(channel);
                    channel.BasicConsume(queueName, true, consumer);
                    
                    Console.WriteLine("waiting for messages, hit ctrl c to exit");


                    while (true)
                    {
                        // de queue is basically the pop method
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                        var body = ea.Body;

                        var message = Encoding.UTF8.GetString(body);
                        
                        Console.WriteLine("received message: " + message);


                    }
                }
            }
        }

    }
}
