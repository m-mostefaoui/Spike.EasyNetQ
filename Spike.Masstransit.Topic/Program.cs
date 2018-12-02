namespace Spike.Masstransit.Topic
{
    using Autofac;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using Messages;
    using RabbitMQ.Client;
    using System;

    class Program
    {
        private const string Url = "rabbitmq://localhost/ParcelVision.Retail";
        private const string User = "pvRetailDev";
        private const string Password = "pvRetailDev";

        private static IRabbitMqHost host;
        private const string ExchangeName = "spike.exchange";

        static void Main(string[] args)
        {
            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                host = cfg.Host(new Uri(Url), h =>
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
            });

            busControl.Start();

            var builder = new ContainerBuilder();
            builder.RegisterType<GenericConsumer>();
            var container = builder.Build();

            var ctx = container.Resolve<IComponentContext>();

            Subscribe(ctx, "generic");

            string input;
            do
            {
                Console.WriteLine("ready");
                busControl.Publish(new AddUser("Joe doe"));
                busControl.Publish(new AddUser("Jane doe"));
                busControl.Publish(new DeleteUser("Joe Doe"));
                input = Console.ReadLine();
            } while (input != "q");

            busControl.Stop();
        }

        private static void Subscribe(IComponentContext context, string key)
        {
            var queueName = $"spike-{key}-queue";
            //var handle = host.ConnectReceiveEndpoint(queueName, e =>
            var handle = host.ConnectReceiveEndpoint("spike.queue", e =>
            {
                e.BindMessageExchanges = false;
                e.Consumer<GenericConsumer>(context);
                //e.Bind(ExchangeName, x =>
                e.Bind("spike.exchange", x =>
                {
                    x.ExchangeType = ExchangeType.Direct;
                    x.Durable = true;
                    x.RoutingKey = "spike.routingKey";
                });
               // e.Bind<IEvent>();
            });

            handle.Ready.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}