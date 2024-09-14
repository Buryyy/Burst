using Burst.Pipelines;
using System.Net;
using System.Net.Sockets;

namespace Burst.Core
{
    public class SocketListener
    {
        private readonly EventLoopGroup _eventLoopGroup;
        private readonly Action<Pipeline> _pipelineConfigurator;

        public SocketListener(EventLoopGroup eventLoopGroup, Action<Pipeline> pipelineConfigurator)
        {
            _eventLoopGroup = eventLoopGroup;
            _pipelineConfigurator = pipelineConfigurator;
        }

        public async Task BindAsync(EndPoint endPoint, bool tcpNoDelay, int receiveBufferSize, int sendBufferSize)
        {
            var listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            listener.Bind(endPoint);
            listener.Listen(100);

            Console.WriteLine($"Server listening on {endPoint}");

            while (true)
            {
                var clientSocket = await listener.AcceptAsync();
                Console.WriteLine("[Server] Client connected.");

                // Apply TCP-specific socket settings
                if (clientSocket.SocketType == SocketType.Stream && clientSocket.ProtocolType == ProtocolType.Tcp)
                {
                    clientSocket.NoDelay = tcpNoDelay;               // Set TCP NoDelay (Nagle's algorithm)
                }

                clientSocket.ReceiveBufferSize = receiveBufferSize;
                clientSocket.SendBufferSize = sendBufferSize;

                var pipeline = new Pipeline();
                _pipelineConfigurator(pipeline);

                var eventLoop = _eventLoopGroup.GetNextEventLoop();
                var channel = new Channel(clientSocket, eventLoop, pipeline);
                channel.Start();
            }
        }
    }
}
