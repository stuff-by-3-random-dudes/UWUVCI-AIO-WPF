using GameBaseClassLibrary;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UWUVCI_AIO_WPF.Classes;
using UWUVCI_AIO_WPF.Properties;
using UWUVCI_AIO_WPF.UI.Windows;

namespace UWUVCI_AIO_WPF
{
    class MainViewModel : BaseModel
    {
        //public GameConfig GameConfiguration { get; set; }
        private GameConfig gameConfiguration = new GameConfig();

        
        public GameConfig GameConfiguration
        {
            get { return gameConfiguration; }
            set
            {
                gameConfiguration = value;
                OnPropertyChanged();
            }
        }
        private string romPath;

        public string RomPath
        {
            get { return romPath; }
            set { romPath = value;
                OnPropertyChanged();
            }
        }


        private GameBases gbTemp;

        public GameBases GbTemp
        {
            get { return gbTemp; }
            set { gbTemp = value; }
        }



        private List<string> lGameBasesString = new List<string>();

        public List<string> LGameBasesString
        {
            get { return lGameBasesString; }
            set
            {
                lGameBasesString = value;
                OnPropertyChanged();
            }
        }
        private bool pathsSet { get; set; } = false;

        public bool PathsSet
        {
            get { return pathsSet; }
            set
            {
                pathsSet = value;
                OnPropertyChanged();
            }
        }

        private List<GameBases> lBases = new List<GameBases>();

        public List<GameBases> LBases
        {
            get { return lBases; }
            set { lBases = value;
                OnPropertyChanged();
            }
        }

        #region TKLIST
        private List<GameBases> lNDS = new List<GameBases>();

        public List<GameBases> LNDS
        {
            get { return lNDS; }
            set { lNDS = value; OnPropertyChanged(); }
        }

        private List<GameBases> lN64 = new List<GameBases>();

        public List<GameBases> LN64
        {
            get { return lN64; }
            set { lN64 = value; OnPropertyChanged(); }
        }

        private List<GameBases> lNES = new List<GameBases>();
            
        public List<GameBases> LNES
        {
            get { return lNES; }
            set { lNES = value; OnPropertyChanged(); }
        }
        private List<GameBases> lGBA = new List<GameBases>();

        public List<GameBases> LGBA
        {
            get { return lGBA; }
            set { lGBA = value; OnPropertyChanged(); }
        }

        private List<GameBases> lSNES = new List<GameBases>();

        public List<GameBases> LSNES
        {
            get { return lSNES; }
            set { lSNES = value; OnPropertyChanged(); }
        }

        private List<GameBases> ltemp = new List<GameBases>();

        public List<GameBases> Ltemp
        {
            get { return ltemp; }
            set { ltemp = value; OnPropertyChanged(); }
        }
        #endregion


        public MainViewModel()
        {
            

            GameConfiguration = new GameConfig();
            UpdatePathSet(Properties.Settings.Default.PathsSet);
            GetAllBases();
        }

        public void UpdatePathSet(bool newValue)
        {
            PathsSet = newValue;
        }

        public void GetBases(GameConsoles Console)
        {
            List<GameBases> lTemp = new List<GameBases>();
            string vcbpath = $@"bases/bases.vcb{Console.ToString().ToLower()}";
            lTemp = VCBTool.ReadBasesFromVCB(vcbpath);
            LBases.Clear();
            GameBases custom = new GameBases();
            custom.Name = "Custom";
            custom.Region = Regions.EU;
            LBases.Add(custom);
            foreach(GameBases gb in lTemp)
            {
                LBases.Add(gb);
            }
            lGameBasesString.Clear();
            foreach(GameBases gb in LBases)
            {
                LGameBasesString.Add($"{gb.Name} {gb.Region}");
            }
        }
        
