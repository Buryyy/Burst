using Burst.Buffers;

namespace Burst.Pipelines
{
    public class Pipeline
    {
        private readonly List<IPipeline> _handlers = new List<IPipeline>();

        // Add a handler to the pipeline
        public void AddHandler(IPipeline handler)
        {
            _handlers.Add(handler);
        }

        // Process incoming data through all handlers in the pipeline
        public async Task ProcessReadAsync(IByteBuffer buffer, IChannel channel)
        {
            foreach (var handler in _handlers)
            {
                await handler.OnChannelReadAsync(buffer, channel); // Use the client's channel
            }
        }

        // Process outgoing data through all handlers in the pipeline
        public async Task ProcessWriteAsync(IByteBuffer buffer, IChannel channel)
        {
            foreach (var handler in _handlers)
            {
                await handler.OnChannelWriteAsync(buffer, channel); // Use the client's channel
            }
        }
    }
}
