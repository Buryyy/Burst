using Burst.Pipelines;
using System.Net;

namespace Burst.Core
{
    public class ServerBootstrap
    {
        private int _port;
        private EventLoopGroup? _eventLoopGroup;
        private Action<Pipeline>? _pipelineConfigurator;

        // Socket settings
        private bool _useTcpNoDelay = true;       // Nagle's Algorithm (default to off)
        private int _receiveBufferSize = 512;     // Default receive buffer size
        private int _sendBufferSize = 512;        // Default send buffer size

        public ServerBootstrap SetPort(int port)
        {
            _port = port;
            return this;
        }

        public ServerBootstrap SetEventLoopGroup(EventLoopGroup eventLoopGroup)
        {
            _eventLoopGroup = eventLoopGroup;
            return this;
        }

        public ServerBootstrap SetPipelineHandlers(Action<Pipeline> pipelineConfigurator)
        {
            _pipelineConfigurator = pipelineConfigurator;
            return this;
        }

        // Set TCP NoDelay (Nagle's algorithm)
        public ServerBootstrap SetTcpNoDelay(bool tcpNoDelay)
        {
            _useTcpNoDelay = tcpNoDelay;
            return this;
        }

        // Set the socket receive buffer size
        public ServerBootstrap SetReceiveBufferSize(int size)
        {
            _receiveBufferSize = size;
            return this;
        }

        // Set the socket send buffer size
        public ServerBootstrap SetSendBufferSize(int size)
        {
            _sendBufferSize = size;
            return this;
        }

        public async Task StartAsync()
        {
            if (_eventLoopGroup == null || _pipelineConfigurator == null)
            {
                throw new InvalidOperationException("EventLoopGroup and PipelineConfigurator must be set before starting the server.");
            }

            var listener = new SocketListener(_eventLoopGroup, _pipelineConfigurator);

            await listener.BindAsync(new IPEndPoint(IPAddress.Any, _port), _useTcpNoDelay, _receiveBufferSize, _sendBufferSize);
        }
    }
}
