using ForumSurfer.Collections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.ViewModel
{
    public abstract class TreeNodeViewModel
    {
        public abstract Object Id { get; }
        public abstract Object SortKey { get; }
        public abstract String Title { get; set; }
        public abstract SortableObservableCollection<TreeNodeViewModel> Children { get; set; }
        public abstract bool IsSelected { get; set; }
        public abstract int UnreadCount { get; }
        //public abstract bool HasUnread { get; }
    }
}
