namespace Subscriber
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Topology;
    using Messages;
    using Newtonsoft.Json.Linq;

    class Program
    {
        static void Main(string[] args)
        {
            const string HostName = "localhost";
            const string VirtualHost = "ParcelVision.Retail";
            const string Username = "pvRetailDev";
            const string Password = "pvRetailDev";

            var connectionString = $"host={HostName};virtualHost={VirtualHost};username={Username};password={Password}";

            
            using (var bus = RabbitHutch.CreateBus(connectionString))
            {

               
                

                var queue = bus.Advanced.QueueDeclare("spike.easynetq.queue");
                var exchange = bus.Advanced.ExchangeDeclare("spike.easynetq.exchange", ExchangeType.Topic);
                bus.Advanced.Bind(exchange, queue, "#");

                bus.Advanced.Consume(queue, x => x
                        .Add<IEvent>((message, info) =>
                        {
                            JToken jToken = JToken.FromObject(message.Body);
                            Console.WriteLine("Add User: {0}", jToken.First);
                        })
                        .ThrowOnNoMatchingHandler = false
                );

                
                Console.WriteLine("Listening for messages. Hit <return> to quit.");
                Console.ReadLine();
            }

        }
    }
}