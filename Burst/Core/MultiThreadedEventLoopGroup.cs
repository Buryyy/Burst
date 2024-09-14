namespace Burst.Core
{
    public class EventLoopGroup
    {
        private readonly EventLoop[] _eventLoops;
        private int _nextLoopIndex = 0;

        public EventLoopGroup(int numLoops)
        {
            _eventLoops = new EventLoop[numLoops];
            for (int i = 0; i < numLoops; i++)
            {
                _eventLoops[i] = new EventLoop();
                _eventLoops[i].Start();
            }
        }

        public EventLoop GetNextEventLoop()
        {
            int index = Interlocked.Increment(ref _nextLoopIndex) % _eventLoops.Length;
            return _eventLoops[index];
        }

        public void Shutdown()
        {
            foreach (var loop in _eventLoops)
            {
                loop.Shutdown();
            }
        }
    }
}
