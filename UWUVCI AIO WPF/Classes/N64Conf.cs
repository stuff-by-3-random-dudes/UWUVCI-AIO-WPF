using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class N64Conf : BaseModel
    {
        private string iniPath;

        public string INIPath
        {
            get { return iniPath; }
            set { iniPath = value;
                OnPropertyChanged();
            }
        }
        private bool darkFilter;

        public bool DarkFilter
        {
            get { return darkFilter; }
            set { darkFilter = value;
                OnPropertyChanged();
            }
        }


        public byte[] INIBin { get; set; }

    }
}
