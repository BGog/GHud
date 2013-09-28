using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.IO;

using GHud;

namespace testGHud
{
    class Program
    {
     
        static void Main(string[] args)
        {
            Console.WriteLine(System.IO.Directory.GetCurrentDirectory());
            GHud.GHud hud = new GHud.GHud();
            hud.TestMode();
            hud.Awake();
            
            for(int i = 0; i < 10000; i++)
            {
                hud.Update();
                //System.Threading.Thread.Sleep(50);
            }

            hud.OnDestroy();

            
            /*



            GHud.GHud g = new GHud.GHud();
            GHud.GHud g2 = new GHud.GHud();
            g.TestMode();
            g2.TestMode();
            //g.Awake();
            //g2.Awake();
            g.init_lcd(2);
            g2.init_lcd(1);
            while (true)
            {
                g.do_buttons();
                g2.do_buttons();

                g.Update();
                g2.Update();
                System.Threading.Thread.Sleep(50);
            }
             */
        }
    }
}
