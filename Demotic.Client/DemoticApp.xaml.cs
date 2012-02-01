using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Windows;

namespace Demotic.Client
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class DemoticApp : Application
    {
        internal void Connect(string hostname, int port)
        {
            Client = new Client(hostname, port);
            Client.Start();
        }

        internal void Disconnect()
        {
            Client = null;
        }

        private void DemoticApp_Startup(object sender, StartupEventArgs e)
        {
        }

        internal Client Client { get; private set; }
    }
}
