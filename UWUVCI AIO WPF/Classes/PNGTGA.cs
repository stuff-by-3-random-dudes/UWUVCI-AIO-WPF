using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UWUVCI_AIO_WPF.Classes
{
    class PNGTGA : BaseModel
    {
		private string imgPath;

		public string ImgPath
		{
			get { return imgPath; }
			set { imgPath = value;
				OnPropertyChanged();
			}
		}

		public byte[] ImgBin { get; set; }
    }
}
