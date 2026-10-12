using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using DesktopPet;
using Timer = System.Windows.Forms.Timer;

// Visible native checks for animation publication, actual child creation and live option changes.
class TransitionRuntimeTests
{
    static BindingFlags F = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static object Field(object o, string n) { return o.GetType().GetField(n, F).GetValue(o); }
    static void Set(object o, string n, object v) { o.GetType().GetField(n, F).SetValue(o, v); }
    static object Call(object o, string n, params object[] a) { return o.GetType().GetMethod(n, F).Invoke(o, a); }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Pump(int ms) { var c = Stopwatch.StartNew(); while (c.ElapsedMilliseconds < ms) { Application.DoEvents(); Thread.Sleep(1); } }
    static TAnimation Animation(int id, int x, int y)
    {
        var a = new TAnimation("transition fixture", id);
        a.Start.X.Value = a.End.X.Value = x;
        a.Start.Y.Value = a.End.Y.Value = y;
        a.Start.Interval.Value = a.End.Interval.Value = 200;
        a.Start.Opacity = a.End.Opacity = 1;
        a.Sequence.Frames.Add(id == 900 ? 0 : 1);
        a.Sequence.Frames.Add(id == 900 ? 0 : 1);
        a.Sequence.TotalSteps = 4;
        a.EndAnimation.Add(new TNextAnimation(id == 900 ? 901 : id, 1, TNextAnimation.TOnly.NONE));
        a.EndBorder.Add(new TNextAnimation(901, 1, TNextAnimation.TOnly.NONE));
        return a;
    }
    static FormPet Pet(Animations animations, Xml xml, Point location, int display)
    {
        var pet = new FormPet(animations, xml);
        pet.Name = "transition-test";
        foreach (Image sprite in xml.sprites) pet.AddImage(sprite);
        pet.Show(xml.spriteWidth, xml.spriteHeight);
        Set(pet, "DisplayIndex", display);
        ((Timer)Field(pet, "timer1")).Stop();
        pet.Location = location; pet.Opacity = 1;
        Set(pet, "PositionX", (double)location.X); Set(pet, "PositionY", (double)location.Y);
        Set(pet, "IsMovingLeft", true);
        Set(pet, "CurrentAnimation", animations.GetAnimation(900)); Set(pet, "AnimationStep", 0);
        Call(pet, "ResetMotion");
        return pet;
    }
    static void CloseChildren(FormPet pet)
    {
        foreach (Form child in (IList)Field(pet, "childs")) child.Dispose();
        ((IList)Field(pet, "childs")).Clear();
    }
    [STAThread] static int Main(string[] args)
    {
        try
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            int scale = args.Length > 0 ? int.Parse(args[0]) : 1;
            int display = args.Length > 1 ? int.Parse(args[1]) : 0;
            var app = typeof(FormPet).Assembly;
            var program = app.GetType("DesktopPet.Program");
            program.GetField("Mainthread", F).SetValue(null, FormatterServices.GetUninitializedObject(app.GetType("DesktopPet.StartUp")));
            object data = program.GetField("MyData", F).GetValue(null);
            Call(data, "SetScale", scale); Call(data, "SetVolume", 0.0);
            Call(data, "SetWindowForeground", false);
            Call(data, "SetXml", File.ReadAllText("motion-fixture.xml"), "transition-tests");
            var xml = new Xml(scale);
            Check(xml.ReadXML(), "Stock sprite fixture did not load");
            var animations = new Animations(xml);
            animations.SaveAnimation(Animation(900, -20 * scale, 0), 900);
            animations.SaveAnimation(Animation(901, 0, 0), 901);
            animations.SaveAnimation(Animation(902, 0, 0), 902);
            var childInfo = new TChild { AnimationID = 901, Next = 902 };
            childInfo.Position.X = xml.GetXMLCompute("imageX+imageW", "child X");
            childInfo.Position.Y = xml.GetXMLCompute("imageY", "child Y");
            animations.SheepChild[901] = new List<TChild> { childInfo };
            Rectangle area = Screen.AllScreens[display].WorkingArea;
            foreach (bool smooth in new[] { false, true })
            {
                Call(data, "SetIntValue", "Smooth", smooth ? 1 : 0);
                // Exercise every border path, plus the production sequence-to-child path.
                foreach (string scenario in new[] { "screen-left", "screen-right", "screen-top", "taskbar", "window-left", "window-right", "window-landing", "window-landing-dynamic", "sequence" })
                using (var platform = new Form())
                {
                    platform.Text = "DesktopPet transition support";
                    platform.TopMost = true;
                    platform.StartPosition = FormStartPosition.Manual;
                    platform.Bounds = new Rectangle(area.Left + 50, area.Top + 450, Math.Min(360, area.Width - 80), 150);
                    bool window = scenario.StartsWith("window-");
                    if (window) { platform.Show(); Pump(20); }
                    int dx = -20 * scale, dy = 0;
                    Point start = new Point(area.Left + 10 * scale, area.Bottom - xml.spriteHeight);
                    Point expected = new Point(area.Left, start.Y);
                    switch (scenario)
                    {
                        case "screen-right": dx = 20 * scale; start.X = area.Right - xml.spriteWidth - 10 * scale; expected.X = area.Right - xml.spriteWidth; break;
                        case "screen-top": dx = 0; dy = -20 * scale; start = new Point(area.Left + 100, area.Top + 10 * scale); expected = new Point(start.X, area.Top); break;
                        case "taskbar": dx = 0; dy = 20 * scale; start = new Point(area.Left + 100, area.Bottom - xml.spriteHeight - 10 * scale); expected = new Point(start.X, area.Bottom - xml.spriteHeight); break;
                        case "window-left": start = new Point(platform.Left + 10 * scale, platform.Top - xml.spriteHeight); expected = new Point(platform.Left, start.Y); break;
                        case "window-right": dx = 20 * scale; start = new Point(platform.Right - xml.spriteWidth - 10 * scale, platform.Top - xml.spriteHeight); expected = new Point(platform.Right - xml.spriteWidth, start.Y); break;
                        case "window-landing-dynamic":
                        case "window-landing": dx = 0; dy = 20 * scale; start = new Point(platform.Left + 20, platform.Top - xml.spriteHeight - 10 * scale); expected = new Point(start.X, platform.Top - xml.spriteHeight); break;
                        case "sequence": start.X = area.Left + 120 * scale; expected.X = start.X - 20 * scale; break;
                    }
                    animations.SaveAnimation(Animation(900, dx, dy), 900);
                    var landingAnimation = Animation(901, 0, 0);
                    if (scenario == "window-landing-dynamic")
                        landingAnimation.Start.Y = new TValue { IsDynamic = true, Compute = "1", Value = 0 };
                    animations.SaveAnimation(landingAnimation, 901);
                    using (var pet = Pet(animations, xml, start, display))
                    {
                        if (window && !scenario.StartsWith("window-landing"))
                        {
                            object rect = Activator.CreateInstance(app.GetType("DesktopPet.NativeMethods+RECT"));
                            Set(rect, "Left", platform.Left); Set(rect, "Right", platform.Right);
                            Set(rect, "Top", platform.Top); Set(rect, "Bottom", platform.Bottom);
                            Set(pet, "hwndWindow", platform.Handle); Set(pet, "currentWindowSize", rect);
                        }
                        bool committedBeforeTransition = false;
                        pet.LocationChanged += (s, e) => {
                            if (pet.Location == expected) committedBeforeTransition = ((TAnimation)Field(pet, "CurrentAnimation")).ID == 900;
                        };
                        if (scenario == "sequence") Set(pet, "AnimationStep", 4);
                        var spriteTimer = (Timer)Field(pet, "timer1");
                        spriteTimer.Tick += (s, e) => {
                            if (((TAnimation)Field(pet, "CurrentAnimation")).ID == 901) spriteTimer.Stop();
                        };
                        Call(pet, "Timer1_Tick", null, EventArgs.Empty);
                        if (((TAnimation)Field(pet, "CurrentAnimation")).ID == 900)
                        {
                            Check(smooth && ((IList)Field(pet, "childs")).Count == 0,
                                "Pending movement published its child before completion");
                            var wait = Stopwatch.StartNew();
                            while (((TAnimation)Field(pet, "CurrentAnimation")).ID == 900 && wait.ElapsedMilliseconds < 1500) Pump(1);
                        }
                        spriteTimer.Stop();
                        Check(pet.Location == expected && ((TAnimation)Field(pet, "CurrentAnimation")).ID == 901, scenario + ": transition was published before its position; actual=" + pet.Location + ", expected=" + expected);
                        if (scenario == "window-landing-dynamic")
                            Check((IntPtr)Field(pet, "hwndWindow") == IntPtr.Zero, "Dynamic landing animation retained its old window attachment");
                        Check(committedBeforeTransition, "Position was not committed before the new animation became observable");
                        var children = (IList)Field(pet, "childs");
                        Check(children.Count == 1, "Production SetNewAnimation did not create exactly one child");
                        var child = (FormPet)children[0];
                        ((Timer)Field(child, "timer1")).Stop();
                        Check(child.Left == pet.Left + xml.spriteWidth && child.Top == pet.Top, "PlayChild anchor does not match the committed parent");
                        Check(!(bool)Call(Field(pet, "motion"), "IsMoving", ((Stopwatch)Field(pet, "motionClock")).Elapsed.TotalMilliseconds), "Old movement survived the transition");
                        Pump(60);
                        Check(pet.Location == expected && child.Left == expected.X + xml.spriteWidth && child.Top == expected.Y, "Parent or child drifted after transition");
                        CloseChildren(pet);
                        Console.WriteLine("PASS actual child {0}: smooth={1}, scale={2}, display={3}", scenario, smooth, scale, display);
                    }
                }
            }
            // A mode switch while interpolation is active must not resurrect a stale track.
            Call(data, "SetIntValue", "Smooth", 1);
            animations.SaveAnimation(Animation(900, -20 * scale, 0), 900);
            using (var pet = Pet(animations, xml, new Point(area.Left + 120, area.Bottom - xml.spriteHeight), display))
            {
                Call(pet, "NextStep");
                Pump(40);
                Call(data, "SetIntValue", "Smooth", 0);
                Pump(25);
                Check(pet.Left == (int)(double)Field(pet, "PositionX"), "Disabling smooth movement left an active stale track");
                Check(!((Timer)Field(pet, "motionTimer")).Enabled, "Disabled interpolation still requests the high-rate timer");
                int settled = pet.Left;
                Pump(50);
                Check(pet.Left == settled, "Disabled pet moved backwards to an old sample");
                Call(data, "SetIntValue", "Smooth", 1);
                Set(pet, "AnimationStep", 1); Call(pet, "NextStep");
                Check(pet.Left == settled, "Re-enabling smooth movement jumped before interpolation");
                Pump(240);
                Check(pet.Left == settled - 20 * scale, "Re-enabled interpolation did not finish its new segment");
                Console.WriteLine("PASS live mode switch during movement and idle timer cleanup");
            }
            xml.Dispose();
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
        finally { foreach (Form form in new List<Form>(GetOpenForms())) form.Dispose(); }
    }
    static IEnumerable<Form> GetOpenForms() { foreach (Form form in Application.OpenForms) yield return form; }
}
