using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Demotic.Core
{
    /// <summary>
    ///   The atom of self-amendment.  A script consists of a trigger and an effect.
    ///   It runs whenever the trigger is true, and carries out the actions described
    ///   by the effect.
    /// </summary>
    public class Script
    {
        static Script()
        {
            _engine = new ScriptEngine(
                new[] { "System", 
                        Assembly.GetExecutingAssembly().Location,
                      });
            
        }

        public Script(string trigger, string effect)
        {
            _session = Session.Create(EnvironmentFactory.CommonRoot);
            _engine.Execute("using System; using Demotic.Core; using Demotic.Core.ObjectSystem;", _session);

            _trigger = _engine.CompileSubmission<bool>(trigger, _session, isInteractive: true);
            _effect = _engine.CompileSubmission<bool>(effect, _session, isInteractive: false);
        }

        public bool IsTriggered
        {
            get
            {
                return _trigger.Execute();
            }
        }

        public void Run()
        {
            _effect.Execute();
        }

        private static ScriptEngine _engine;

        private Session _session;

        private Submission<bool> _trigger;
        private Submission<bool> _effect;
    }
}
