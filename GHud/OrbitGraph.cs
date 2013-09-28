using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;


namespace GHud
{

    // Display module which displays a rendering of the current orbit and its parent body.
    class OrbitGraph: DisplayModule
    {
        protected Pen orbitpen;
        
        protected double ApR;
        protected double PeR;
        protected double ApA;
        protected double PeA;
        
        protected double semi_major_axis;
        protected double semi_minor_axis;
        protected double eccentricity;
        protected double atmos_dia;
        protected double dia;
        protected Rectangle elliptical_rect;
        protected System.Drawing.Color body_color;

        protected double a;
        protected double r;
        protected Dictionary<string, System.Drawing.Color> body_colors;

        public OrbitGraph(Device dev, String argmon = "") : base(dev)
        {
            name = "Orbit Graph";
            selectable = false;
            active = false;
        
            width = dev.width;
            height = dev.height;

            a = 0;

            // Define the rendering color of the various orbital bodies.
            body_colors = new Dictionary<string, System.Drawing.Color>();
            body_colors.Add("Sun", System.Drawing.Color.Yellow);
            body_colors.Add("Kerbol", System.Drawing.Color.Yellow);
            body_colors.Add("Moho", System.Drawing.Color.Peru);
            body_colors.Add("Eve", System.Drawing.Color.BlueViolet);
            body_colors.Add("Kerbin", System.Drawing.Color.DodgerBlue);
            body_colors.Add("Duna", System.Drawing.Color.Firebrick);
            body_colors.Add("Dres", System.Drawing.Color.Gray);
            body_colors.Add("Jool", System.Drawing.Color.LimeGreen);
            body_colors.Add("Eeloo", System.Drawing.Color.LightGray);
            body_colors.Add("Gilly", System.Drawing.Color.DarkGray);
            body_colors.Add("Mun", System.Drawing.Color.DarkGray);
            body_colors.Add("Minmus", System.Drawing.Color.Gray);
            body_colors.Add("Ike", System.Drawing.Color.DarkGray);
            body_colors.Add("Laythe", System.Drawing.Color.DodgerBlue);
            body_colors.Add("Vall", System.Drawing.Color.Gray);
            body_colors.Add("Tylo", System.Drawing.Color.LightGray);
            body_colors.Add("Bop", System.Drawing.Color.Gray);
            body_colors.Add("Pol", System.Drawing.Color.SandyBrown);
            body_colors.Add("Default", System.Drawing.Color.DodgerBlue);
            
            body_colors.TryGetValue("Pol", out body_color);
            
            orbitpen = new Pen(System.Drawing.Color.Yellow, 1.0f);
            orbitpen.Alignment = System.Drawing.Drawing2D.PenAlignment.Outset;
        }

        // setup some dummy values when in non-ksp test mode.
        protected void PrepTestData(Rectangle rect)
        {
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
            r = 100000.0;
            a += 0.6;
            if (a > 360.0)
                a -= 360.0;

            dia = 80000.00;
            atmos_dia = 120000.00;
            ApR = 100000.00;
            PeR = 100000.00;
           
            ApA = ApR + (dia / 2);
            PeA = PeR + (dia / 2);
            semi_major_axis = (ApR + PeR) / 2;
            eccentricity = (ApR - PeR) / (ApR + PeR);
            semi_minor_axis = semi_major_axis * (1 - eccentricity);
        }


        // Gather and prepare data needed for rendering this frame.
        protected void PrepData(Orbit orbit, Rectangle rect)
        {
            // Get the color of the body being orbited
            if(!body_colors.TryGetValue(orbit.referenceBody.GetName(), out body_color)){
                body_colors.TryGetValue("Default", out body_color);
            }
            
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

            // Orbit info
            ApR = orbit.ApR;
            PeR = orbit.PeR;
            ApA = orbit.ApA;
            PeA = orbit.PeA;

            // Calc the diameter of the reference body and the diameter of its atmosphere
            dia = orbit.referenceBody.Radius * 2;
            atmos_dia = dia + (orbit.referenceBody.maxAtmosphereAltitude * 2);
                        
            semi_major_axis = orbit.semiMajorAxis;
            semi_minor_axis = orbit.semiMinorAxis;
            eccentricity = orbit.eccentricity;
            
            r = orbit.RadiusAtTrueAnomaly(orbit.trueAnomaly);
            a = orbit.trueAnomaly;//(360.0 - orbit.trueAnomaly) + 90.0;
        }

