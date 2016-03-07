using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ForumSurfer.ViewModel
{
    public class BoilerplateAnswer : Model.BindableBase
    {

        private Data.Boilerplate _boilerplate;
        private Action MenuAction { get; set; }
        
        public long Id
        {
            get { return _boilerplate.Id; }
        }

        public String Title
        {
            get
            {
                return _boilerplate.Title;
            }

            set
            {
                _boilerplate.Title = value;
            }
        }

        public String Text
        {
            get
            {
                return _boilerplate.Text;
            }

            set
            {
                _boilerplate.Text = value;
            }
        }

        private RelayCommand _menuActionCommand;


        public RelayCommand MenuActionCommand
        {
            get
            {
                return _menuActionCommand
                  ?? (_menuActionCommand = new RelayCommand(
                    () =>
                    {
                        MenuAction.Invoke();
                    }));
            }
        }

        public BoilerplateAnswer(Data.Boilerplate b, Action a)
        {
            MenuAction = a;
            _boilerplate = b;
        }
    }
}
