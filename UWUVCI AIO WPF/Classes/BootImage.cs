using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Reflection;

namespace UWUVCI_AIO_WPF.Classes
{
    public class BootImage : IDisposable
    {
        private bool disposed = false;
        private Bitmap _frame;
        private Bitmap _titleScreen;
        private Font font;

        public string _imageVar;
        public Rectangle _rectangleGBA = new Rectangle(132, 260, 399, 266);
        public Rectangle _rectangleGBC = new Rectangle(183, 260, 296, 266);
        public Rectangle _rectangleH4V3 = new Rectangle(131, 249, 400, 300);
        public Rectangle _rectangleWII = new Rectangle(224, 200, 832, 333);

        public Bitmap Frame
        {
            set { _frame?.Dispose(); _frame = value; }
            get { return _frame; }
        }

        public Bitmap TitleScreen
        {
            set { _titleScreen?.Dispose(); _titleScreen = value; }
            get { return _titleScreen; }
        }

        public string NameLine1;
        public string NameLine2;
        public int Released;
        public int Players;
        public bool Longname;

        public BootImage()
        {
            NameLine1 = null;
            NameLine2 = null;
            Released = 0;
            Players = 0;
            Longname = false;
        }

        ~BootImage()
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
                    _frame?.Dispose();
                    _titleScreen?.Dispose();
                }
                disposed = true;
            }
        }

        private bool ContainsJapanese(string text)
        {
            foreach (char c in text)
            {
                if (char.GetUnicodeCategory(c) == UnicodeCategory.OtherLetter) // this covers Hiragana, Katakana, and Kanji
                    return true;
            }
            return false;
        }

        private Font GetFont()
        {
            try
            {
                var privateFonts = new PrivateFontCollection();
                privateFonts.AddFontFile(@"bin\Tools\font.otf");
                return new Font(privateFonts.Families[0], 10.0F, FontStyle.Regular, GraphicsUnit.Point);
            }
            catch
            {
                return new Font("Trebuchet MS", 10.0F, FontStyle.Bold, GraphicsUnit.Point);
            }
        }

        private Rectangle GetRectangleForConsole(string console)
        {
            _imageVar = "_rectangle" + console;
            try
            {
                var fieldInfo = GetType().GetField(_imageVar, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                return fieldInfo != null ? (Rectangle)fieldInfo.GetValue(this) : _rectangleH4V3;
            }
            catch
            {
                return _rectangleH4V3;
            }
        }

        private void DrawText(Graphics g, string text, Font font, Rectangle rectangle, Pen shadow, Pen outline, Brush brush)
        {
            using (var path = new GraphicsPath())
            {
                path.AddString(text, font.FontFamily, (int)FontStyle.Regular, g.DpiY * 25.0F / 72.0F, rectangle, new StringFormat());
                g.DrawPath(shadow, path);
                g.DrawPath(outline, path);
                g.FillPath(brush, path);
            }
        }

        public Bitmap Create(string console)
        {
            using (var img = new Bitmap(1280, 720))
            using (var g = Graphics.FromImage(img))
            {
                g.PixelOffsetMode = PixelOffsetMode.Half;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.TextRenderingHint = TextRenderingHint.ClearTypeGridFit;
                g.Clear(Color.White);

                font = GetFont();
                using (var brush = new SolidBrush(Color.FromArgb(32, 32, 32)))
                using (var outline = new Pen(Color.FromArgb(222, 222, 222), 4.0F))
                using (var shadow = new Pen(Color.FromArgb(190, 190, 190), 6.0F))
                {
                    var rectangle = GetRectangleForConsole(console);

                    if (TitleScreen != null)
                        g.DrawImage(TitleScreen, rectangle);
                    else
                        g.FillRectangle(Brushes.Black, rectangle);

                    if (Frame != null)
                        g.DrawImage(Frame, new Rectangle(0, 0, 1280, 720));

                    bool isNotEnglish = ContainsJapanese(NameLine1) || ContainsJapanese(NameLine2);

                    if (!string.IsNullOrEmpty(NameLine1))
                    {
                        DrawText(g, NameLine1, font, Longname ? new Rectangle(578, 313, 640, 50) : new Rectangle(578, 340, 640, 50), shadow, outline, brush);
                    }

                    if (!string.IsNullOrEmpty(NameLine2))
                    {
                        DrawText(g, NameLine2, font, new Rectangle(578, 368, 640, 50), shadow, outline, brush);
                    }

                    if (Released > 0)
                    {
                        var releasedString = isNotEnglish ? Released.ToString() + "年発売" : "Released: " + Released;
                        DrawText(g, releasedString, font, new Rectangle(586, 450, 600, 40), shadow, outline, brush);
                    }

                    if (Players > 0)
                    {
                        string pStr = Players >= 4 ? "1-4" : (Players == 1 ? "1" : "1-" + Players);
                        pStr = isNotEnglish ? "プレイ人数　" + pStr + "人" : "Players: " + pStr;
                        DrawText(g, pStr, font, new Rectangle(586, 496, 600, 40), shadow, outline, brush);
                    }

                    return img;
                }
            }
        }
    }
}
