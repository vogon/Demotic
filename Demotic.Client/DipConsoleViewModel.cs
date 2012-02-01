using System.Collections.Generic;
using System.ComponentModel;

using Demotic.Network;

namespace Demotic.Client
{
    internal class DipConsoleViewModel : DemoticViewModel
    {
        public DipConsoleViewModel(Client model) : base(model) {}

        public ConversationViewModel StartConversation(Message msg)
        {
            ConversationViewModel rvm = new ConversationViewModel(msg);
            Client.SendMessage(msg, rvm, ((sender, e) => rvm.OnResponse(e.Response)));

            return rvm;
        }
    }

    internal class ConversationViewModel : INotifyPropertyChanged
    {
        public ConversationViewModel(Message request)
        {
            Request = request;
            Responses = new List<Message>();
        }

        public Message Request { get; private set; }
        public IList<Message> Responses { get; private set; }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string info)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        public void OnResponse(Message msg)
        {
            Responses.Add(msg);
            NotifyPropertyChanged("Replies");
        }
    }
}
