using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ratcow.DebugViewer
{

    public class Settings
    {
        public Settings()
        {
            Urls = new List<DebugTarget>();
        }

        public List<DebugTarget> Urls { get; set; }
    }

    public class DebugTarget
    {
        public DebugTarget()
        {
            Version = 100;
        }

        public int Version { get; set; }
        public string Url { get; set; }
        public string Description { get; set; }
    }
}
