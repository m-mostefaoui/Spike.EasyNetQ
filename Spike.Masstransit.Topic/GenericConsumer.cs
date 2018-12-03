namespace Spike.Masstransit.Topic
{
    using Autofac;
    using MassTransit;
    using Messages;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Threading.Tasks;

    public class GenericConsumer : IConsumer<IEvent>
    {
        //private IComponentContext componentContext;

        //public GenericConsumer(IComponentContext componentContext)
        //{
        //    this.componentContext = componentContext ?? throw new ArgumentNullException(nameof(componentContext));
        //}

            
        public Task Consume(ConsumeContext<IEvent> context)
        {
            if (context.TryGetMessage(out ConsumeContext<JToken> jsonContext))
            {
                var jToken = jsonContext.Message;
                return Console.Out.WriteLineAsync($"Received: {jToken.First} - {jToken.Last}");
            }

            return Task.CompletedTask;
        }
    }
}