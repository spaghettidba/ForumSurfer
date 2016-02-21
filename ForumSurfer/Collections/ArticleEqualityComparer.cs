using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Collections
{

    public class ArticleEqualityComparer : IEqualityComparer<Model.Article>
    {
        public bool Equals(Model.Article x, Model.Article y)
        {
            return x.Location.Equals(y.Location);
        }

        public int GetHashCode(Model.Article obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}

