using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MahApps.Metro.Controls;
using GalaSoft.MvvmLight.Messaging;
using ForumSurfer.ViewModel;
using System.Diagnostics;

namespace ForumSurfer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        public MainWindow()
        {
            Browser.BrowserHelper.SetBrowserFeatureControl();
            InitializeComponent();
            Messenger.Default.Register<SendBoilerplateMessage>(this, (action) => ReceiveMessage(action));
        }


        private void TreeViewItem_RequestBringIntoView(object sender, RequestBringIntoViewEventArgs e)
        {
            e.Handled = true;
        }

        private void TreeViewItem_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            ((TreeViewItem)sender).IsSelected = true;
            e.Handled = true;
        }


        private object ReceiveMessage(SendBoilerplateMessage action)
        {
            Debug.Print(action.Text);
            Clipboard.SetText(action.Text);
            wbFeed.Focus();
            dynamic document = wbFeed.Document;
            document.ExecCommand("Paste", false, null);
            return null;
        }
    }
}
