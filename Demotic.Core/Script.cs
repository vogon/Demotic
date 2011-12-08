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
                        "System.Core",
                        Assembly.GetExecutingAssembly().Location,
                      });
            
        }

        public Script(string trigger, string effect)
        {
            _triggerSource = trigger;
            _effectSource = effect;
            _bindings = new Dictionary<World, WorldBinding>();
        }

        private struct WorldBinding
        {
            public Session Session { get; set; }

            public Submission<bool> Trigger { get; set; }
            public Submission<bool> Effect { get; set; }
        }

        public void BindTo(World world)
        {
            WorldBinding b = new WorldBinding();

            b.Session = Session.Create(world.GlobalObjectRoot);
            _engine.Execute("using System; using Demotic.Core; using Demotic.Core.ObjectSystem;", b.Session);

            b.Trigger = _engine.CompileSubmission<bool>(_triggerSource, b.Session, isInteractive: true);
            b.Effect = _engine.CompileSubmission<bool>(_effectSource, b.Session, isInteractive: false);

            _bindings[world] = b;
        }

        public bool IsTriggered(World world)
        {
            if (!_bindings.ContainsKey(world))
            {
                BindTo(world);
            }

            return _bindings[world].Trigger.Execute();
        }
        
        public void Run(World world)
        {
            if (!_bindings.ContainsKey(world))
            {
                BindTo(world);
            }

            _bindings[world].Effect.Execute();
        }

        private static ScriptEngine _engine;
        private string _triggerSource;
        private string _effectSource;

        private Dictionary<World, WorldBinding> _bindings;
    }
}
