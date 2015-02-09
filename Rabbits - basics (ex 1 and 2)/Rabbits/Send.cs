using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using RabbitMQ.Client;

namespace Rabbits
{
    class Send
    {
        private static string queueDurableName = "someDurableQueue";
        private static string queueName = "someQueue";


        

        static void Main(string[] args)
        {
            var factory = new ConnectionFactory()
            {
                HostName = "localhost"
            };
            // the connection abstracts the socket connection, takes care of version negotiation and authentication and so on for us. We connect the broker on the local machine
            using (var connection = factory.CreateConnection())
            {
                // channel is the API for the queue
                using (var channel = connection.CreateModel())
                {
                    // this is to send to competing consumers
                    // pass in arguments by using the exe form the command line and type rabbits some message here .... each dot will stop the worker for one second.
                    // in order to turn on basic dispatching - otherwise it is a blind round robin (this requires acknowledgement)
                    channel.BasicQos(0, 1, false);

                    var msg = GetMessage(args);
                    var body = Encoding.UTF8.GetBytes(msg);
                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);
                    // in this case we have a queue that is not durable.
                    //channel.QueueDeclare(queueName, false, false, false, null);
                    
                    // Now we put together a durable queue instead
                    bool durable = true;
                    channel.QueueDeclare(queueDurableName, durable, false, false, null);
                    //var properties = channel.CreateBasicProperties();
                    
                    //properties.DeliveryMode = 2;
                    // the first parameter is the exchange used, the second the queue, then properties and what we are sending though the wire.
                        
                    channel.BasicPublish("", queueDurableName, properties, body);




                    // ****************** Hello world
                    // we then need to create / retriebve the queue we want to use
                    // this is idepotent operation 
                    
                    //channel.QueueDeclare(queueName, false, false, false, null);
                    //string msg = "Yolo, bass rules";
                    //var body = Encoding.UTF8.GetBytes(msg);
                    //channel.BasicPublish("",queueName, null, body);
                    
                    
                    
                    
                    
                    
                    Console.WriteLine("sent message {0}", msg);

                }
            }


        }

        private static string GetMessage(string[] args)
        {
            return ((args.Length > 0) ? string.Join(" ", args) : "Yolo, this works!!");
        }
    }
}