        protected void DoRender(Rectangle rect)
        {
            // Render in the specified rectangle.  If the rect is 0, then use the whole device geometry
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
                        

            int border = 3;
            // The bounding box for the drawing
            Rectangle dr = new Rectangle(xoff + border, yoff + border, (int)(width - (border * 2)), (int)(height - (border * 2)));
            
            // Calculate scaling factor used to scale from KSP orbital sizes down to device pixels
            double width_mod = dr.Width / (semi_major_axis * 2);
            double height_mod = dr.Height / (semi_minor_axis * 2);

            double atmos_mod = Math.Min(dr.Width, dr.Height) / atmos_dia;

            // Use the smallest of the scaling factors
            double mod = Math.Min(atmos_mod, Math.Min(width_mod, height_mod));

            // Ellipse geometry scaled to display area
            double el_width = (semi_major_axis * 2) * mod;
            double el_height = (semi_minor_axis * 2) * mod;
            int elx = (int)(dr.X + ((dr.Width - el_width) / 2));
            int ely = (int)(dr.Y + ((dr.Height - el_height) / 2));

            // Draw the ellipse
            elliptical_rect = new Rectangle(elx, ely, (int)(el_width), (int)(el_height));
            if (elliptical_rect.Height < 0)
                elliptical_rect.Height = 1;
            dev.graph.DrawEllipse(orbitpen, elliptical_rect);
            
            // Scale and draw the body
            double body_dia = dia * mod;
            // Determine the scaled location where the body should be centered
            double centerx = (elx + (el_width / 2)) - (body_dia / 2);
            double centery = (ely + (el_height / 2)) - (body_dia / 2);
            double body_xoff = (semi_major_axis - PeR) * mod;
            double body_x = centerx + body_xoff;

            // Draw the body
            Rectangle body_rect = new Rectangle((int)body_x, (int)centery, (int)body_dia, (int)body_dia);
            if (dev.is_color)
            { 
                System.Drawing.Brush body_brush = new System.Drawing.SolidBrush(body_color);
                dev.graph.FillEllipse(body_brush, body_rect);
                body_brush.Dispose();
            }
            else
            {
                dev.graph.FillEllipse(dev.inverted_clear_brush, body_rect);
            }
            
            // Calculate the drawing location of the vessel on the ellipse.
            double aradian = (a + 90) * (Math.PI/180);
            double x = (r * Math.Sin(aradian)) * mod;
            double y = (r * Math.Cos(aradian)) * mod;
            int xx = (int)(x + body_x + (body_dia / 2));
            int yy = (int)(y + centery  + (body_dia / 2));

            int vi_rad = (int)(dev.font_pt / 3);
            Rectangle vessel_rect = new Rectangle(xx - vi_rad, yy - vi_rad, vi_rad * 2, vi_rad * 2);
            dev.graph.FillEllipse(dev.inverted_clear_brush, vessel_rect);
            

            // Calculate and draw the atmosphere.  This is drawn over the body with and alpha level.
            // By drawing this last, we get the effect of seeing the orbit and vessel behind the atmostphere.
            double atmosdia = atmos_dia * mod;
            double atmos_centerx = (elx + (el_width / 2)) - (atmosdia / 2);
            double atmos_centery = (ely + (el_height / 2)) - (atmosdia / 2);
            double atmos_x = atmos_centerx + body_xoff;
            Rectangle atmos_rect = new Rectangle((int)atmos_x, (int)atmos_centery, (int)atmosdia, (int)atmosdia);
            UnityEngine.Debug.LogWarning("[XXXXXXX] Rend 8");    

            if (dev.is_color)
            {
                double reduction = (atmosdia - body_dia) / 2;
                double pos_reduction = reduction / 2;
                Rectangle atmos_inner_rect = new Rectangle((int)(atmos_x + pos_reduction), (int)(atmos_centery + pos_reduction), (int)(atmosdia - (reduction)), (int)(atmosdia - (reduction)));
                System.Drawing.Brush atmos_brush = new System.Drawing.SolidBrush(System.Drawing.Color.FromArgb(100, (int)body_color.R, (int)body_color.G, (int)body_color.B));
                
                dev.graph.FillEllipse(atmos_brush, atmos_inner_rect);
                dev.graph.FillEllipse(atmos_brush, atmos_rect);
                atmos_brush.Dispose();
            }
            else
            {
                Pen pen = (Pen)dev.default_pen.Clone();
                pen.DashPattern = new float[] { 1.0F, 4.0F };
                dev.graph.DrawEllipse(pen, atmos_rect);
                pen.Dispose();
            }
            
            // We are leaving the sphere of influence,  no orbit can be drawn.
            if (ApR < 0 && ApR < PeR)
            {
                ModuleMsg("Leaving Sphere of Influence", rect);
                return;
            }
        }

        public override void TestRender(Rectangle rect)
        {
            if (!active)
                return;

            PrepTestData(rect);
            DoRender(rect);
        }

       
        public override void Render(Rectangle rect)
        {
            if (!active)
                return;

            PrepData(orbit, rect);
            DoRender(rect);
        }
    }
}
