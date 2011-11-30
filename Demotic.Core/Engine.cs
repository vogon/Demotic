using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Demotic.Core
{
    public class Engine
    {
        public Engine()
        {
            _scripts = new List<Script>();
            _stopLoopSource = new CancellationTokenSource();
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

        public void RegisterScript(Script s)
        {
            _scripts.Add(s);
        }

        private void ScriptLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                foreach (Script s in _scripts)
                {
                    if (s.IsTriggered)
                    {
                        s.Run();
                    }
                }

                Thread.Sleep(100);
            }
        }

        private Task _loopTask;
        private CancellationTokenSource _stopLoopSource;

        private List<Script> _scripts;
    }
}
