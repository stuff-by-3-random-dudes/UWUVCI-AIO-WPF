using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF
{
    class MainViewModel : BaseModel
    {
        //public GameConfig GameConfiguration { get; set; }
        private GameConfig gameConfiguration;

        public GameConfig GameConfiguration
        {
            get { return gameConfiguration; }
            set
            {
                gameConfiguration = value;
                OnPropertyChanged();
            }
        }

        public bool PathsSet { get; set; } = false;

        public MainViewModel()
        {
            GameConfiguration = new GameConfig();
        }
    }
}
