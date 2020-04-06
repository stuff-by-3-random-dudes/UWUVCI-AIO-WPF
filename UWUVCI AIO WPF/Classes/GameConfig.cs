using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF
{
    [Serializable]
    class GameConfig : BaseModel
    {
        public GameConsoles Console { get; set; }
        public GameBases BaseRom { get; set; }

        private string cBasePath;

        public string CBasePath
        {
            get { return cBasePath; }
            set { cBasePath = value;
                OnPropertyChanged();
            }
        }



        public string GameName { get; set; }


        public PNGTGA TGAIco { get; set; } = new PNGTGA();

        public PNGTGA TGADrc { get; set; } = new PNGTGA();
        public PNGTGA TGATv { get; set; } = new PNGTGA();
        public PNGTGA TGALog { get; set; } = new PNGTGA();
        public N64Conf N64Stuff { get; set; } = new N64Conf();

    }
}