        private void GetAllBases()
        {
            LN64.Clear();
            LNDS.Clear();
            LNES.Clear();
            LSNES.Clear();
            LGBA.Clear();
            lNDS = VCBTool.ReadBasesFromVCB($@"bases/bases.vcbnds");
            lNES = VCBTool.ReadBasesFromVCB($@"bases/bases.vcbnes");
            lSNES = VCBTool.ReadBasesFromVCB($@"bases/bases.vcbsnes");
            lN64 = VCBTool.ReadBasesFromVCB($@"bases/bases.vcbn64");
            lGBA = VCBTool.ReadBasesFromVCB($@"bases/bases.vcbgba");
            CreateSettingIfNotExist(lNDS, GameConsoles.NDS);
            CreateSettingIfNotExist(lNES, GameConsoles.NES);
            CreateSettingIfNotExist(lSNES, GameConsoles.SNES);
            CreateSettingIfNotExist(lGBA, GameConsoles.GBA);
            CreateSettingIfNotExist(lN64, GameConsoles.N64);
        }
        private void CreateSettingIfNotExist(List<GameBases> l, GameConsoles console)
        {
            string file = $@"keys\{console.ToString().ToLower()}.vck";
            if (!File.Exists(file))
            {
                List<TKeys> temp = new List<TKeys>();
                foreach(GameBases gb in l)
                {
                    TKeys tempkey = new TKeys();
                    tempkey.Base = gb;
                    temp.Add(tempkey);
                }
                KeyFile.ExportFile(temp, console);
            }
                       
        }
        public void getTempList(GameConsoles console)
        {
            switch (console)
            {
                case GameConsoles.NDS:
                    Ltemp = LNDS;
                    break;
                case GameConsoles.N64:
                    Ltemp = LN64;
                    break;
                case GameConsoles.GBA:
                    Ltemp = LGBA;
                    break;
                case GameConsoles.NES:
                    Ltemp = LNES;
                    break;
                case GameConsoles.SNES:
                    Ltemp = LSNES;
                    break;
            }
        }

        public void EnterKey(bool ck)
        {
            EnterKey ek = new EnterKey(ck);
            ek.ShowDialog();
        }
        public bool checkcKey(string key)
        {
            if (487391367 == key.GetHashCode())
            {
                Settings.Default.Ckey = key;
                Settings.Default.Save();
                
                return true;
            }
            return false;
        }
        public bool isCkeySet()
        {
            if (Settings.Default.Ckey.GetHashCode() == 487391367)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public bool checkKey(string key)
        {
            if(GbTemp.KeyHash == key.GetHashCode())
            {
                UpdateKeyInFile(key, $@"keys\{GetConsoleOfBase(gbTemp).ToString().ToLower()}.vck", GbTemp, GetConsoleOfBase(gbTemp));
                return true;
            }
            return false;
        }
        public void UpdateKeyInFile(string key, string file, GameBases Base, GameConsoles console)
        {
            if (File.Exists(file))
            {
                var temp = KeyFile.ReadBasesFromKeyFile(file);
                foreach(TKeys  t in temp)
                {
                    if(t.Base.Name == Base.Name && t.Base.Region == Base.Region)
                    {
                        t.Tkey = key;
                    }
                }
                File.Delete(file);
                KeyFile.ExportFile(temp, console);
            }
        }
        public bool isKeySet(GameBases bases)
        {
            var temp = KeyFile.ReadBasesFromKeyFile($@"keys\{GetConsoleOfBase(bases).ToString().ToLower()}.vck");
            foreach(TKeys t in temp)
            {
                if(t.Base.Name == bases.Name && t.Base.Region == bases.Region)
                {
                    if(t.Tkey != null)
                    {
                        return true;
                    }
                }
            }
            return false;

        }
        public GameConsoles GetConsoleOfBase(GameBases gb)
        {
            GameConsoles ret = new GameConsoles();
            bool cont = false;
            foreach(GameBases b in lNDS)
            {
                if(b == gb)
                {
                    ret = GameConsoles.NDS;
                    cont = true;
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lN64)
                {
                    if (b == gb)
                    {
                        ret = GameConsoles.N64;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lNES)
                {
                    if (b == gb)
                    {
                        ret = GameConsoles.NES;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lSNES)
                {
                    if (b == gb)
                    {
                        ret = GameConsoles.SNES;
                        cont = true;
                    }
                }
            }
            if (!cont)
            {
                foreach (GameBases b in lGBA)
                {
                    if (b == gb)
                    {
                        ret = GameConsoles.GBA;
                        cont = true;
                    }
                }
            }
            return ret;
        }
        public List<bool> getInfoOfBase(GameBases gb)
        {
            List<bool> info = new List<bool>();
            if (Directory.Exists($@"{Settings.Default.BasePath}\{gb.Name}[{gb.Region}]"))
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            if (isKeySet(gb))
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            if (isCkeySet())
            {
                info.Add(true);
            }
            else
            {
                info.Add(false);
            }
            return info;
        }
    }
}
