using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using UnityEngine;

namespace GHud
{

    // This is a hybrid displaymodule.  It contains both and orbitinfo and orbit graph displaymodule and splits them on the screen.
    class VesselInfo : DisplayModule
    {
        protected OrbitInfo orbitinfo;
        protected OrbitInfo target_orbitinfo;
        protected OrbitGraph orbitgraph;
        protected OrbitGraph target_orbitgraph;

        protected Image background;
        protected System.Drawing.Imaging.ColorMatrix colmatrix;
        protected System.Drawing.Imaging.ImageAttributes img_attr;

        public VesselInfo(Device dev) : base(dev)
        {
            name = "Vessel";
            selectable = true;
            active = false;

            width = dev.width;
            height = dev.height;
            xoff = 0;
            yoff = 0;


            orbitinfo = new OrbitInfo(dev, "✈", System.Drawing.Color.FromArgb(0xee, 0xee, 0x00), System.Drawing.Color.FromArgb(0xaa, 0xaa, 0x44));
            target_orbitinfo = new OrbitInfo(dev, "⊹", System.Drawing.Color.LightBlue, System.Drawing.Color.MediumPurple);
            orbitgraph = new OrbitGraph(dev);
            target_orbitgraph = new OrbitGraph(dev);

            target_orbitinfo.is_target_type_module = true;
            target_orbitgraph.is_target_type_module = true;

            orbitinfo.Activate();
            orbitgraph.Activate();

            
            if(dev.use_backdrops)
                background = Image.FromFile("stars.gif");

            colmatrix = new System.Drawing.Imaging.ColorMatrix();
            colmatrix.Matrix33 = 0.7F;
            img_attr = new System.Drawing.Imaging.ImageAttributes();
            img_attr.SetColorMatrix(colmatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
            
            dev.ButtonLEFT += new Device.ButtonHandler(ButtonLeft);
            dev.ButtonRIGHT += new Device.ButtonHandler(ButtonRight);
        }


        public override void Clear()
        {
            base.Clear();

            if (dev.use_backdrops)
            {
                dev.graph.DrawImage(background, new Rectangle(0, 0, width, height), 0, 0, background.Width,
                                    background.Height, GraphicsUnit.Pixel, img_attr);
            }
        }


        public void ButtonLeft(Device dev)
        {
            // Disabled for now.  Will add back when target view is working.
            return;
            
            orbitinfo.Activate();
            target_orbitinfo.Deactivate();
            if (dev.is_color)
            {
                orbitgraph.Activate();
                target_orbitgraph.Deactivate();
            }
        }

        public void ButtonRight(Device dev)
        {
            // Disabled for now.  Will add back when target view is working.
            return;

            orbitinfo.Deactivate();
            target_orbitinfo.Activate();
            if (dev.is_color)
            {
                orbitgraph.Deactivate();
                target_orbitgraph.Activate();
            }
        }
       
        public override void TestRender(Rectangle rect)
        {
            if (!active)
                return;

            if (rect.Width == 0 || rect.Height == 0)
            {
                rect = dev.GetRect();
            }

            Clear();


            Rectangle vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            Rectangle trect = new Rectangle(rect.X, vrect.Height, rect.Width, (int)(rect.Height - vrect.Height));
            Rectangle frect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            orbitinfo.TestRender(vrect);
            target_orbitinfo.TestRender(vrect);
            orbitgraph.TestRender(trect);
            target_orbitgraph.TestRender(trect);
        }

        public override void Render(Rectangle rect)
        {
            if (!active)
                return;


            if (rect.Width == 0 || rect.Height == 0)
            {
                rect = dev.GetRect();
            }

            Clear();

            Rectangle vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            Rectangle trect = new Rectangle(rect.X, vrect.Height, rect.Width, (int)(rect.Height - vrect.Height));
            Rectangle frect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            Vessel vessel = FlightGlobals.ActiveVessel;

            if (vessel == null)
            {
                dev.ClearLCD("Waiting for flight...");
                return;
            }

            ITargetable target;
            target = FlightGlobals.fetch.VesselTarget;

            orbitinfo.SetOrbit(vessel.orbit);
            orbitinfo.Render(vrect);

            if (target != null)
            {
                target_orbitinfo.SetOrbit(target.GetOrbit());
                target_orbitinfo.Render(vrect);
            }
            else
            {
                target_orbitinfo.ModuleMsg("No Target", vrect);
            }
            orbitgraph.SetOrbit(vessel.orbit);
            orbitgraph.Render(trect);

            if (target != null)
            {
                target_orbitgraph.SetOrbit(target.GetOrbit());
                target_orbitgraph.Render(trect);
            }
            else
            {
                target_orbitinfo.ModuleMsg("No Target", trect);
            }

        }

    }
}
