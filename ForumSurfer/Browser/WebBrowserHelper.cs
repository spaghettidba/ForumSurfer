using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ForumSurfer.Browser
{
    // helper calss to create AttachedProperty
    public static class WebBrowserHelper
    {
        public static readonly DependencyProperty LinkSourceProperty =
            DependencyProperty.RegisterAttached("LinkSource", typeof(string), typeof(WebBrowserHelper), new UIPropertyMetadata(null, LinkSourcePropertyChanged));

        public static string GetLinkSource(DependencyObject obj)
        {
            return (string)obj.GetValue(LinkSourceProperty);
        }

        public static void SetLinkSource(DependencyObject obj, string value)
        {
            obj.SetValue(LinkSourceProperty, value);
        }

        // When link changed navigate to site.
        public static void LinkSourcePropertyChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            var browser = o as WebBrowser;
            if (browser != null)
            {
                string uri = e.NewValue as string;
                browser.Source = uri != null ? new Uri(uri) : null;
            }
        }

    }
}
