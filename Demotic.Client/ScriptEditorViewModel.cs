using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Client
{
    class ScriptEditorViewModel : DemoticViewModel
    {
        public ScriptEditorViewModel(Client client)
            : base(client)
        {
            _client = client;
        }

        public void Submit(string trigger, string body)
        {
            _currentScript = new ScriptSubmissionTask(trigger, body);
            _currentScript.Submit(_client);
        }

        private ScriptSubmissionTask _currentScript;
        private Client _client;

        class ScriptSubmissionTask
        {
            public ScriptSubmissionTask(string trigger, string body)
            {
                _trigger = trigger;
                _body = body;
            }

            public void Submit(Client client)
            {
                Message msg = new Message();
                
                msg.OpCode = MessageOpCode.DoScript;
                msg.Attributes["when"] = _trigger;
                msg.Attributes["do"] = _body;

                client.SendMessage(msg, null, OnResponseReceived);
            }

            private void OnResponseReceived(object sender, ResponseReceivedEventArgs e)
            {
                Debug.WriteLine("response received: {0}", e.Response);
            }

            private string _trigger;
            private string _body;
        }
    }

}
