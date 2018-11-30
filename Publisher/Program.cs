namespace Publisher
{
    using System;
    using EasyNetQ;
    using EasyNetQ.Topology;
    using Messages;

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
                var exchange = new Exchange("spike.easynetq.exchange");
                var routingKey = "spike.*";

                string input;
                Console.WriteLine("Enter message (or quit to exit)");
                Console.Write("> ");
                while ((input = Console.ReadLine()) != "q")
                {
                    switch (input)
                    {
                        case "a":
                            var messageAddUser = new Message<AddUser>(new AddUser("Add John Doe"));
                            bus.Advanced.Publish<AddUser>(exchange, routingKey, false, messageAddUser);
                            break;
                        case "d":
                            var messageDeleteUser = new Message<DeleteUser>(new DeleteUser("Delete John Doe"));
                            bus.Advanced.Publish<DeleteUser>(exchange, routingKey, false, messageDeleteUser);
                            break;
                    }
                }
            }
        }
    }
}