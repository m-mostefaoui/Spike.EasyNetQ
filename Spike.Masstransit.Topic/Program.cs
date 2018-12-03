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

        static void Main(string[] args)
        {
            string exchangeName = "spike-exchange-test";

            var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                host = cfg.Host(new Uri(Url), h =>
                {
                    h.Username(User);
                    h.Password(Password);
                });

                cfg.Send<AddUser>(x => { x.UseRoutingKeyFormatter(context => "spike.users.*"); });
                cfg.Message<AddUser>(x => x.SetEntityName(exchangeName));

                cfg.Send<DeleteUser>(x => { x.UseRoutingKeyFormatter(context => "spike.users.delete"); });
                cfg.Message<DeleteUser>(x => x.SetEntityName(exchangeName));
                
                cfg.Publish<AddUser>(x => x.ExchangeType = ExchangeType.Topic);
                cfg.Publish<DeleteUser>(x => x.ExchangeType = ExchangeType.Topic);


                cfg.ReceiveEndpoint(host, "spike.queue", x =>
                {
                    x.BindMessageExchanges = false;

                    x.Consumer<GenericConsumer>();

                    x.Bind(exchangeName, s =>
                    {
                        s.RoutingKey = "spike.users.*";
                        s.ExchangeType = ExchangeType.Topic;
                    });
                });


            });

            busControl.Start();

      /*
            var builder = new ContainerBuilder();
            builder.RegisterType<GenericConsumer>();
            var container = builder.Build();

            var ctx = container.Resolve<IComponentContext>();

            Subscribe(ctx, "spike.exchange", "spike.queue1", "spike.*");
            Subscribe(ctx, "spike.exchange", "spike.queue2", "spike.delete.*");
    */        

            string input;
            do
            {
                Console.WriteLine("ready");
                busControl.Publish(new AddUser("Add: Joe doe"));
                busControl.Publish(new DeleteUser("Delete : Joe Doe"));
                input = Console.ReadLine();
            } while (input != "q");

            busControl.Stop();
        }

        private static void Subscribe(IComponentContext context, string exchangeName, string queueName, string routingKey)
        {
            var handle = host.ConnectReceiveEndpoint(queueName, e =>
            {
                e.BindMessageExchanges = false;
                e.Consumer<GenericConsumer>(context);
                e.Bind(exchangeName, x =>
                {
                    x.ExchangeType = ExchangeType.Topic;
                    x.Durable = true;
                    x.RoutingKey = routingKey;
                });
            });

            handle.Ready.ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}