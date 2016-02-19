using ForumSurfer.Collections;
using GalaSoft.MvvmLight;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.ViewModel
{
    public class TreeViewModel : ViewModelBase
    {
        public SortableObservableCollection<TreeNodeViewModel> Items { get; set; }
    }
}
