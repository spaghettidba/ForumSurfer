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
using System.Windows.Forms;

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

                    System.Threading.Thread.Sleep(10);

                    try
                    {
                        sendTextViaInnerText(document, action.Text);
                    }
                    catch(Exception)
                    {
                        try
                        {
                            sendTextViaClipboard(document, action.Text);
                        }
                        catch(Exception)
                        {
                            throw;
                        }
                        
                    }

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


        private void sendTextViaInnerText(dynamic document, string text)
        {
            Debug.Print("innerText: " + document.ActiveElement.innerText);
            document.ActiveElement.Focus();
            document.ActiveElement.innerText = text;
        }

        private void sendTextViaClipboard(dynamic document, string text)
        {
            System.Windows.Clipboard.SetText(text);
            System.Threading.Thread.Sleep(10);
            document.ExecCommand("Paste", false, null);
        }


        private void sendTextViaScript(dynamic document, string text)
        {
            HtmlElement head = document.GetElementsByTagName("head")[0];
            HtmlElement s = document.CreateElement("script");
            s.SetAttribute("text", "function ___fillIn() { document.activeElement.innerText = '"+ text +"'; }");
            head.AppendChild(s);
            document.InvokeScript("___fillIn");
        }
    }
}
