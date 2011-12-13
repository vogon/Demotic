using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Dip;

namespace Demotic.Server
{
    abstract class UserAction
    {
        [AttributeUsage(AttributeTargets.Method)]
        internal class IsAAttribute : Attribute
        {
        }

        [AttributeUsage(AttributeTargets.Method)]
        internal class MakeAAttribute : Attribute
        {
        }

        static UserAction()
        {
            _router = new MessageRouter<Func<IPresentationClient, dynamic, UserAction>>();

            Assembly asm = Assembly.GetExecutingAssembly();

            foreach (Type t in asm.GetTypes())
            {
                if (t.IsSubclassOf(typeof(UserAction)))
                {
                    Func<dynamic, bool> isa = null;
                    Func<IPresentationClient, dynamic, UserAction> makea = null;

                    foreach (MethodInfo mi in t.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic))
                    {
                        IsAAttribute[] isaattr =
                            (IsAAttribute[])mi.GetCustomAttributes(typeof(IsAAttribute), false);

                        if (isaattr.Length > 0)
                        {
                            isa = (Func<dynamic, bool>)Delegate.CreateDelegate(typeof(Func<dynamic, bool>), mi);
                        }

                        MakeAAttribute[] makeaattr =
                            (MakeAAttribute[])mi.GetCustomAttributes(typeof(MakeAAttribute), false);

                        if (makeaattr.Length > 0)
                        {
                            makea = (Func<IPresentationClient, dynamic, UserAction>)Delegate.CreateDelegate(typeof(Func<IPresentationClient, dynamic, UserAction>), mi);
                        }
                    }

                    if (isa != null && makea != null)
                    {
                        _router.AddVerifier(makea, isa);
                    }
                }
            }
        }

        public UserAction(IPresentationClient client)
        {
            Client = client;
        }

        public static UserAction Make(IPresentationClient client, DObject payload)
        {
            var f = _router.Lookup(payload);

            return (f == null) ? null : f.Invoke(client, payload);
        }

        public abstract void Execute();

        protected IPresentationClient Client { get; set; }

        protected static MessageRouter<Func<IPresentationClient, dynamic, UserAction>> _router;
    }

    class GetObjectAction : UserAction
    {
        [UserAction.IsA]
        private static bool IsGetMessage(dynamic m)
        {
            return (m.op == "get") && (!string.IsNullOrEmpty(m.path));
        }

        [UserAction.MakeA]
        private static UserAction MakeGetAction(IPresentationClient client, dynamic m)
        {
            return new GetObjectAction(client, m.path);
        }

        public GetObjectAction(IPresentationClient client, string path) 
            : base(client)
        {
            Path = path;
        }

        public override void Execute()
        {
            Client.PutObject(Program.World.GlobalObjectRoot.Get(Path));
        }

        private string Path { get; set; }
    }

    class PutObjectAction : UserAction
    {
        [UserAction.IsA]
        private static bool IsPutMessage(dynamic m)
        {
            return (m.op == "put") && (!string.IsNullOrEmpty(m.path)) &&
                (m.value != null);
        }

        [UserAction.MakeA]
        private static UserAction MakePutAction(IPresentationClient client, dynamic m)
        {
            return new PutObjectAction(client, m.path, m.value);
        }

        public PutObjectAction(IPresentationClient client, string path, DObject value)
            : base(client)
        {
            Path = path;
            Value = value;
        }

        public override void Execute()
        {
            Program.World.GlobalObjectRoot.Set(Path, Value);
            Client.PutMessage(string.Format("put {0} to path {1}.", Value, Path));
        }

        private string Path { get; set; }
        private DObject Value { get; set; }
    }

    class DoScriptAction : UserAction
    {
        [UserAction.IsA]
        private static bool IsDoMessage(dynamic m)
        {
            return (m.op == "do") && (!string.IsNullOrEmpty(m.trigger)) &&
                (!string.IsNullOrEmpty(m.effect));
        }

        [UserAction.MakeA]
        private static UserAction MakeDoAction(IPresentationClient client, dynamic m)
        {
            return new DoScriptAction(client, m.trigger, m.effect);
        }

        public DoScriptAction(IPresentationClient client, string trigger, string effect)
            : base(client)
        {
            Trigger = trigger;
            Effect = effect;
        }

        public override void Execute()
        {
            Script s = new Script(Trigger, Effect);

            try
            {
                s.BindTo(Program.World);
            }
            catch (Exception e)
            {
                // TODO: remove pokemon exception handling
                Client.PutMessage(string.Format("compilation failed: {0}", e.Message));
                return;
            }

            Program.World.RegisterScript(s);
            Client.PutMessage("script registered successfully.");
        }

        private string Trigger { get; set; }
        private string Effect { get; set; }
    }
}
