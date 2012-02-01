using System;
using System.Reflection;

using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Server
{
    abstract class UserAction
    {
        public UserAction(RequestContext client)
        {
            Client = client;
        }

        public abstract void Execute();

        protected RequestContext Client { get; set; }
    }

    class GetObjectAction : UserAction
    {
        public GetObjectAction(RequestContext client, string path) 
            : base(client)
        {
            Path = path;
        }

        public override void Execute()
        {
            Client.QuoteObject(Program.World.GlobalObjectRoot.Get(Path));
            Client.Acknowledge();
        }

        private string Path { get; set; }
    }

    class PutObjectAction : UserAction
    {
        public PutObjectAction(RequestContext client, string path, DObject value)
            : base(client)
        {
            Path = path;
            Value = value;
        }

        public override void Execute()
        {
            Program.World.GlobalObjectRoot.Set(Path, Value);
            Client.Acknowledge();
            //Client.PutMessage(string.Format("put {0} to path {1}.", Value, Path));
        }

        private string Path { get; set; }
        private DObject Value { get; set; }
    }

    class DoScriptAction : UserAction
    {
        public DoScriptAction(RequestContext client, string trigger, string effect)
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
            catch (Exception)
            {
                // TODO: remove pokemon exception handling
                Client.NegativeAcknowledge();
                //Client.PutMessage(string.Format("compilation failed: {0}", e.Message));
                return;
            }

            Program.World.RegisterScript(s);
            //Client.PutMessage("script registered successfully.");
            Client.Acknowledge();
        }

        private string Trigger { get; set; }
        private string Effect { get; set; }
    }
}
