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
            _scriptEditor = new ScriptEditorPage();

            _navigationButtons = new Button[] { BtnDipConsole, BtnObjectExplorer, BtnScriptEditor };

            DoNavigate(BtnDipConsole, _dipConsole);
        }

        private void OnPaneChangeClick(object sender, RoutedEventArgs e)
        {
            if (sender == BtnDipConsole)
            {
                DoNavigate(BtnDipConsole, _dipConsole);
            }
            else if (sender == BtnScriptEditor)
            {
                DoNavigate(BtnScriptEditor, _scriptEditor);
            }
        }

        private void DoNavigate(Button button, object content)
        {
            ContentFrame.Navigate(content);

            foreach (Button b in _navigationButtons)
            {
                b.IsEnabled = (b != button);
            }
        }

        private readonly Button[] _navigationButtons;

        private DipConsolePage _dipConsole;
        private ScriptEditorPage _scriptEditor;
    }
}
