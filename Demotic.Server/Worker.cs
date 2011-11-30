using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Demotic.Server
{
    internal class Worker
    {
        public Worker()
        {
            _workQueue = new Queue<WorkItem>();
        }

        public void Start()
        {
            _canceler = new CancellationTokenSource();

            _task = Task.Factory.StartNew(() => DoWork(), _canceler.Token,
                TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public void Dispatch(UserAction request, IPresentationClient asClient)
        {
            Console.WriteLine("enqueuing request on work queue");
            _workQueue.Enqueue(new WorkItem { Issuer = asClient, Request = request });
        }

        public void Stop()
        {
            _canceler.Cancel();

            _task.Wait();
        }

        private struct WorkItem
        {
            public IPresentationClient Issuer { get; set; }
            public UserAction Request { get; set; }
        }

        private void DoWork()
        {
            while (true)
            {
                WorkItem? next;

                lock (_workQueue)
                {
                    next = _workQueue.Count != 0 ? _workQueue.Dequeue() : (WorkItem?)null;
                }

                if (next.HasValue)
                {
                    Console.WriteLine("doing work!");

                    // pulling work off the queue succeeded; do it.
                    WorkItem work = next.Value;

                    work.Request.Execute();
                    //Console.WriteLine("result is {0}", result);
                    //work.Issuer.Endpoint.Write(result);
                }
                else
                {
                    // pulling work off the queue failed; take a nap.
                    Thread.Sleep(100);
                }
            }
        }

        private Queue<WorkItem> _workQueue;

        private Task _task;
        private CancellationTokenSource _canceler;
    }
}
