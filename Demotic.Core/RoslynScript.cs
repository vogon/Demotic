﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Roslyn.Scripting;
using Roslyn.Scripting.CSharp;

namespace Demotic.Core
{
    public class RoslynScript : Demotic.Core.IScript
    {
        static RoslynScript()
        {
            _engine = new ScriptEngine(
                new[] { "System", 
                        "System.Core",
                        // TODO: remove hard-coded dependency
                        @"C:\Users\Colin Bayer\Documents\Demotic\Demotic.Server\bin\Debug\W3b.Sine.dll",
                        Assembly.GetExecutingAssembly().Location,
                      });
            
        }

        public RoslynScript(string trigger, string effect)
        {
            _triggerSource = trigger;
            _effectSource = effect;
            _bindings = new Dictionary<World, WorldBinding>();
        }

        private struct WorldBinding
        {
            public bool CompilationFailed { get; set; }

            public Session Session { get; set; }

            public Submission<bool> Trigger { get; set; }
            public Submission<bool> Effect { get; set; }
        }

        public void BindTo(World world)
        {
            WorldBinding b = new WorldBinding();

            b.Session = Session.Create(world.GlobalObjectRoot);
            _engine.Execute("using System; using Demotic.Core; using Demotic.Core.ObjectSystem; using W3b.Sine;", b.Session);

            b.Trigger = _engine.CompileSubmission<bool>(_triggerSource, b.Session, isInteractive: true);
            b.Effect = _engine.CompileSubmission<bool>(_effectSource, b.Session, isInteractive: false);

            _bindings[world] = b;
        }

        public bool IsTriggered(World world)
        {
            try
            {
                if (!_bindings.ContainsKey(world))
                {
                    BindTo(world);
                }
            }
            catch (Roslyn.Compilers.CompilationErrorException)
            {
                // suppress compilation errors and return false.
                _bindings[world] = new WorldBinding { CompilationFailed = true };
                
                return false;
            }

            if (!_bindings[world].CompilationFailed)
            {
                return _bindings[world].Trigger.Execute();
            }
            else
            {
                return false;
            }
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
