using System;

namespace UWUVCI_AIO_WPF.Models

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

        public bool CommunityIni { get; set; }

    }
}
