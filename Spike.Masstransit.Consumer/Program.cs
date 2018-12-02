using System;
using MassTransit;
using Messages;
using RabbitMQ.Client;

namespace Spike.Masstransit.Consumer
{
    class Program
    {
        private const string Url = "rabbitmq://localhost/ParcelVision.Retail";
        private const string User = "pvRetailDev";
        private const string Password = "pvRetailDev";

        static void Main(string[] args)
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri(Url), h =>
                {
                    h.Username(User);
                    h.Password(Password);
                });

                sbc.ReceiveEndpoint(host, "spike.queue", ep =>
                {
                    ep.BindMessageExchanges = false;
                    ep.Consumer<GenericHandler>();

                    ep.Bind("spike.exchange", x =>
                    {
                        //x.Durable = false;
                        //x.AutoDelete = true;
                        x.RoutingKey = "spike.routingKey";
                        x.ExchangeType = ExchangeType.Direct;
                        
                    });
                    //ep.Bind<IEvent>();
                });
            });


            Console.WriteLine("Listening for messages. Hit <return> to quit.");
            Console.ReadLine();
        }
    }
}
