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

namespace Demotic.Client
{
    /// <summary>
    /// Interaction logic for ScriptEditorPage.xaml
    /// </summary>
    public partial class ScriptEditorPage : Page
    {
        public ScriptEditorPage()
        {
            InitializeComponent();

            _vm = new ScriptEditorViewModel(((DemoticApp)DemoticApp.Current).Client);
        }

        private void OnSubmitButtonClick(object sender, RoutedEventArgs e)
        {
            _vm.Submit(TriggerTextBox.Text, BodyTextBox.Text);
        }

        private ScriptEditorViewModel _vm;
    }
}
