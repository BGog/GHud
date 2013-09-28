using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;

namespace GHud
{
    class OrbitInfo : DisplayModule
    {
        protected String orb_velocity_str;
        protected String orb_ap_str;
        protected String orb_pe_str;
        protected String orb_apt_str;
        protected String orb_pet_str;
        protected String orb_ap_suffix;
        protected String orb_pe_suffix;
        protected String orb_inc_str;
        protected String orb_body_name;
       
        protected String monicer = "✈";
        protected int count = 0;
        
        protected String situationstr = "";

        public OrbitInfo(Device dev, String argmon, 
                         System.Drawing.Color brect_c1, 
                         System.Drawing.Color brect_c2) : base(dev)
        {
            name = "Orbit";
            selectable = false;
            active = false;
            monicer = argmon;

            width = dev.width;
            height = dev.height;

            if(brect_c1 != null)
                back_rect_c1 = brect_c1;
            if (brect_c2 != null)
                back_rect_c2 = brect_c2;

            ResetBackRectBrush();
        }
        
        // Setup some test data for when running in non-ksp test mode
        protected void PrepTestData(String targname = "")
        {

            orb_velocity_str = "174.55m/s";

            orb_ap_str = "70.48m";

            orb_pe_str = "-598.44km";

            orb_inc_str = "0.103°";

            orb_apt_str = "4d 00:00:00";
            orb_pet_str = "00:00:00";

            orb_body_name = "Kerbin";
            if (targname == "")
                targname = "Jool Triprobe";
            orb_obj_name = targname;

            situationstr = "Sub Orbital";
        }


        protected void PrepData(String targname = "")
        {
            Vessel vessel = FlightGlobals.ActiveVessel;

            switch (vessel.situation)
            {
                case Vessel.Situations.DOCKED:
                    situationstr = "Docked";
                    break;
                case Vessel.Situations.ESCAPING:
                    situationstr = "Escaping";
                    break;
                case Vessel.Situations.FLYING:
                    situationstr = "Flying";
                    break;
                case Vessel.Situations.LANDED:
                    situationstr = "Landed";
                    break;
                case Vessel.Situations.ORBITING:
                    situationstr = "Orbiting";
                    break;
                case Vessel.Situations.PRELAUNCH:
                    situationstr = "Prelaunch";
                    break;
                case Vessel.Situations.SPLASHED:
                    situationstr = "Splashed";
                    break;
                case Vessel.Situations.SUB_ORBITAL:
                    situationstr = "Sub Orbital";
                    break;
                default:
                    situationstr = "Unknown";
                break;  
            }
            

            orb_velocity_str = orbit.vel.magnitude.ToString("F2");

            orb_ap_str = Util.xMuMech_ToSI(orbit.ApA, ref orb_ap_suffix);
            orb_ap_str += orb_ap_suffix + "m";

            orb_pe_str = Util.xMuMech_ToSI(orbit.PeA, ref orb_pe_suffix);
            orb_pe_str += orb_pe_suffix + "m";

            orb_inc_str = orbit.inclination.ToString("F3") + "°";

            orb_apt_str = Util.ConvertInterval(orbit.timeToAp, false);
            orb_pet_str = Util.ConvertInterval(orbit.timeToPe, false);

            orb_body_name = orbit.referenceBody.GetName();
            orb_obj_name = vessel.name;
        }

        protected void DoRender(Rectangle rect)
        {
            int curline = 0;
            float tmp_font_pt = font_pt;

            if (rect.Width != 0 && rect.Height != 0)
            {
                width = rect.Width;
                height = rect.Height;
                xoff = rect.X;
                yoff = rect.Y;
            }
            else
            {
                width = dev.width;
                height = dev.height;
                xoff = 0;
                yoff = 0;
            }
            font_pt = Math.Min(Math.Max((float)((height / 4) * 0.7), 7), 14);
            curline = 0;
            RenderString(monicer, curline, 0, ref two_column_labeled_offsets, fmt_center, System.Drawing.FontStyle.Bold, true);
            RenderString(orb_obj_name, curline, 1, ref two_column_labeled_offsets, fmt_left, System.Drawing.FontStyle.Regular, true);
            RenderString("⊙ " + orb_body_name, curline, 3, ref two_column_labeled_offsets, fmt_center, System.Drawing.FontStyle.Regular, true);
            //∆v
         
            curline = 1;
            RenderString("⇢", curline, 0, ref two_column_labeled_offsets, fmt_right, System.Drawing.FontStyle.Regular, false, tmp_font_pt + 3.0f, 0.0f, -3.0f);
            RenderString(orb_velocity_str, curline, 1, ref two_column_labeled_offsets, fmt_center);
            
            RenderString("θ", curline, 2, ref two_column_labeled_offsets, fmt_right);
            RenderString(orb_inc_str, curline, 3, ref two_column_labeled_offsets, fmt_center);
            
            curline = 2;
            RenderString("a", curline, 0, ref two_column_offsets, fmt_left);
            RenderString("p", curline, 1, ref two_column_offsets, fmt_left);

            RenderString(orb_ap_str, curline, 0, ref two_column_offsets, fmt_right);
            RenderString(orb_pe_str, curline, 1, ref two_column_offsets, fmt_right);
            dev.graph.DrawLine(dev.is_color ? Pens.Green : Pens.Black, 0 + xoff, curline * line_offset + yoff, width + xoff, curline * line_offset + yoff);
            dev.graph.DrawLine(dev.is_color ? Pens.Green : Pens.Black, (width / 2) + xoff, (curline * line_offset) + yoff, (width / 2) + xoff, height + yoff);

            curline = 3;
            RenderString(orb_apt_str, curline, 0, ref two_column_offsets, fmt_right);
            RenderString(orb_pet_str, curline, 1, ref two_column_offsets, fmt_right);
        }

        public override void TestRender(Rectangle rect)
        {
            if (!active)
                return;
            PrepTestData();

            DoRender(rect);
        }



        public override void Render(Rectangle rect)
        {
            if (!active)
                return;
            PrepData();
            DoRender(rect);
        }
    }
}
