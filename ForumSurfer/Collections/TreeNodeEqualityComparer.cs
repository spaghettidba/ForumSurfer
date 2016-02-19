using ForumSurfer.Model;
using ForumSurfer.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Collections
{
    public class TreeNodeEqualityComparer : IEqualityComparer<TreeNodeViewModel>
    {
        public bool Equals(TreeNodeViewModel x, TreeNodeViewModel y)
        {
            return x.Id.Equals(y.Id);
        }

        public int GetHashCode(TreeNodeViewModel obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
