using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ForumSurfer.Data
{
    public class Repository
    {
        public const String DatabaseName = "ForumSurfer.sqlite";
        public static String DatabaseFolder
        {
            get
            {
                return Environment.ExpandEnvironmentVariables(@"%AppData%\ForumSurfer");
            }
        }
        
        public static String DatabasePath
        {
            get
            {
                return DatabaseFolder + "\\" + DatabaseName;
            }
        }

        public static String ConnectionString
        {
            get
            {
                return "Data Source=" + DatabasePath + ";Version=3;";
            }
        }
    }
}
