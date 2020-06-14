using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Forms;

namespace UWUVCI_AIO_WPF.Classes
{
    public class BootLogoImage : IDisposable
    {
        private bool disposed = false;

        private Bitmap _frame;
        private Bitmap _titleScreen;

        public Bitmap Frame
        {
            set
            {
                if (_frame != null)
                    _frame.Dispose();
                _frame = value;
            }
            get { return _frame; }
        }
        public Bitmap TitleScreen
        {
            set
            {
                if (_titleScreen != null)
                    _titleScreen.Dispose();
                _titleScreen = value;
            }
            get { return _titleScreen; }
        }

        public BootLogoImage()
        {
            _frame = null;
            _titleScreen = null;
        }

        ~BootLogoImage()
        {
            Dispose(false);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!this.disposed)
            {
                if (disposing)
                {
                    if (Frame != null)
                    {
                        Frame.Dispose();
                        Frame = null;
                    }
                    if (TitleScreen != null)
                    {
                        TitleScreen.Dispose();
                        TitleScreen = null;
                    }
                }
                disposed = true;
            }
        }

        public Bitmap Create(string text, float fontsize)
        {
            Bitmap img = new Bitmap(170, 42);
            Graphics g = Graphics.FromImage(img);
            StringFormat format1 = new StringFormat();
            format1.Alignment = StringAlignment.Center;
            format1.LineAlignment = StringAlignment.Center;
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            //g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(System.Drawing.Color.FromArgb(30, 30, 30));
            g.DrawImage(Frame, 0, 0, 170, 42);
            Rectangle rectangletxt = new Rectangle(18, 5, 134, 32);

            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(@"bin\Tools\font2.ttf");

            Font font = new Font(privateFonts.Families[0], fontsize, System.Drawing.FontStyle.Bold, GraphicsUnit.Pixel);

            /*g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;*/
            // g.DrawString(text, font, new SolidBrush(System.Drawing.Color.FromArgb(180, 180, 180)), rectangletxt, format1);
            SizeF size = g.MeasureString(text, font);
            // g.DrawString(text, font, new SolidBrush(System.Drawing.Color.FromArgb(180, 180, 180)), (rectangletxt.Width - size.Width) / 2, (rectangletxt.Height - size.Height) / 2);
            TextRenderer.DrawText(g, text, font, rectangletxt, System.Drawing.Color.FromArgb(180, 180, 180), Color.White, TextFormatFlags.HorizontalCenter |
   TextFormatFlags.VerticalCenter |
   TextFormatFlags.GlyphOverhangPadding);
            return img;
        }
    }
}
