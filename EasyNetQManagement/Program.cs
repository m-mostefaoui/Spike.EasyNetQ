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

        static async Task Main(string[] args)
        {
            string exchangeName = "spike.exchange";
            string queueName = "spike.queue";

            managementClient = new ManagementClient(HostName, Username, Password);
            vhost = await managementClient.GetVhostAsync(VirtualHost);

            var exchange = await EnsureExchangeExists(exchangeName);
            var queue = await EnsureQueueExists(queueName);

            //var bindings = await managementClient.GetBindingsWithSourceAsync(exchange);
            //var bindings = managementClient.GetBindingsWithDestinationAsync(exchange);
        }

        static async Task<Exchange> EnsureExchangeExists(string exchangeName)
        {
            var ( found, foundExchange)  = await IsExchangeExists(exchangeName);
            if (!found)
            {
                
                var exchangeInfo = new ExchangeInfo("spike_exchange", "direct");
                var createdExchange = await managementClient.CreateExchangeAsync(exchangeInfo, vhost);
                return createdExchange;
            }

            return foundExchange;
        }

        static async Task<(bool found, Exchange exchange)> IsExchangeExists(string exchangeName)
        {
            Exchange exchange = null;

            try
            {
                exchange = await managementClient.GetExchangeAsync(exchangeName, vhost);
            }
            catch (UnexpectedHttpStatusCodeException) { }
            
            return (exchange != null, exchange);
        }


        static async Task<Queue> EnsureQueueExists(string queueName)
        {
            var (found, foundQueue) = await IsQueueExists(queueName);
            if (!found)
            {
                
                var queueInfo = new QueueInfo(queueName);
                var createdQueue = await managementClient.CreateQueueAsync(queueInfo, vhost);
                return createdQueue;
            }

            return foundQueue;
        }

        static async Task<(bool found, Queue queue)> IsQueueExists(string queueName)
        {
            var queue = await managementClient.GetQueueAsync(queueName, vhost);

            return (queue != null, queue);
        }

        static async Task EnsureBindingExists(string exchangeName, string queueName, string routingKey)
        {
            var queue = await managementClient.GetQueueAsync(queueName, vhost);
            var exchange = await managementClient.GetExchangeAsync(exchangeName, vhost);

            var bindings = await managementClient.GetBindingsAsync(exchange, queue);
            var binding = bindings.SingleOrDefault(b => b.RoutingKey.Equals(routingKey, StringComparison.OrdinalIgnoreCase));

            if (binding == null)
            {
                var bindingInfo = new BindingInfo(routingKey);

                await managementClient.CreateBinding(exchange, queue, bindingInfo);
            }
        }
    }
}