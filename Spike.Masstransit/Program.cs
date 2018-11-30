namespace Spike.Masstransit
{
    using System;
    using MassTransit;
    using Messages;

    public class Program
    {
        public static void Main()
        {
            var bus = Bus.Factory.CreateUsingRabbitMq(sbc =>
            {
                var host = sbc.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                sbc.ReceiveEndpoint(host, "test_queue", ep =>
                {
                    ep.Consumer<GenericHandler>(); 
                });
            });

            bus.Start();

            bus.Publish(new AddUser("Add User Joe doe", "jdoe@somedomain.com",""));
            bus.Publish(new DeleteUser("Delete User Joe doe"));

            Console.WriteLine("Press any key to exit");
            Console.ReadKey();

            bus.Stop();
        }
    }
}