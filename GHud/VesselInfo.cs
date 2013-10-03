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
        List<DisplayModule> top_modules = new List<DisplayModule>();
        List<DisplayModule> bottom_modules = new List<DisplayModule>();
        DisplayModule active_top_mod;
        DisplayModule active_bottom_mod;
        
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
            target_orbitinfo = new OrbitInfo(dev, "+", System.Drawing.Color.LightBlue, System.Drawing.Color.MediumPurple);

            orbitgraph = new OrbitGraph(dev, System.Drawing.Color.Yellow, "✈");
            target_orbitgraph = new OrbitGraph(dev, System.Drawing.Color.LightBlue, "+");

            orbitgraph.companion_mod = orbitinfo;
            orbitinfo.companion_mod = orbitgraph;

            target_orbitinfo.is_target_type_module = true;
            target_orbitgraph.is_target_type_module = true;
            target_orbitinfo.companion_mod = target_orbitgraph;
            target_orbitgraph.companion_mod = target_orbitinfo;

            orbitinfo.Activate();
            active_top_mod = orbitinfo;
            orbitgraph.Activate();
            active_bottom_mod = orbitgraph;

            orbitinfo.modid = 1;
            target_orbitinfo.modid = 2;
            orbitgraph.modid = 3;
            target_orbitgraph.modid = 4;

            top_modules.Add(orbitinfo);
            top_modules.Add(target_orbitinfo);
            bottom_modules.Add(orbitgraph);
            bottom_modules.Add(target_orbitgraph);


            orbitinfo = new OrbitInfo(dev, "✈", System.Drawing.Color.FromArgb(0xee, 0xee, 0x00), System.Drawing.Color.FromArgb(0xaa, 0xaa, 0x44));
            target_orbitinfo = new OrbitInfo(dev, "+", System.Drawing.Color.LightBlue, System.Drawing.Color.MediumPurple);

            orbitgraph = new OrbitGraph(dev, System.Drawing.Color.Yellow, "✈");
            target_orbitgraph = new OrbitGraph(dev, System.Drawing.Color.LightBlue, "+");

            orbitgraph.companion_mod = orbitinfo;
            orbitinfo.companion_mod = orbitgraph;

            target_orbitinfo.is_target_type_module = true;
            target_orbitgraph.is_target_type_module = true;
            target_orbitinfo.companion_mod = target_orbitgraph;
            target_orbitgraph.companion_mod = target_orbitinfo;

            orbitinfo.modid = 1;
            target_orbitinfo.modid = 2;
            orbitgraph.modid = 3;
            target_orbitgraph.modid = 4;

            bottom_modules.Add(orbitinfo);
            bottom_modules.Add(target_orbitinfo);
            top_modules.Add(orbitgraph);
            top_modules.Add(target_orbitgraph);
            
            if(dev.use_backdrops)
                background = Image.FromFile("stars.gif");

            colmatrix = new System.Drawing.Imaging.ColorMatrix();
            colmatrix.Matrix33 = 0.7F;
            img_attr = new System.Drawing.Imaging.ImageAttributes();
            img_attr.SetColorMatrix(colmatrix, System.Drawing.Imaging.ColorMatrixFlag.Default, System.Drawing.Imaging.ColorAdjustType.Bitmap);
            
            dev.ButtonUP += new Device.ButtonHandler(ButtonUp);
            dev.ButtonDOWN += new Device.ButtonHandler(ButtonDown);
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

        private bool ActivateModule(DisplayModule new_active_mod, ref DisplayModule change_active_mod, ref DisplayModule other_active_mod)
        {
            /*
            if (((other_active_mod == change_active_mod.companion_mod && new_active_mod.companion_mod.GetType() == other_active_mod.GetType()) ||
                new_active_mod.companion_mod.GetType() == other_active_mod.GetType()) && 
                new_active_mod.companion_mod != null
                )
            {
                other_active_mod.Deactivate();
                other_active_mod = new_active_mod.companion_mod;  
            }
             */
            change_active_mod.Deactivate();
            change_active_mod = new_active_mod;
            change_active_mod.Activate();
            other_active_mod.Activate();
            return true;
        }

        private void CycleModules(ref List<DisplayModule> changelist, ref List<DisplayModule> otherlist,
                                    ref DisplayModule change_active_mod, ref DisplayModule other_active_mod, bool activate_first)
        {
            bool activate_next = false;
            bool activated = false;

            if (activate_first)
            {
                ActivateModule(changelist[0], ref change_active_mod, ref other_active_mod);
                return;
            }

            foreach (DisplayModule mod in changelist)
            {
                if (activate_next)
                {
                    if (mod.modid == other_active_mod.modid)
                        continue;
                    ActivateModule(mod, ref change_active_mod, ref other_active_mod);
                     
                    activate_next = false;
                    activated = true;
                }
                else
                {
                    if (mod.active)
                    {
                        activate_next = true;
                    }
                }
            
            }
            if (!activated)
            {
                // Activate the first module since we were at the end of the list
                CycleModules(ref changelist, ref otherlist, ref change_active_mod, ref other_active_mod, true);
            }

        }

        /*
        public override void SetOrbit(Orbit argorbit, String obj_name = "Unknown")
        {
            base.SetOrbit(argorbit, obj_name);

            List<DisplayModule> list = top_modules + bottom_modules;

            foreach (DisplayModule mod in list)
            {
                ITargetable tgt;
                if (mod.is_target_type_module)
                {
                    tgt = FlightGlobals.fetch.VesselTarget;
                    mod.SetOrbit(tgt == null ? null : tgt.GetOrbit(), tgt == null ? null : tgt.GetName());
                }
                else
                {
                    mod.SetOrbit(argorbit, obj_name);
                }
            }

            
        }
        */

        public void ButtonUp(Device dev)
        {
            if (!active)
                return;

            CycleModules(ref top_modules, ref bottom_modules, ref active_top_mod, ref active_bottom_mod, false);  
        }

        public void ButtonDown(Device dev)
        {
            if (!active)
                return;
            if (dev.is_color)
            {
                CycleModules(ref bottom_modules, ref top_modules, ref active_bottom_mod, ref active_top_mod, false);
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

            Rectangle vrect;
            if (active_top_mod.GetType() == active_bottom_mod.GetType())
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height / 2));
            }
            else if (active_top_mod is OrbitInfo)
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            }
            else
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.6f));
            }
            //Rectangle vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            Rectangle trect = new Rectangle(rect.X, (int)(rect.Y + vrect.Height), rect.Width, (int)(rect.Height - vrect.Height));
            Rectangle frect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            //Rectangle vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            //Rectangle trect = new Rectangle(rect.X, vrect.Height, rect.Width, (int)(rect.Height - vrect.Height));
            //Rectangle frect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);

            TestRenderModules(top_modules, vrect);
            TestRenderModules(bottom_modules, trect);

            //orbitinfo.TestRender(vrect);
            //target_orbitinfo.TestRender(vrect);
            //orbitgraph.TestRender(trect);
            //target_orbitgraph.TestRender(trect);
        }

        private void TestRenderModules(List<DisplayModule> list, Rectangle rect)
        {
            
            foreach (DisplayModule mod in list)
            {
                if (mod.active)
                {
                    if (mod.is_target_type_module)
                    {
                        mod.TestRender(rect);
                    }
                    else
                    {
                        mod.TestRender(rect);
                    }
                }
            }
        }

        private void RenderModules(List<DisplayModule> list, Rectangle rect)
        {
            Vessel vessel = FlightGlobals.ActiveVessel;

            if (vessel == null)
            {
                dev.ClearLCD("Waiting for flight...");
                return;
            }
            ITargetable target;
            target = FlightGlobals.fetch.VesselTarget;

            foreach (DisplayModule mod in list)
            {
                if (mod.active)
                {
                    if (!mod.is_target_type_module)
                    {
                        mod.SetOrbit(vessel.orbit, vessel.GetName());
                        mod.Render(rect);
                    }
                    else
                    {
                        if (target != null)
                        {
                            mod.SetOrbit(target.GetOrbit(), target.GetName());
                        }
                        else
                        {
                            mod.SetOrbit(null, null);
                        }
                        mod.Render(rect);
                    }
                }
            }

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

            Rectangle vrect;
            if (active_top_mod.GetType() == active_bottom_mod.GetType())
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height / 2));
            }
            else if (active_top_mod is OrbitInfo)
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            }
            else
            {
                vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.6f));
            }
            //Rectangle vrect = new Rectangle(rect.X, rect.Y, rect.Width, (int)(rect.Height * 0.4f));
            Rectangle trect = new Rectangle(rect.X, (int)(rect.Y + vrect.Height), rect.Width, (int)(rect.Height - vrect.Height));
            Rectangle frect = new Rectangle(rect.X, rect.Y, rect.Width, rect.Height);



            RenderModules(top_modules, vrect);
            RenderModules(bottom_modules, trect);
            
        }

    }
}
