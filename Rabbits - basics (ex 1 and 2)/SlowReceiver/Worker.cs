using System;
using System.Text;
using System.Threading;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace SlowReceiver
{
    class Worker
    {
        // private static string queueName = "someQueue";

        private static string queueDurableName = "someDurableQueue";



        static void Main(string[] args)
        {

            // this is to show the automatic round robin for the dispatching of messages.
            var factory = new ConnectionFactory() { HostName = "localhost" };

            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    bool durable = true;
                    channel.QueueDeclare(queueDurableName, durable, false, false, null);

                    // we also need to state that the messages are to be durable.
                    channel.CreateBasicProperties().SetPersistent(true);



                    // the subscriber in rabbitMq is called consumer
                    var consumer = new QueueingBasicConsumer(channel);
                    // in this version the message is delivered but is at least once
                    // channel.BasicConsume(queueName, true, consumer);
                    
                    // we turn the acknowledgement to true, meaning that the message is popped only when it is consumed - not earlier. at most once
                    bool NoAck = false;
                    channel.BasicConsume(queueDurableName, NoAck, consumer);

                    Console.WriteLine("waiting for messages, hit ctrl c to exit");


                    while (true)
                    {
                        // de queue is basically the pop method
                        var ea = (BasicDeliverEventArgs)consumer.Queue.Dequeue();
                        var body = ea.Body;

                        var message = Encoding.UTF8.GetString(body);

                        Console.WriteLine("received message: " + message);

                        int dots = message.Split('.').Length - 1;
                        
                        Thread.Sleep(dots * 1000);
                        Console.WriteLine("Some processing here");
                        // artificial crash to test acknowledgement.
                        //Console.WriteLine("oh no, i crashed..");
                        //Console.ReadLine();
                        //return;
                        Thread.Sleep(1000);
                        Console.WriteLine("Done with processing");


                        channel.BasicAck(ea.DeliveryTag, false);

                        // this is required in case there is a acknowledgement

                    }
                    
                }
            }
        }
    }
}
