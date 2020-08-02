using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
    
{
    [Serializable]
   public class N64Conf
    {
        private string iniPath = null;

        public string INIPath
        {
            get { return iniPath; }
            set { iniPath = value;
            }
        }
        private bool darkFilter = false;

        public bool DarkFilter
        {
            get { return darkFilter; }
            set { darkFilter = value;
            }
        }

        private bool wideScreen = false;

        public bool WideScreen
        {
            get { return wideScreen; }
            set { wideScreen = value; }
        }

        public byte[] INIBin { get; set; }

    }
}
