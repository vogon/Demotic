using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using IronPython;
using IronPython.Hosting;
using Microsoft.Scripting.Hosting;

namespace Demotic.Core
{
    public class IronPythonScript : IScript
    {
        public IronPythonScript(string trigger, string effect)
        {
            _trigger = trigger;
            _effect = effect;

            _engine = Python.CreateEngine();
        }

        public void BindTo(World world)
        {
            //throw new NotImplementedException();
            return;
        }

        public bool IsTriggered(World world)
        {
            //throw new NotImplementedException();

            ScriptScope sc = _engine.CreateScope();
            sc.SetVariable("r", world.GlobalObjectRoot);

            return _engine.Execute<bool>(_trigger, sc);
        }

        public void Run(World world)
        {
            //throw new NotImplementedException();

            ScriptScope sc = _engine.CreateScope();
            sc.SetVariable("r", world.GlobalObjectRoot);

            _engine.Execute(_effect);
        }

        private ScriptEngine _engine;
        private string _trigger;
        private string _effect;
    }
}
