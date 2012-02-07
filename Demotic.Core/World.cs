using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Demotic.Core.ObjectSystem;

namespace Demotic.Core
{
    public class World
    {
        public World()
        {
            _scripts = new List<IScript>();
            _stopLoopSource = new CancellationTokenSource();

            GlobalObjectRoot = new DRecord();
            GlobalObjectRoot["test"] = new DNumber(3);
        }

        public void Start()
        {
            _loopTask = Task.Factory.StartNew(
                            (() => ScriptLoop(_stopLoopSource.Token)),
                            _stopLoopSource.Token,
                            TaskCreationOptions.LongRunning,
                            TaskScheduler.Default
                        );
        }

        public void Stop()
        {
            _stopLoopSource.Cancel();
            _loopTask.Wait();
        }

        public void RegisterScript(IScript s)
        {
            _scripts.Add(s);
        }

        private void ScriptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (IScript s in _scripts)
                {
                    if (s.IsTriggered(this))
                    {
                        s.Run(this);
                    }
                }

                Thread.Sleep(100);
            }
        }

        private Task _loopTask;
        private CancellationTokenSource _stopLoopSource;

        private List<IScript> _scripts;

        public DRecord GlobalObjectRoot { get; private set; }
    }
}
