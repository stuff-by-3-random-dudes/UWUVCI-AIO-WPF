using System;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UWUVCI_AIO_WPF.Classes
{
    public class MenuIconImage : IDisposable
    {
        private bool disposed = false;

        private Bitmap _frame;
        private Bitmap _titleScreen;

        public string _imageVar;
        public Rectangle _rectangleGBA = new Rectangle(3, 17, 122, 81);
        public Rectangle _rectangleH4V3 = new Rectangle(3, 9, 122, 92);
        public Rectangle _rectangleWII = new Rectangle(0, 23, 128, 94);

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

        public MenuIconImage()
        {
            _frame = null;
            _titleScreen = null;
        }

        ~MenuIconImage()
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
            if (!disposed)
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

        public Bitmap Create(string console)
        {
            Bitmap img = new Bitmap(128, 128);
            Graphics g = Graphics.FromImage(img);
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.FromArgb(30, 30, 30));

            _imageVar = "_rectangle" + console;
            Rectangle rectangle;
            try
            {
                rectangle = (Rectangle)GetType().GetField(_imageVar).GetValue(this);
            }
            catch
            {
                //if rectangle isn't able to get set then H4V3 should be used.
                rectangle = _rectangleH4V3;
            }

            if (TitleScreen != null)
                g.DrawImage(TitleScreen, rectangle);
            else
                g.FillRectangle(new SolidBrush(Color.Black), rectangle);

            /*
             * commented this section out because it doesn't seem to be doing anything right now, but maybe it was suppose to?
            else if (console == "NDS")
            {
                if (TitleScreen != null)
                {
                    if (TitleScreen.Width > TitleScreen.Height)
                        g.DrawImage(TitleScreen, rectangleH4V3);
                    else if (TitleScreen.Width < TitleScreen.Height)
                    {
                      
                        g.DrawImage(TitleScreen, rectangleH4V3);
                    }
                    else
                    {
                        g.DrawImage(TitleScreen, rectangleH4V3);
                       
                    }
                }
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectangleH4V3);
            */
            if (Frame == null)
            {
                GraphicsPath vc = new GraphicsPath();
                Font font = new Font("Arial", 10.0F, FontStyle.Regular, GraphicsUnit.Point);
                StringFormat format = new StringFormat();
                format.Alignment = StringAlignment.Center;
                format.LineAlignment = StringAlignment.Center;

                vc.AddString("Virtual Console.", font.FontFamily,
                (int)(FontStyle.Bold | FontStyle.Italic),
                g.DpiY * 9.2F / 72.0F, new Rectangle(0, 101, 128, 27), format);
                g.DrawPath(new Pen(Color.Black, 2.0F), vc);
                g.FillPath(new SolidBrush(Color.FromArgb(147, 149, 152)), vc);
            }
            else
                g.DrawImage(Frame, new Rectangle(0, 0, 128, 128));

            return img;
        }
    }
}
