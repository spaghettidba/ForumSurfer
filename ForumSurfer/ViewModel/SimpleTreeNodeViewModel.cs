using ForumSurfer.Collections;
using ForumSurfer.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.ViewModel
{
    public class SimpleTreeNodeViewModel : TreeNodeViewModel
    {
        private RSSNode _node;
        private SortableObservableCollection<TreeNodeViewModel> _children;
        private String _title;

        public RSSNode Node
        {
            get
            {
                return _node;
            }
            set
            {
                _node = value;
            }
        }

        public override string Title
        {
            get
            {
                if(_title != null)
                {
                    return _title;
                }
                else
                {
                    return _node.Title;
                }
            }
            set
            {
                _title = value;
            }
        }
        public override SortableObservableCollection<TreeNodeViewModel> Children
        {
            get
            {
                if (_children != null)
                {
                    return _children;
                }
                else
                {
                    _children = new SortableObservableCollection<TreeNodeViewModel>();
                    if (_node is Host)
                    {
                        Host h = (Host)_node;
                        foreach (Feed f in h.Feeds)
                        {
                            _children.Add(new SimpleTreeNodeViewModel(f));
                        }
                    }
                    _children.Sort(el => el.SortKey, ListSortDirection.Ascending);
                    return _children;
                }
            }

            set
            {
                _children = value;
            }
        }
        public override bool IsSelected { get; set; }

        public override object Id
        {
            get
            {
                return (_node==null)? new Uri("about:blank") : _node.Location;
            }

        }

        public override object SortKey
        {
            get
            {
                return (_node == null) ? null : _node.SortKey;
            }

        }

        public override int UnreadCount
        {
            get
            {
                int result = 0;
                if (_children == null)
                    result = 0;
                else
                {
                    if(_node is Host)
                    {
                        foreach(Feed f in ((Host)_node).Feeds)
                        {
                            result += (f.Articles.Count(el => el.Unread));
                        }
                        return result;
                    }
                    else if(_node is Feed)
                    {
                        result = (((Feed)_node).Articles.Count(el => el.Unread));
                    }
                }
                return result;
            }
        }



        public SimpleTreeNodeViewModel()
        {

        }

        public SimpleTreeNodeViewModel(RSSNode node)
        {
            this._node = node;
        }
    }
}
