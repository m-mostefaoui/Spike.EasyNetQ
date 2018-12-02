using RabbitMQ.Client;

namespace Spike.Masstransit
{
    using System;
    using MassTransit;
    using Messages;

    public class Program
    {
        private const string Url = "rabbitmq://localhost/ParcelVision.Retail";
        private const string User = "pvRetailDev";
        private const string Password = "pvRetailDev";

        public static void Main()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri(Url), h =>
                {
                    h.Username(User);
                    h.Password(Password);
                });

                cfg.Send<IEvent>(x =>
                {
                    x.UseRoutingKeyFormatter(context => "spike.routingKey");
                });

                cfg.Message<IEvent>(x => x.SetEntityName("spike.exchange"));
                cfg.Publish<IEvent>(x => x.ExchangeType = ExchangeType.Direct);

                //sbc.ReceiveEndpoint(host, "spike.queue", ep =>
                //{
                //    ep.Bind("spike.exchange", x =>
                //    {
                //        x.Durable = false;
                //        x.AutoDelete = true;
                //        x.ExchangeType = "direct";
                //       // x.RoutingKey = "spike.routingKey";
                //    });

                //    ep.Consumer<GenericHandler>();
                //});
            });

            bus.Start();

            bus.Publish(new AddUser("Add User Joe doe"));
            bus.Publish(new DeleteUser("Delete User Joe doe"));

            Console.WriteLine("Producer... Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}