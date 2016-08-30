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
    public class BoilerplateEditorViewModel : ViewModelBase
    {

        private Boilerplate _bp = new Boilerplate();
        private DialogCoordinator _dialogCoordinator;

        public bool Cancel = false;
        public Exception Exception;
        public Object Context;
        public BaseMetroDialog Dialog;

        public String Title
        {
            get
            {
                return _bp.Title;
            }
            set
            {
                _bp.Title = value;
            }
        }
        public String Text
        {
            get
            {
                return _bp.Text;
            }
            set
            {
                _bp.Text = value;
            }
        }


        public ICommand CancelCommand { get; set; }
        public ICommand OKCommand { get; set; }


        public BoilerplateEditorViewModel()
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
            Cancel = false;
            try
            {
                Data.Boilerplate bp = new Data.Boilerplate();
                bp.Text = _bp.Text;
                bp.Title = _bp.Title;
                bp.Save();
            }
            catch (Exception ex)
            {
                Exception = ex;
            }
            finally
            {
                await _dialogCoordinator.HideMetroDialogAsync(Context, Dialog);
                ((MainViewModel)Context).UpdateBoilerplate(_bp);
            }
        }
    }
}
