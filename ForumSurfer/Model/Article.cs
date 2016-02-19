using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Model
{
    public class Article : RSSNode
    {
        public Uri Location { get; set; }
        public DateTime PublishDate { get; set; }
        public String DisplayDate
        {
            get
            {
                if (PublishDate > DateTime.Now.Date)
                {
                    return PublishDate.ToString("HH:mm");
                }
                else if (PublishDate > (DateTime.Now.Date.AddDays(-7)))
                {
                    return PublishDate.ToString("ddd HH:mm");
                }
                else
                {
                    return PublishDate.ToString(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
                }
            }
        }

        public Object SortKey
        {
            get
            {
                return PublishDate;
            }
        }
        

        public String Title { get; set; }
        public Boolean Unread { get; set; }


        public Article()
        {

        }

        public Article(Article a) : this()
        {
            this.Location = a.Location;
            this.PublishDate = a.PublishDate;
            this.Title = a.Title;
            this.Unread = a.Unread;
        }
    }
}
