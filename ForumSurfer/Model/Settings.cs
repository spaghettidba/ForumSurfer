using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Model
{
    public class Settings
    {

        public int RetentionDays { get; set; }
        public int AutoUpdateMinutes { get; set; }

        public Settings()
        {
            RetentionDays = 30;
            AutoUpdateMinutes = 1;
        }

        public Settings(Settings s) : this()
        {
            RetentionDays = s.RetentionDays;
            AutoUpdateMinutes = s.AutoUpdateMinutes;
        }
    }
}
