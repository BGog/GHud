using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GHud
{

    // Device base class
    public class Device: IDisposable
    {
        public bool valid;
        
        // Device physical parameters
        public int width;
        public int height;
        public bool is_color;

        // Backdrops are an alpha blended background
        public bool use_backdrops;
        
        // Rendering parameters
        public float font_pt;
        public System.Drawing.Text.TextRenderingHint render_hint;
        public System.Drawing.Color clear_color;
        public Brush clear_brush;
        public Brush inverted_clear_brush;
        public Brush default_txt_brush;
        public Brush inverted_txt_brush;
        public Pen default_pen;

        // Device LCD connection info
        protected int connection = DMcLgLCD.LGLCD_INVALID_CONNECTION;
        public int device = DMcLgLCD.LGLCD_INVALID_DEVICE;
        protected int device_type = DMcLgLCD.LGLCD_INVALID_DEVICE;

        protected uint last_buttons = 0;
 
        public String[] font_names = new String[]  {"Inconsolata Medium", "Arial", "Arial Narrow", "Consolas", "Terminal", "Segoe UI Light", "Segoe UI"};
        public int cur_font = 1;
        protected int num_fonts = 7;

        public delegate void ButtonHandler(Device dev);
       
        public event ButtonHandler ButtonUP;
        public event ButtonHandler ButtonDOWN;
        public event ButtonHandler ButtonLEFT;
        public event ButtonHandler ButtonRIGHT;
        public event ButtonHandler ButtonOK;
        public event ButtonHandler ButtonCANCEL;
        public event ButtonHandler ButtonMENU;

        public Bitmap LCD;  // Main rendering surface
        public System.Drawing.Graphics graph;
        public System.Drawing.Font sys_font;

        public List<DisplayModule> modules = new List<DisplayModule>();

        private bool disposed = false;

        public Device()
        {
            valid = false;
            width = 0;
            height = 0;
            is_color = false;
            font_pt = 0.0F;
            render_hint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            clear_color = System.Drawing.Color.White;
            clear_brush = Brushes.White;
            inverted_clear_brush = Brushes.Black;
            default_txt_brush = Brushes.Black;
            inverted_txt_brush = Brushes.White;
            default_pen = Pens.Black;
            use_backdrops = false;
        }

        public bool isValid()
        {
            return !(device == DMcLgLCD.LGLCD_INVALID_DEVICE);
        }

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            if (sys_font != null)
                sys_font.Dispose();

            LCD.Dispose();

            if (device != DMcLgLCD.LGLCD_INVALID_DEVICE)
            {
                DMcLgLCD.LcdClose(device);
                device = DMcLgLCD.LGLCD_INVALID_DEVICE;
            }

            if (connection != DMcLgLCD.LGLCD_INVALID_CONNECTION)
            {
                DMcLgLCD.LcdDisconnect(connection);
                connection = DMcLgLCD.LGLCD_INVALID_CONNECTION;
            }
            GC.SuppressFinalize(this);
        }

        public void RenderSysString(String msg, bool invert, Rectangle rect)
        {
            if(sys_font == null)
                sys_font = new System.Drawing.Font(font_names[cur_font], font_pt, System.Drawing.FontStyle.Bold);
            SizeF str_bounds = graph.MeasureString(msg, sys_font, new Point(0, 0), System.Drawing.StringFormat.GenericDefault);
            Brush br = invert ? inverted_txt_brush : default_txt_brush;

            int x = (int)((width / 2) - (str_bounds.Width / 2));
            int y = (int)((height / 2) - (str_bounds.Height / 2));

            if (invert)
            {
                if (rect.Width == 0 || rect.Height == 0)
                    rect = new Rectangle(x, y, (int)(str_bounds.Width + 4), (int)(str_bounds.Height + 4));
                graph.FillRectangle(inverted_clear_brush, rect);
            }

            graph.DrawString(msg, sys_font, invert ? inverted_txt_brush : default_txt_brush, x, y);
        }

        // Poorly named.  This simply clears the bitmap that will eventually be written to the LCD
        public void ClearLCD(String msg)
        {
            if (graph == null)
                return;
            
            graph.Clear(clear_color);
            
            if (msg != null && msg != "")
            {
                RenderSysString(msg, false, new Rectangle(0,0,0,0));
            }
        }

        public void DisplayFrame()
        {
            if (!valid)
                return;
            DMcLgLCD.LcdUpdateBitmap(device, LCD.GetHbitmap(), device_type);
        }

        public Rectangle GetRect()
        {
            return new Rectangle(0, 0, width, height);
        }

        protected void InitLCD()
        {
            connection = DMcLgLCD.LcdConnectEx("GHud", 0, 0);
            if (DMcLgLCD.LGLCD_INVALID_CONNECTION != connection)
            {
                device = DMcLgLCD.LcdOpenByType(connection, device_type);
                if(DMcLgLCD.LGLCD_INVALID_DEVICE == device)
                    return;

                LCD = new Bitmap(width, height);
                
                graph = System.Drawing.Graphics.FromImage(LCD);
                
                graph.TextRenderingHint = render_hint;

                ClearLCD("Initializing...");
                DMcLgLCD.LcdSetAsLCDForegroundApp(device, DMcLgLCD.LGLCD_FORE_YES);
                valid = true;
           }
        }

        public void DoButtons()
        {
            uint buttons = DMcLgLCD.LcdReadSoftButtons(device);
            
            if (buttons != last_buttons)
            {
                if ((buttons & (DMcLgLCD.LGLCD_BUTTON_1 | DMcLgLCD.LGLCD_BUTTON_LEFT)) != 0)
                {
                    ButtonLEFT(this);
                }
                if ((buttons & (DMcLgLCD.LGLCD_BUTTON_2 | DMcLgLCD.LGLCD_BUTTON_RIGHT)) != 0)
                {
                    ButtonRIGHT(this);
                }
                if ((buttons & (DMcLgLCD.LGLCD_BUTTON_3 | DMcLgLCD.LGLCD_BUTTON_OK)) != 0)
                {
                    ButtonOK(this);
                }
                if ((buttons & (DMcLgLCD.LGLCD_BUTTON_4 | DMcLgLCD.LGLCD_BUTTON_MENU)) != 0)
                {
                    ButtonMENU(this);
                }
                if ((buttons & DMcLgLCD.LGLCD_BUTTON_UP) != 0)
                {
                    ButtonUP(this);
                }
                if ((buttons & DMcLgLCD.LGLCD_BUTTON_DOWN) != 0)
                {
                    ButtonDOWN(this);
                }
                if ((buttons & DMcLgLCD.LGLCD_BUTTON_CANCEL) != 0)
                {
                    ButtonCANCEL(this);
                }
                         
                last_buttons = buttons;
            }
        }
    }

    class DeviceBW : Device
    {
        public DeviceBW()
        {
            valid = false;
            width = 160;
            height = 43;
            is_color = false;
            font_pt = 7.0F;
            render_hint = System.Drawing.Text.TextRenderingHint.SingleBitPerPixelGridFit;
            clear_color = System.Drawing.Color.White;
            default_txt_brush = Brushes.Black;
            clear_brush = Brushes.White;
            inverted_clear_brush = Brushes.Black;
            inverted_txt_brush = Brushes.White;
            default_pen = Pens.Black;
            use_backdrops = false;

            device_type = DMcLgLCD.LGLCD_DEVICE_BW;
            InitLCD();
        }
    }

    class DeviceQVGA : Device
    {
        public DeviceQVGA()
        {
            valid = false;
            width = 320;
            height = 240;
            is_color = true;
            font_pt = 14.0F;
            render_hint = System.Drawing.Text.TextRenderingHint.AntiAliasGridFit;
            clear_color = System.Drawing.Color.Black;
            default_txt_brush = Brushes.White;
            clear_brush = Brushes.Black;
            inverted_clear_brush = Brushes.White;
            inverted_txt_brush = Brushes.Black;
            default_pen = Pens.White;
            use_backdrops = false;

            device_type = DMcLgLCD.LGLCD_DEVICE_QVGA;
            InitLCD();
        }
    }
}

