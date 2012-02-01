using System;
using System.Collections.Generic;
using System.Windows.Controls;
using System.Windows.Documents;

using Demotic.Network;
using System.Windows.Input;
using System.Text;

namespace Demotic.Client
{
    /// <summary>
    /// Interaction logic for DipConsolePage.xaml
    /// </summary>
    public partial class DipConsolePage : Page
    {
        public DipConsolePage()
        {
            InitializeComponent();

            _consoleVm = new DipConsoleViewModel(((DemoticApp)DemoticApp.Current).Client);
            _conversations = new List<ConversationView>();
        }

        private void PromptTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DipMessageFormat format = new DipMessageFormat();

            if (e.Text == "\r")
            {
                int consumed = 0;
                Message msg = format.Decode(Encoding.UTF8.GetBytes(PromptTextBox.Text), out consumed);
                
                ConversationViewModel convoVm = _consoleVm.StartConversation(msg);
                ConversationView convoView = new ConversationView(convoVm);

                ScrollbackTextBox.Document.Blocks.Add(convoView.View);
                _conversations.Add(convoView);

                ScrollbackTextBox.ScrollToEnd();

                PromptTextBox.Clear();

                e.Handled = true;
            }
        }

        private DipConsoleViewModel _consoleVm;
        private List<ConversationView> _conversations;

        private class ConversationView
        {
            public ConversationView(ConversationViewModel vm)
            {
                if (vm == null)
                {
                    throw new ArgumentNullException("vm");
                }

                _viewModel = vm;
                
                BuildEmptyView();

                IEnumerable<Inline> requestView = MakeRequestView(_viewModel.Request);
                _view.Inlines.AddRange(requestView);
                _requestView.AddRange(requestView);

                vm.PropertyChanged += 
                    ((sender, e) => 
                        _view.Dispatcher.Invoke((Action)RefreshResponseViews, args: new object[0]));
            }

            private void BuildEmptyView()
            {
                _view = new Paragraph();
                _requestView = new List<Inline>();
                _responseViews = new List<Inline>();
            }

            private IEnumerable<Inline> MakeRequestView(Message request)
            {
                return new Inline[] 
                {
                    new Bold(new Run("=> " + MessageToDisplayString(request))),
                };
            }

            private string MessageToDisplayString(Message msg)
            {
                switch (msg.OpCode)
                {
                    case MessageOpCode.GetObject:
                        return string.Format("get {0}", msg.Attributes["path"].ToString());
                    case MessageOpCode.NG:
                        return "error";
                    case MessageOpCode.OK:
                        return "ok";
                    case MessageOpCode.PutObject:
                        return string.Format("{0} = {1}", msg.Attributes["path"].ToString(),
                            msg.Attributes["value"]);
                    case MessageOpCode.Quote:
                        return msg.Attributes["object"].ToString();
                    default:
                        return "???";
                }
            }

            private IEnumerable<Inline> MakeResponseView(Message response)
            {
                return new Inline[]
                {
                    new LineBreak(),
                    new Run("<= " + MessageToDisplayString(response)),
                };
            }

            private void RefreshResponseViews()
            {
                // clear the toplevel view and add the request view back in.
                _view.Inlines.Clear();
                _responseViews.Clear();

                _view.Inlines.AddRange(_requestView);
                
                // now build new views for each response in the viewmodel.
                foreach (Message msg in _viewModel.Responses)
                {
                    IEnumerable<Inline> responseView = MakeResponseView(msg);

                    _view.Inlines.AddRange(responseView);
                    _responseViews.AddRange(responseView);
                }
            }

            public Block View
            {
                get { return _view; }
            }

            private Paragraph _view;
            private List<Inline> _requestView;
            private List<Inline> _responseViews;

            private ConversationViewModel _viewModel;
        }
    }
}
