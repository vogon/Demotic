using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using Demotic.Core;
using Demotic.Core.ObjectSystem;
using Demotic.Network;

namespace Demotic.Client
{
    internal class ClientView
    {
        public ClientView(Client model)
        {
            _model = model;

            model.ResponseReceived += OnResponseReceived;
        }

        public void Send(Message msg, out Block p)
        {
            ScrollbackRequestView msgView = new ScrollbackRequestView(msg);

            _model.SendMessage(msg, msgView);

            p = msgView.Block;
        }

        private interface IRequestView
        {
            void Respond(Message msg);
        }

        private class ScrollbackRequestView : IRequestView
        {
            public ScrollbackRequestView(Message request)
            {
                _p = new Paragraph();
                _p.Inlines.Add(new Bold(new Run("=> " + request.ToString())));
            }

            public void Respond(Message msg)
            {
                _p.Dispatcher.Invoke((Action<Message>)_Respond, msg);
            }

            private void _Respond(Message msg)
            {
                _p.Inlines.Add(new LineBreak());
                _p.Inlines.Add(new Run("<= " + msg.ToString()));
            }

            public Block Block
            {
                get
                {
                    return _p;
                }
            }

            private Paragraph _p;
        }

        private void OnResponseReceived(object sender, ResponseReceivedEventArgs e)
        {
            IRequestView view = (IRequestView)e.Context;
            view.Respond(e.Response);
        }

        private Client _model;
    }
}
