using System;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

class DisplayTopologyTests
{
    static readonly BindingFlags Flags=BindingFlags.Instance|BindingFlags.Static|BindingFlags.Public|BindingFlags.NonPublic;
    static void Check(bool ok,string message) { if(!ok) throw new Exception(message); }
    [STAThread] static int Main()
    {
        try {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Assembly app=Assembly.LoadFrom("DesktopPet.exe");
            int staleIndex=Screen.AllScreens.Length;
            object xml=Activator.CreateInstance(app.GetType("DesktopPet.Xml"),new object[]{1});
            object width=xml.GetType().GetMethod("ParseValue").Invoke(xml,new object[]{"screenW","display removal",staleIndex});
            Check((int)width==Screen.PrimaryScreen.Bounds.Width,"XML did not fall back to the primary screen");
            using(Form pet=(Form)Activator.CreateInstance(app.GetType("DesktopPet.FormPet"))) {
                pet.GetType().GetField("DisplayIndex",Flags).SetValue(pet,staleIndex);
                Rectangle area=(Rectangle)pet.GetType().GetProperty("ScreenArea",Flags).GetValue(pet,null);
                Check(area==Screen.PrimaryScreen.WorkingArea,"Pet did not resolve the removed display");
                Check((int)pet.GetType().GetField("DisplayIndex",Flags).GetValue(pet)<Screen.AllScreens.Length,"Pet kept a stale display index");
                pet.Location=new Point(SystemInformation.VirtualScreen.Right+500,SystemInformation.VirtualScreen.Bottom+500);
                pet.GetType().GetMethod("RecoverDisplayLayout",Flags).Invoke(pet,null);
                Check(Screen.FromRectangle(pet.Bounds).WorkingArea.IntersectsWith(pet.Bounds),"Pet remained outside the remaining desktop");
                pet.Dispose();
                pet.GetType().GetMethod("RecoverDisplayLayout",Flags).Invoke(pet,null);
            }
            Console.WriteLine("PASS removed-display indices in XML and pet bounds");
            return 0;
        } catch(Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}

