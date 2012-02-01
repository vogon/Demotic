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

            // TODO: add connect/disconnect interface
            DemoticApp app = (DemoticApp)DemoticApp.Current;
            app.Connect("localhost", 17717);

            _dipConsole = new DipConsolePage();

            ContentFrame.Navigate(_dipConsole);
        }

        private Client _client;

        private DipConsolePage _dipConsole;
    }
}
