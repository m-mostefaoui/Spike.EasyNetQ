using System;

namespace EasyNetQManagement
{
    using System.Linq;
    using System.Threading.Tasks;
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;

    class Program
    {
        const string HostName = "localhost";
        const string VirtualHost = "ParcelVision.Retail";
        const string Username = "pvRetailDev";
        const string Password = "pvRetailDev";

        private static ManagementClient managementClient;
        private static Vhost vhost;

        private static async Task Main(string[] args)
        {
            string exchangeName = "spike.exchange";
            string queueName = "spike.queue";
            string routingKey = "spike.routingKey";

            managementClient = new ManagementClient(HostName, Username, Password);
            vhost = await managementClient.GetVhostAsync(VirtualHost);

            var exchange = await EnsureExchangeExists(exchangeName);
            var queue = await EnsureQueueExists(queueName);
            await EnsureBindingExists(exchange, queue, routingKey);

            Console.WriteLine("-- end --");
            //var bindings = await managementClient.GetBindingsWithSourceAsync(exchange);
            //var bindings = managementClient.GetBindingsWithDestinationAsync(exchange);
        }

        private static async Task<Exchange> EnsureExchangeExists(string exchangeName)
        {
            var (found, foundExchange) = await GetExchange(exchangeName);
            if (!found)
            {
                var createdExchange =
                    await managementClient.CreateExchangeAsync(
                        new ExchangeInfo(exchangeName, "direct"), vhost);
                return createdExchange;
            }

            return foundExchange;
        }

        private static async Task<(bool found, Exchange exchange)> GetExchange(string exchangeName)
        {
            Exchange exchange = null;

            try
            {
                exchange = await managementClient.GetExchangeAsync(exchangeName, vhost);
            }
            catch (UnexpectedHttpStatusCodeException)
            {
                // Log it
            }

            return (exchange != null, exchange);
        }

        private static async Task<Queue> EnsureQueueExists(string queueName)
        {
            var (found, foundQueue) = await GetQueue(queueName);
            if (!found)
            {
                var createdQueue = await managementClient.CreateQueueAsync(new QueueInfo(queueName), vhost);
                return createdQueue;
            }

            return foundQueue;
        }

        private static async Task<(bool found, Queue queue)> GetQueue(string queueName)
        {
            Queue queue = null;
            try
            {
                queue = await managementClient.GetQueueAsync(queueName, vhost);
            }
            catch (UnexpectedHttpStatusCodeException)
            {
                // Log it
            }

            return (queue != null, queue);
        }

        private static async Task EnsureBindingExists(Exchange exchange, Queue queue, string routingKey)
        {
            var bindings = await managementClient.GetBindingsAsync(exchange, queue);
            var binding = bindings.SingleOrDefault(b => b.RoutingKey.Equals(routingKey, StringComparison.OrdinalIgnoreCase));

            if (binding == null)
            {
                await managementClient.CreateBinding(exchange, queue, new BindingInfo(routingKey));
            }
        }
    }
}