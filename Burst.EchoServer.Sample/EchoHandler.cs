using Burst.Buffers;
using Burst.Pipelines;
using System.Text;

namespace Burst.EchoServer.Sample
{
    public class EchoHandler : IPipeline
    {
        public async Task OnChannelReadAsync(IByteBuffer buffer, IChannel channel)
        {
            // Read the incoming message
            byte[] strBytes = new byte[buffer.ReadableBytes];
            buffer.ReadBytes(strBytes);
            string receivedStr = Encoding.ASCII.GetString(strBytes);
            Console.WriteLine($"[EchoHandler::OnReadAsync]: {receivedStr}");

            // Echo the message back to the client
            IByteBuffer sendBuffer = new ByteBuffer();
            sendBuffer.WriteBytes(strBytes);

            // Send the data back to the client (this triggers the pipeline's write process)
            await channel.SendAsync(sendBuffer);
        }

        public Task OnChannelWriteAsync(IByteBuffer buffer, IChannel channel)
        {
            // No need to call SendAsync here, as it will cause a recursive loop
            // Just return a completed task for write operation
            return Task.CompletedTask;
        }
    }
}
