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
using System.Windows.Threading;

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
            try
            {
                Clipboard.SetText(action.Text);

                Debug.Print(@"///////////////////////// BEGIN SEND /////////////////////////");
                Debug.Print(action.Text);
                wbFeed.Focus();
                dynamic document = wbFeed.Document;
                if (document != null)
                {
                    try
                    {
                        // HACK: I don't need these properties, but evaluating them
                        //       before accessing other properties I need seems to 
                        //       avoid issues with dynamic properties thru COM.
                        //       Go figure...
                        string chset = document.charset;
                        dynamic el = document.ActiveElement;
                        // /HACK
                    }
                    catch(Exception ex)
                    {
                        Debug.Print(ex.StackTrace);
                        string chset = document.charset;
                    }

                    System.Threading.Thread.Sleep(100);
                    if(document.ActiveElement != null)
                        document.ActiveElement.Focus();
                    document.ExecCommand("Paste", false, null);
                }
                else
                {
                    Debug.Print(@"-------- SORRY, NO DOCUMENT --------------");
                }

                Debug.Print(@"\\\\\\\\\\\\\\\\\\\\\\\\\ END SEND \\\\\\\\\\\\\\\\\\\\\\\\\");

            }
            catch (Exception e)
            {
                // Ignore
                Debug.Print(e.StackTrace);
                Debug.Print(@"------------------------- EXCEPTION SEND -------------------------");

            }
            return null;
        }
    }
}
