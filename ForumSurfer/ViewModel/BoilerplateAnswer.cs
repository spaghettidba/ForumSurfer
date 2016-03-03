using GalaSoft.MvvmLight.Command;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ForumSurfer.ViewModel
{
    public class BoilerplateAnswer
    {

        private Data.Boilerplate _boilerplate;
        private Action MenuAction { get; set; }
        
        public String Title
        {
            get
            {
                return _boilerplate.Title;
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
