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
        private bool darkFilter;

        public bool DarkFilter
        {
            get { return darkFilter; }
            set { darkFilter = value;
            }
        }


        public byte[] INIBin { get; set; }

    }
}
