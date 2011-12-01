using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Demotic.Core;
using Demotic.Core.ObjectSystem;

namespace Demotic.Server
{
    abstract class UserAction
    {
        public UserAction(IPresentationClient client)
        {
            Client = client;
        }

        public abstract void Execute();

        protected IPresentationClient Client { get; set; }
    }

    class GetObjectAction : UserAction
    {
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

    class PutNumberAction : UserAction
    {
        public PutNumberAction(IPresentationClient client, string path, int value)
            : base(client)
        {
            Path = path;
            Value = value;
        }

        public override void Execute()
        {
            Program.World.GlobalObjectRoot.Set(Path, new DNumber(Value));
            Client.PutMessage(string.Format("put {0} to path {1}.", Value, Path));
        }

        private string Path { get; set; }
        private int Value { get; set; }
    }

    class DoScriptAction : UserAction
    {
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
