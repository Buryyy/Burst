using System.Collections.Concurrent;

namespace Burst.Core
{
    public class EventLoop
    {
        private readonly BlockingCollection<Func<ValueTask>> _taskQueue = new BlockingCollection<Func<ValueTask>>();
        private readonly Thread _workerThread;
        private volatile bool _isRunning = false;

        public EventLoop()
        {
            _workerThread = new Thread(Run) { IsBackground = true };
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            _workerThread.Start();
        }

        public void Add(Func<ValueTask> task)
        {
            _taskQueue.Add(task); // Efficiently add tasks to the queue
        }

        private void Run()
        {
            foreach (var task in _taskQueue.GetConsumingEnumerable())
            {
                if (!_isRunning) break;
                try
                {
                    var valueTask = task();
                    if (!valueTask.IsCompleted)
                    {
                        valueTask.AsTask().Wait(); // Ensure the task is completed
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error executing task: {ex.Message}");
                }
            }
        }

        public void Shutdown()
        {
            _isRunning = false;
            _taskQueue.CompleteAdding(); // Signal the loop to stop consuming
        }
    }
}
