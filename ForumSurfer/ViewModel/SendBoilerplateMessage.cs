using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.ViewModel
{
    public class SendBoilerplateMessage
    {
        public String Text { get; private set; }

        public SendBoilerplateMessage(String text)
        {
            this.Text = text;
        }
    }
}
