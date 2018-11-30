namespace Spike.Masstransit
{
    using System;
    using System.Threading.Tasks;
    using MassTransit;
    using Messages;
    using Newtonsoft.Json.Linq;

    public class GenericHandler : IConsumer<IEvent>
    {
        public Task Consume(ConsumeContext<IEvent> context)
        {
            if (context.TryGetMessage(out ConsumeContext<JToken> jsonContext))
            {
                JToken jToken = jsonContext.Message;
                return Console.Out.WriteLineAsync($"Received: {jToken.First}");
            }

            return Task.FromResult(0);
        }
    }
}