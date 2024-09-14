using Burst.Buffers;

namespace Burst.Pipelines
{
    public interface IPipeline
    {
        Task OnChannelReadAsync(IByteBuffer buffer, IChannel channel);  // Process incoming data
        Task OnChannelWriteAsync(IByteBuffer buffer, IChannel channel); // Process outgoing data
    }
}
