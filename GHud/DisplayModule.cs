using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GHud
{
    public class DisplayModule: IDisposable
    {
        public string name;
        public int width = 1;
        public int height = 1;
        public int xoff = 0;
        public int yoff = 0;
        public float font_pt;
        public bool is_target_type_module = false;

        protected bool selectable;
        public bool active = false;
        protected System.Drawing.Font curfont;

        protected float line_offset;
        protected int max_suffix_width = 0;
        protected int max_lab_width = 0;
        protected const String mps2_suf = "m²";//"km²";
        protected const String big_suf = "mT";
        protected const String big_lab = "WW:";

        protected StringFormat fmt_center = new StringFormat();
        protected StringFormat fmt_right = new StringFormat();
        protected StringFormat fmt_left = new StringFormat();
        protected StringFormat fmt_default = System.Drawing.StringFormat.GenericDefault;

        protected int[] two_column_labeled_offsets;
        protected int[] two_column_offsets;
        protected Device dev;

        protected System.Drawing.Color back_rect_c1 = System.Drawing.Color.Black;
        protected System.Drawing.Color back_rect_c2 = System.Drawing.Color.Black;

        protected System.Drawing.Drawing2D.LinearGradientBrush back_rect_brush;

        protected Orbit orbit;
        protected String orb_obj_name;

        
        public int modid;
        public DisplayModule companion_mod;

        private bool disposed = false;

        public void Dispose()
        {
            if (disposed)
                return;
            disposed = true;

            if (fmt_center != null)
            {
                fmt_center.Dispose();
                fmt_center = null;
            }
            if (fmt_right != null)
            {
                fmt_right.Dispose();
                fmt_right = null;
            }
            if (fmt_left != null)
            {
                fmt_left.Dispose();
                fmt_left = null;
            }
            if (fmt_default != null)
            {
                fmt_default.Dispose();
                fmt_default = null;
            }
            if (back_rect_brush != null)
            {
                back_rect_brush.Dispose();
                back_rect_brush = null;
            }
        }

        public DisplayModule(Device argdev)
        {
            dev = argdev;
            name = "INVALID";
            selectable = false;
            active = false;
            curfont = null;

            modid = 0;

            font_pt = dev.font_pt;

            companion_mod = null;

            fmt_left.Alignment = StringAlignment.Near;
            fmt_left.LineAlignment = StringAlignment.Center;
            fmt_center.Alignment = StringAlignment.Center;
            fmt_center.LineAlignment = StringAlignment.Center;
            fmt_right.Alignment = StringAlignment.Far;
            fmt_right.LineAlignment = StringAlignment.Center;
        }

        public virtual void SetOrbit(Orbit argorbit, String obj_name = "Unknown")
        {
            orbit = argorbit;
            orb_obj_name = obj_name;
        }

        protected void ResetBackRectBrush()
        {
            if(back_rect_brush != null)
                back_rect_brush.Dispose();
            back_rect_brush = new System.Drawing.Drawing2D.LinearGradientBrush(new PointF(0.5F,0.0F), new PointF(0.5F, 1.0F), back_rect_c1, back_rect_c2);
        }

        public virtual void ModuleMsg(String msg, Rectangle rect, bool invert = false)
        {
            if (!active)
                return;
            
            if (msg != null && msg != "" && rect != null)
            {
                dev.RenderSysString(msg, invert, rect);
            }
        }


        public virtual void Clear()
        {
            if (!active)
                return;
            dev.graph.FillRectangle(dev.clear_brush, new Rectangle(xoff, yoff, width, height));
        }

        public virtual void Activate()
        {
            active = true;
        }

        public virtual void Deactivate()
        {
            active = false;
        }

        public virtual void TestRender(Rectangle rect)
        {

        }

        public virtual void Render(Rectangle rect)
        {
            
        }

        public virtual void DisplayModuleName()
        {
            dev.RenderSysString(name, true, new Rectangle(0,0,0,0));
        }

        // Calculates a set of column offset info based on measurements of the font and the display geometry
        protected void CalcColumns(System.Drawing.Font font)
        {
            if (!active || font == null)
                return;

            SizeF max_suf_bounds = dev.graph.MeasureString(big_suf, font, new Point(0, 0), fmt_default);
            max_suffix_width = (int)(max_suf_bounds.Width + 1);


            SizeF max_lab_bounds = dev.graph.MeasureString(big_lab, font, new Point(0, 0), fmt_default);
            max_lab_width = (int)(max_lab_bounds.Width);

            int small_col = max_lab_width;
            int large_col = (width - (small_col * 2)) / 2;

            line_offset = (float)Math.Round(font.GetHeight());

            if (two_column_labeled_offsets == null)
                two_column_labeled_offsets = new int[5];
            two_column_labeled_offsets[0] = xoff + 0;
            two_column_labeled_offsets[1] = xoff + small_col;
            two_column_labeled_offsets[2] = two_column_labeled_offsets[1] + large_col;
            two_column_labeled_offsets[3] = two_column_labeled_offsets[2] + small_col;
            two_column_labeled_offsets[4] = xoff + width;

            if (two_column_offsets == null)
                two_column_offsets = new int[3];
            two_column_offsets[0] = xoff + 0;
            two_column_offsets[1] = xoff + width / 2;
            two_column_offsets[2] = xoff + width;
        }

        public void FillRectGradient(Rectangle rect, float pct1, Color c1, Color c2, float pct2, Color c3, Color c4)
        {
            Rectangle rect1 = new Rectangle(rect.X, rect.Y, rect.Width, (int)Math.Ceiling((float)(rect.Height * pct1)));
            Rectangle rect2 = new Rectangle(rect.X, rect.Y + rect1.Height-1, rect.Width, (int)Math.Ceiling((float)(rect.Height *pct2))+1 );
            
            System.Drawing.Drawing2D.LinearGradientBrush tmpbrush = new System.Drawing.Drawing2D.LinearGradientBrush(rect2, c3, c4, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            dev.graph.FillRectangle(tmpbrush, rect2);
            tmpbrush.Dispose();

             tmpbrush = new System.Drawing.Drawing2D.LinearGradientBrush(rect1, c1, c2, System.Drawing.Drawing2D.LinearGradientMode.Vertical);
            dev.graph.FillRectangle(tmpbrush, rect1);
            tmpbrush.Dispose();
        }
        
        protected void RenderString(String str, int line, int column, ref int[] c_offsets, StringFormat fmt, System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular, bool back_rect = false, float cur_font_pt = 0.0f, float xoffset = 0.0f, float yoffset = 0.0f)
        {
            if (!active)
                return;

            System.Drawing.Font cfont = new System.Drawing.Font(dev.font_names[dev.cur_font], cur_font_pt == 0 ? font_pt : cur_font_pt, style);
            CalcColumns(cfont);
            
            xoffset += xoff;
            yoffset += yoff;
            Rectangle rect1 = new Rectangle((int)(c_offsets[column] + xoffset), 
                                                  (int)(Math.Floor(line_offset * line) + yoffset), 
                                                  (int)(c_offsets[column + 1] - c_offsets[column]), //+xoffset 
                                                  (int)(Math.Ceiling(line_offset)));// +yoffset
           
            if (back_rect)
            {
                FillRectGradient(rect1, 0.5f, back_rect_c1, back_rect_c2, 0.5f, back_rect_c2, back_rect_c1);             
                dev.graph.DrawString(str, cfont, dev.inverted_txt_brush, rect1, fmt);
            }
            else
            {
                dev.graph.DrawString(str, cfont, dev.default_txt_brush, rect1, fmt);
            }
            
            cfont.Dispose();
        }
    }
}
