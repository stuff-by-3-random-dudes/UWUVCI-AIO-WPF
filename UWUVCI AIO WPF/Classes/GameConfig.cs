using GameBaseClassLibrary;
using System;
using UWUVCI_AIO_WPF.Classes;

namespace UWUVCI_AIO_WPF
{
    [Serializable]
    public class GameConfig
    {
        public GameConfig Clone()
        {
            return this.MemberwiseClone() as GameConfig;
        }
        public GameConsoles Console { get; set; }
        public GameBases BaseRom { get; set; }

        private string cBasePath;

        public string CBasePath
        {
            get { return cBasePath; }
            set { cBasePath = value;

            }
        }

        public byte[] bootsound;
        public string extension = "";
        public bool fourbythree = false;
        public bool disgamepad = false;
        public bool donttrim = false;
        public bool lr = false;
        public bool motepass = false;
        public bool jppatch = false;
        public bool pokepatch = false;

        public bool tgcd = false;

        public int Index;

        public bool pixelperfect = false;
        public string GameName { get; set; }

        public bool vm = false;
        public bool vmtopal = false;


        public bool rf = false;
        public bool rfus = false;
        public bool rfjp = false;



        public PNGTGA TGAIco { get; set; } = new PNGTGA();



        public PNGTGA TGADrc { get; set; } = new PNGTGA();
        public PNGTGA TGATv { get; set; } = new PNGTGA();
        public PNGTGA TGALog { get; set; } = new PNGTGA();
        public N64Conf N64Stuff { get; set; } = new N64Conf();

    }
}
