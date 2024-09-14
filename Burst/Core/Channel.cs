using Burst.Buffers;
using Burst.Pipelines;
using System.Buffers;
using System.Net.Sockets;

namespace Burst.Core
{
    public class Channel : IChannel
    {
        private const int MinBufferSize = 128;
        private const int MaxBufferSize = 8192;

        private readonly Socket _socket;
        private readonly Pipeline _pipeline;
        private readonly EventLoop _eventLoop;
        private readonly MemoryPool<byte> _memoryPool;
        private IMemoryOwner<byte>? _readMemoryOwner;
        private bool _isClosed = false;
        private int _currentBufferSize = MinBufferSize;

        public event Action? OnConnect;
        public event Action? OnDisconnect;

        public Channel(Socket socket, EventLoop eventLoop, Pipeline pipeline)
        {
            _socket = socket;
            _eventLoop = eventLoop;
            _pipeline = pipeline;
            _memoryPool = MemoryPool<byte>.Shared;

            SetPlatformSpecificKeepAliveOptions();
            OnConnect?.Invoke();  // Raise the OnConnect event when the channel is initialized
        }

        private void SetPlatformSpecificKeepAliveOptions()
        {
#if __WINDOWS__
            SetWindowsKeepAliveOptions();
#elif __LINUX__
            SetLinuxKeepAliveOptions();
#endif
        }

#if __WINDOWS__
        private void SetWindowsKeepAliveOptions()
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);

            var keepAliveTime = 7200 * 1000;  // 2 hours
            var keepAliveInterval = 1000;     // 1 second

            var inOptionValues = new byte[12];
            BitConverter.GetBytes(1).CopyTo(inOptionValues, 0);  // Enable keep-alive
            BitConverter.GetBytes(keepAliveTime).CopyTo(inOptionValues, 4);  // Time before sending keep-alive packets
            BitConverter.GetBytes(keepAliveInterval).CopyTo(inOptionValues, 8); // Interval between keep-alive packets

            _socket.IOControl(IOControlCode.KeepAliveValues, inOptionValues, null);
        }
#endif

#if __LINUX__
        private void SetLinuxKeepAliveOptions()
        {
            _socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 7200);  // Keep-alive idle time (2 hours)
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 1);  // Keep-alive interval (1 second)
            _socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 10);  // Keep-alive probe count
        }
#endif

        public void Start()
        {
            _eventLoop.Add(() => ReceiveLoopAsync(_pipeline));
        }

        private async ValueTask ReceiveLoopAsync(Pipeline pipeline)
        {
            try
            {
                while (!_isClosed)
                {
                    _readMemoryOwner = _memoryPool.Rent(_currentBufferSize);
                    Memory<byte> memory = _readMemoryOwner.Memory;

                    int bytesRead;
                    try
                    {
                        bytesRead = await _socket.ReceiveAsync(memory, SocketFlags.None);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"SocketException: {ex.Message}");
                        break;
                    }

                    if (bytesRead == 0)
                    {
                        Console.WriteLine("Connection closed by the client.");
                        break;
                    }

                    if (bytesRead == _currentBufferSize && _currentBufferSize < MaxBufferSize)
                    {
                        _currentBufferSize = Math.Min(_currentBufferSize * 2, MaxBufferSize);  // Grow buffer
                    }
                    else if (bytesRead < _currentBufferSize / 2 && _currentBufferSize > MinBufferSize)
                    {
                        _currentBufferSize = Math.Max(_currentBufferSize / 2, MinBufferSize);  // Shrink buffer
                    }

                    var buffer = new ByteBuffer(bytesRead);
                    buffer.WriteBytes(memory.Slice(0, bytesRead).Span);

                    await pipeline.ProcessReadAsync(buffer, this);
                }
            }
            finally
            {
                _readMemoryOwner?.Dispose();
            }
        }

        public async ValueTask SendAsync(IByteBuffer buffer)
        {
            if (!_socket.Connected)
            {
                Console.WriteLine("Socket is not connected. Cannot send data.");
                return;
            }

            IMemoryOwner<byte>? writeMemoryOwner = null;
            try
            {
                // Use the memory pool to rent memory based on the readable bytes
                writeMemoryOwner = _memoryPool.Rent(buffer.ReadableBytes);
                Memory<byte> memory = writeMemoryOwner.Memory.Slice(0, buffer.ReadableBytes);

                // Copy data from the buffer into the rented memory
                buffer.GetArray().AsSpan(buffer.ReaderIndex, buffer.ReadableBytes).CopyTo(memory.Span);

                int totalBytesSent = 0;
                while (totalBytesSent < buffer.ReadableBytes)
                {
                    int bytesSent;
                    try
                    {
                        Memory<byte> memoryToSend = memory.Slice(totalBytesSent);
                        bytesSent = await _socket.SendAsync(memoryToSend, SocketFlags.None);
                    }
                    catch (SocketException ex)
                    {
                        Console.WriteLine($"SocketException while sending: {ex.Message}");
                        break;
                    }

                    totalBytesSent += bytesSent;
                    buffer.ReaderIndex += bytesSent;
                }
            }
            finally
            {
                // Dispose of the memory owner to return it to the pool, ensuring it's not null
                writeMemoryOwner?.Dispose();
            }
        }


        public void Disconnect()
        {
            if (_isClosed) return;

            OnDisconnect?.Invoke();

            if (_socket.Connected)
            {
                _socket.Shutdown(SocketShutdown.Both);
            }

            _socket.Close();
            _socket.Dispose();
            _isClosed = true;
        }
    }
}
