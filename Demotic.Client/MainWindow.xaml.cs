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

using System.Diagnostics;

using Demotic.Network;

namespace Demotic.Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            ScrollbackTextBox.Document.Blocks.Clear();

            _client = new Client("localhost", 17717);
            _client.Start();

            _view = new ClientView(_client);
        }

        private void promptTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            DipMessageFormat format = new DipMessageFormat();

            if (e.Text == "\r")
            {
                int consumed = 0;
                Message? msg = format.Decode(Encoding.UTF8.GetBytes(PromptTextBox.Text), out consumed);
                Block blk;

                _view.Send(msg.Value, out blk);

                ScrollbackTextBox.Document.Blocks.Add(blk);
                ScrollbackTextBox.ScrollToEnd();

                PromptTextBox.Clear();

                e.Handled = true;
            }
        }

        private Client _client;
        private ClientView _view;
    }
}
