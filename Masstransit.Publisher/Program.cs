namespace Masstransit.Publisher
{
    using System;
    using System.Text;
    using MassTransit;
    using MassTransit.RabbitMqTransport;
    using Messages;
    using RabbitMQ.Client;

    class Program
    {
        private const string Url = "rabbitmq://localhost/ParcelVision.Retail";
        private const string User = "pvRetailDev";
        private const string Password = "pvRetailDev";

        private static IRabbitMqHost host;
        
        static void Main(string[] args)
        {
            var busControl = ConfigureBus();

            busControl.Start();

            do
            {
                Console.WriteLine("Enter message (or quit to exit)");
                Console.Write("> ");
                var value = Console.ReadLine();

                if ("q".Equals(value, StringComparison.OrdinalIgnoreCase)) break;

                busControl.Publish(new AddUser(value));

            } while (true);

            busControl.Stop();
        }

        private static IBusControl ConfigureBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                cfg.Host(new Uri(Url), h =>
                {
                    h.Username(User);
                    h.Password(Password);
                });
            });
        }
    }
}