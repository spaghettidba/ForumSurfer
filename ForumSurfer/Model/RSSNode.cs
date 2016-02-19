using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Model
{
    public interface RSSNode
    {
        String Title { get; set; }
        Uri Location { get; set;} 
        Object SortKey { get; }
    }
}
