﻿using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;
using FontFamily = System.Drawing.FontFamily;
using FontStyle = System.Drawing.FontStyle;
using Pen = System.Drawing.Pen;

namespace UWUVCI_AIO_WPF.Classes
{
    public class BootImage : IDisposable
    {
        private bool disposed = false;

        private Bitmap _frame;
        private Bitmap _titleScreen;
        private Font font;
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
        public string NameLine1;
        public string NameLine2;
        public int Released;
        public int Players;
        public bool Longname;
        private bool bf = false;

        public BootImage()
        {
            _frame = null;
            _titleScreen = null;
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

        public Bitmap Create(string console)
        {
            Bitmap img = new Bitmap(1280, 720);
            Graphics g = Graphics.FromImage(img);
            g.PixelOffsetMode = PixelOffsetMode.Half;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.CompositingMode = CompositingMode.SourceOver;
            g.CompositingQuality = CompositingQuality.HighQuality;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            g.Clear(Color.White);
            System.Drawing.Text.PrivateFontCollection privateFonts = new System.Drawing.Text.PrivateFontCollection();
            privateFonts.AddFontFile(@"bin\Tools\font.otf");
            if (bf)
            {
                font = new Font("Trebuchet MS", 10.0F, System.Drawing.FontStyle.Bold, GraphicsUnit.Point);
            }
            else
            {
                font = new Font(privateFonts.Families[0], 10.0F, FontStyle.Regular, GraphicsUnit.Point);
                



            }

            SolidBrush brush = new SolidBrush(Color.FromArgb(32, 32, 32));
            Pen outline = new Pen(System.Drawing.Color.FromArgb(222, 222, 222), 4.0F);
            Pen shadow = new Pen(Color.FromArgb(190, 190, 190), 6.0F);
            StringFormat format = new StringFormat();
            Rectangle rectangleGBA = new Rectangle(132, 260, 399, 266);
            Rectangle rectangleGBC = new Rectangle(183, 260, 296, 266);
            Rectangle rectangleH4V3 = new Rectangle(131, 249, 400, 300);
            // Rectangle rectanglewii = new Rectangle(224, 201, 832, 332);
           Rectangle rectanglewii = new Rectangle(224, 200, 832, 333);
            if (console == "GBA")
            {
                if (TitleScreen != null)
                    g.DrawImage(TitleScreen, rectangleGBA);
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectangleGBA);
            }
            else if (console == "GBC")
            {
                if (TitleScreen != null)
                    g.DrawImage(TitleScreen, rectangleGBC);
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectangleGBC);
            }
            else if (console == "NDS")
            {
                if (TitleScreen != null)
                {
                    
                        g.DrawImage(TitleScreen, rectangleH4V3);
                       
                    
                }
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectangleH4V3);
            }
            else if(console == "WII")
            {
                if (TitleScreen != null)
                    g.DrawImage(TitleScreen, rectanglewii);
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectanglewii);
            }
            else 
            {
                if (TitleScreen != null)
                    g.DrawImage(TitleScreen, rectangleH4V3);
                else
                    g.FillRectangle(new SolidBrush(Color.Black), rectangleH4V3);
            }

            if (Frame != null)
                g.DrawImage(Frame, new Rectangle(0, 0, 1280, 720));

            if (NameLine1 != null && NameLine2 != null)
            {
                Pen outlineBold = new Pen(Color.FromArgb(222, 222, 222), 5.0F);
                Pen shadowBold = new Pen(Color.FromArgb(190, 190, 190), 7.0F);
                Rectangle rectangleNL1 = Longname ? new Rectangle(578, 313, 640, 50) : new Rectangle(578, 340, 640, 50);
                Rectangle rectangleNL2 = new Rectangle(578, 368, 640, 50);
                GraphicsPath nl1 = new GraphicsPath();
                GraphicsPath nl2 = new GraphicsPath();

                if (Longname)
                {
                    nl1.AddString(NameLine1, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL1, format);
                    g.DrawPath(shadowBold, nl1);
                    g.DrawPath(outlineBold, nl1);
                    g.FillPath(brush, nl1);
                    nl2.AddString(NameLine2, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL2, format);
                    g.DrawPath(shadowBold, nl2);
                    g.DrawPath(outlineBold, nl2);
                    g.FillPath(brush, nl2);
                }
                else
                {
                    nl1.AddString(NameLine1, font.FontFamily,
                        (int)(FontStyle.Bold),
                        g.DpiY * 37.0F / 72.0F, rectangleNL1, format);
                    g.DrawPath(shadowBold, nl1);
                    g.DrawPath(outlineBold, nl1);
                    g.FillPath(brush, nl1);
                }
            }

            if (Released > 0)
            {
                GraphicsPath r = new GraphicsPath();
                r.AddString("Released: " + Released.ToString(), font.FontFamily,
                    (int)(FontStyle.Regular),
                    g.DpiY * 25.0F / 72.0F, new Rectangle(586, 450, 300, 40), format);
                g.DrawPath(shadow, r);
                g.DrawPath(outline, r);
                g.FillPath(brush, r);
            }

            if (Players > 0)
            {
                string pStr = Players >= 4 ? "1-4" : Players == 3 ? "1-3" : Players == 2 ? "1-2" : "1";
                GraphicsPath p = new GraphicsPath();
                
                    p.AddString("Players: " + pStr, font.FontFamily,
                    (int)(FontStyle.Regular),
                    g.DpiY * 25.0F / 72.0F, new Rectangle(586, 496, 400, 40), format);
                
                
               g.DrawPath(shadow, p);
                g.DrawPath(outline, p);
                g.FillPath(brush, p);
            }

            return img;
        }
        public void changefont(bool t)
        {
            if (t)
            {
                bf = true;
            }
            else
            {
                bf = false;
            }
        }
    }
}