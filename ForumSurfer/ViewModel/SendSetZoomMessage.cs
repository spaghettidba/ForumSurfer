using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.ViewModel
{
    class SendSetZoomMessage
    {

        public int Zoom { get; set; } = 100;

        public SendSetZoomMessage(int zoom)
        {
            this.Zoom = zoom;
        }
    }
}
