using ForumSurfer.Data;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using MahApps.Metro.Controls.Dialogs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace ForumSurfer.ViewModel
{
    public class ZoomEditorViewModel : ViewModelBase
    {

        private DialogCoordinator _dialogCoordinator;

        public bool Cancel = false;
        public Exception Exception;
        public Object Context;
        public BaseMetroDialog Dialog;
        public Model.Host Host = new Model.Host();

        public int PageZoom { get { return Host.Zoom; } set { Host.Zoom = value; } }
        public int TextZoom { get { return Host.TextZoom; } set { Host.TextZoom = value; } }


        public ICommand CancelCommand { get; set; }
        public ICommand OKCommand { get; set; }


        public ZoomEditorViewModel()
        {
            CancelCommand = new RelayCommand<RoutedEventArgs>(Cancel_Pressed);
            OKCommand = new RelayCommand<RoutedEventArgs>(OK_Pressed);
            _dialogCoordinator = DialogCoordinator.Instance;
            Cancel = false;
            Exception = null;
        }


        private async void Cancel_Pressed(RoutedEventArgs e)
        {
            Cancel = true;
            await _dialogCoordinator.HideMetroDialogAsync(Context, Dialog);
        }

        private async void OK_Pressed(RoutedEventArgs e)
        {
            {
                await _dialogCoordinator.HideMetroDialogAsync(Context, Dialog);
                ((MainViewModel)Context).UpdateZoom(PageZoom, TextZoom, Host);
            }
        }
    }
}
