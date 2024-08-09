using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace UWUVCI_AIO_WPF.Classes
{
    public class MenuIconImage : IDisposable
    {
        private bool disposed = false;
        private Bitmap _frame;
        private Bitmap _titleScreen;

        private static readonly Dictionary<string, Rectangle> ConsoleRectangles = new Dictionary<string, Rectangle>
        {
            { "GBA", new Rectangle(3, 17, 122, 81) },
            { "H4V3", new Rectangle(3, 9, 122, 92) },
            { "WII", new Rectangle(0, 23, 128, 94) }
        };

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

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            if (disposing)
            {
                _frame?.Dispose();
                _titleScreen?.Dispose();
            }

            disposed = true;
        }

        public Bitmap Create(string console)
        {
            Bitmap img = new Bitmap(128, 128);
            using (Graphics g = Graphics.FromImage(img))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.FromArgb(30, 30, 30));

                Rectangle rectangle = ConsoleRectangles.ContainsKey(console) ? ConsoleRectangles[console] : ConsoleRectangles["H4V3"];

                if (TitleScreen != null)
                    g.DrawImage(TitleScreen, rectangle);
                else
                    g.FillRectangle(Brushes.Black, rectangle);

                if (Frame == null)
                {
                    using (GraphicsPath vc = new GraphicsPath())
                    {
                        Font font = new Font("Arial", 10.0F, FontStyle.Regular, GraphicsUnit.Point);
                        StringFormat format = new StringFormat
                        {
                            Alignment = StringAlignment.Center,
                            LineAlignment = StringAlignment.Center
                        };

                        vc.AddString("Virtual Console", font.FontFamily, (int)(FontStyle.Bold | FontStyle.Italic),
                            g.DpiY * 9.2F / 72.0F, new Rectangle(0, 101, 128, 27), format);
                        g.DrawPath(Pens.Black, vc);
                        g.FillPath(new SolidBrush(Color.FromArgb(147, 149, 152)), vc);
                    }
                }
                else
                {
                    g.DrawImage(Frame, new Rectangle(0, 0, 128, 128));
                }
            }

            return img;
        }
    }
}
