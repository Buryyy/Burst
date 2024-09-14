using Burst.Core;

namespace Burst.EchoServer.Sample
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Starting server...");

            var bootstrapper = new ServerBootstrap();
            bootstrapper.SetPort(4324)
                 .SetEventLoopGroup(new EventLoopGroup(1))
                 .SetPipelineHandlers((config) =>
                 {
                     config.AddHandler(new EchoHandler());
                 })
                 .SetTcpNoDelay(true);

            _ = bootstrapper.StartAsync();
            while (true)
            {
                Thread.Sleep(100);
            }
        }
    }
}
