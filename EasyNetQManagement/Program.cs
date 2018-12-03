namespace EasyNetQManagement
{
    using EasyNetQ.Management.Client;
    using EasyNetQ.Management.Client.Model;
    using System;
    using System.Linq;
    using System.Threading.Tasks;

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
            string queueName = "spike.queue";
            
            using (managementClient = new ManagementClient(HostName, Username, Password))
            {
                vhost = await managementClient.GetVhostAsync(VirtualHost);

                var queue = await CreateQueue(queueName);

                var exchanges = await managementClient.GetExchangesAsync();
                var enumerable = exchanges.Where(x => x.Name.EndsWith("ServiceRatesImportFileParsed", StringComparison.OrdinalIgnoreCase)).ToArray();

                foreach (var ex in enumerable)
                {
                    Console.WriteLine($"Exchange {ex.Name}");
                    var exchange = await CreateExchange(ex.Name);
                    await CreateBinding(exchange, queue);
                }
            }

            Console.WriteLine("-- end --");
        }

        private static async Task<Exchange> CreateExchange(string exchangeName)
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

        private static async Task<Queue> CreateQueue(string queueName)
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
            catch (Exception)
            {
                // Log it
            }

            return (queue != null, queue);
        }

        private static async Task CreateBinding(Exchange exchange, Queue queue)
        {
            var bindings = await managementClient.GetBindingsAsync(exchange, queue);
            //var binding = bindings.SingleOrDefault(b => b.RoutingKey.Equals(routingKey, StringComparison.OrdinalIgnoreCase));

           // if (binding == null)
           // {
                await managementClient.CreateBinding(exchange, queue, new BindingInfo(""));
            //}
        }
    }
}