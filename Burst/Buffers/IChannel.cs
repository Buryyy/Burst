namespace Burst.Buffers
{
    public interface IChannel
    {

        event Action? OnConnect;
        event Action? OnDisconnect;

        void Disconnect();
        ValueTask SendAsync(IByteBuffer buffer);
        void Start();
    }
}
