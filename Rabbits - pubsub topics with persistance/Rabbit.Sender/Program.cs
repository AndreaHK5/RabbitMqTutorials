using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Rabbit.Sender
{
    class Program
    {
        // echagne is the RabbitMQ name for the topic. This is in lieu of a standard queue
        private static string exchangeName = "someExchange";

        static void Main(string[] args)
        {
            // create the conneciton factory
            var factory = new ConnectionFactory() { HostName = "localhost" };

            // usign disposable scopes for the channel and the model
            using (var connection = factory.CreateConnection())
            {
                using (var channel = connection.CreateModel())
                {
                    // we are creating the exchange
                    // the options for an exchage are: 
                    // fanout (pub/sub without filters), direct (allows subsciption with a binding key), topic (allows subscription with routing keys) 
                    channel.ExchangeDeclare(exchangeName, "topic");

                    // this option prevent the blind round robin (just based on number of messages) but instead turns on dispatiching to a new listener in case of competing listeners.
                    channel.BasicQos(0, 1, false);

                    // and at thsi point we will declare the queue
                    // in the other exercises the queue has no name.
                    // as the consumer can retrieve the queue name and, frankly, it is not interested in the queue name as it can subscribe via the end point + routing keys
                    // nonetheless here we are declaring a queue name in case this is needed

                    var properties = channel.CreateBasicProperties();
                    properties.SetPersistent(true);

                    // get the routing key (dot format)
                    var routingKey = (args.Length > 0) ? args[0] : "anonymous.info";
                    
                    var msg = (args.Length > 1) ? string.Join("", args.Skip(1).ToArray()) : "Message";
                    var body = Encoding.UTF8.GetBytes(msg);

                    // basically the routing key is used to create the queue as the name
                    channel.BasicPublish(exchangeName, routingKey, properties, body);
                    Console.WriteLine("sent message {0} : {1}", msg, routingKey);



                }



            }
            // create queue/ exchange
            // delete exchange nif already existing
            // demand acknowledgements
            // make queue durable
            // send messasges with binding keys

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
