using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Drawing.Text;

namespace UWUVCI_AIO_WPF.Classes
{
    public class BootLogoImage : IDisposable
    {
        private bool disposed = false;

        private Bitmap _frame;
        private Bitmap _titleScreen;

        private static readonly string FontPath = @"bin\Tools\font2.ttf";
        private static readonly PrivateFontCollection PrivateFonts = new PrivateFontCollection();

        static BootLogoImage()
        {
            PrivateFonts.AddFontFile(FontPath);
        }

        public Bitmap Frame
        {
            get => _frame;
            set
            {
                _frame?.Dispose();
                _frame = value;
            }
        }

        public Bitmap TitleScreen
        {
            get => _titleScreen;
            set
            {
                _titleScreen?.Dispose();
                _titleScreen = value;
            }
        }

        public BootLogoImage()
        {
            _frame = null;
            _titleScreen = null;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    _frame?.Dispose();
                    _titleScreen?.Dispose();
                }
                disposed = true;
            }
        }

        public Bitmap Create(string text, float fontsize)
        {
            Bitmap img = new Bitmap(170, 42);
            using (Graphics g = Graphics.FromImage(img))
            {
                StringFormat format1 = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center
                };

                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.Clear(Color.FromArgb(30, 30, 30));
                g.DrawImage(Frame, 0, 0, 170, 42);

                Rectangle rectangletxt = new Rectangle(18, 5, 134, 32);

                Font font = new Font(PrivateFonts.Families[0], fontsize, FontStyle.Bold, GraphicsUnit.Pixel);

                TextRenderer.DrawText(g, text, font, rectangletxt, Color.FromArgb(180, 180, 180), Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.GlyphOverhangPadding);
            }
            return img;
        }
    }
}
