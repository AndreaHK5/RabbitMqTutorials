using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using RabbitMQ.Client.Framing.Impl;

namespace Rabbit.Receiver
{
    class Program
    {
        // need to know the end point I am subscribing to
        private static string exchangeName = "someExchange";
        // note that we cannot really assing a queue name in this case as we are doing multiple subscribers (pub / sub)
        // if the name of the queue is hard coded the second subscriber will have the same queue (pissibly with different bindings) as compared with the first subscriber)
        // therefore we allow the subscirber to set up the queue name that it deems necessarty.
        //private static string queueName = "someQueue";




        static void Main(string[] args)
        {
            var factory = new ConnectionFactory() {HostName = "localhost"};

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    channel.ExchangeDeclare(exchangeName, "topic");

                    Console.WriteLine("Enter queue name (RabbitMQ will create if does not exist)");
                    var queueName = Console.ReadLine();

                    bool durable = true;
                    channel.QueueDeclare(queueName, durable, false, false, null);
                    // var queueName = channel.QueueDeclare().QueueName;

                    channel.CreateBasicProperties().SetPersistent(true);

                    Console.WriteLine("subscribed to queue {0}", queueName);
                    
                    // check in case the user has forgotten to input the arguments
                    if (args.Length < 1)
                    {
                        Console.WriteLine("need arguments, what do you want to bind this queue to?");
                        Console.WriteLine("About to exit of here");
                        Environment.ExitCode = 1;
                        Console.ReadLine();
                        return;
                    }                  

                    // this is where the binding happens.
                    // Binding in thsi context has nothing to do with MS binding. In this case the binding is the connection between the message and the queue that each consumer will get.
                    foreach (var keys in args)
                    {
                        channel.QueueBind(queueName, exchangeName, keys);
                        Console.WriteLine("{0} now bound to {1}", queueName, keys);
                    }

                    var consumer = new QueueingBasicConsumer(channel);
                    Console.WriteLine("consumer up and running, waiting for messages");
                    
                    // this is where we state that we will send acknowledgement back, otherwise pull will make the message disappear

                    bool NoAck = false;
                    channel.BasicConsume(queueName, NoAck, consumer);
                    
                    // this is where the messages are retrieved
                    while (true)
                    {
                        var ea = (BasicDeliverEventArgs) consumer.Queue.Dequeue();
                        var body = ea.Body;
                        var message = Encoding.UTF8.GetString(body);
                        
                        var routingKey = ea.RoutingKey;
                        Console.WriteLine("Received message {0} : {1}", message, routingKey);


                        channel.BasicAck(ea.DeliveryTag, false);


                    }




                }

            }









        }
        private static string GetMessage(string[] args)
        {
            // the API for the routing keys is to send messages associated with channels.
            // e.g. rabbit.sender  some.channel.here
            // the subscirber can hook to #.here or to some.*.here
            // allowing extra flexibility and reduced coupling
            // With binding keys we instead need to know exacltly what we are subscribing to.
            return ((args.Length > 0) ? string.Join(" ", args) : "This works!!");
        }

    }
}
