using System;

namespace UWUVCI_AIO_WPF.Classes
{
    [Serializable]
    public class PNGTGA
    {
		private string imgPath = null;

		public string ImgPath 
		{
			get { return imgPath; }
			set { imgPath = value;
			}
		}

		public byte[] ImgBin { get; set; } = null;

		public string extension { get; set; }
    }
}
