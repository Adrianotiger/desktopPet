using System;
using System.Collections;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using DesktopPet;

// Hidden-window audit: no Show(), desktop input, installed configuration or message pumping.
// A failure is an assertion of desired behavior, not an expected-bug success.
class MotionAuditTests
{
    static BindingFlags F = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static object Field(object o, string n)
    {
        var field = o.GetType().GetField(n, F);
        return field != null ? field.GetValue(o) : o.GetType().GetProperty(n, F).GetValue(o, null);
    }
    static void Set(object o, string n, object v) { o.GetType().GetField(n, F).SetValue(o, v); }
    static object Call(object o, string n, params object[] a) { return o.GetType().GetMethod(n, F).Invoke(o, a); }
    static int failures, checks;
    static void FinishTransition(FormPet pet)
    {
        var pending = pet.GetType().GetField("transitionPending", F);
        if (pending != null && (bool)pending.GetValue(pet))
        {
            System.Threading.Thread.Sleep(((Timer)Field(pet, "timer1")).Interval + 10);
            Call(pet, "NextStep");
        }
    }
    static void Assert(bool ok, string name, string detail)
    {
        checks++;
        if (!ok) failures++;
        Console.WriteLine("{0} {1}: {2}", ok ? "PASS" : "FAIL", name, detail);
    }
    static TAnimation Animation(int id, int x, int y)
    {
        var a = new TAnimation("audit", id);
        a.Start.X.Value = a.End.X.Value = x;
        a.Start.Y.Value = a.End.Y.Value = y;
        a.Start.Interval.Value = a.End.Interval.Value = 200;
        a.Start.Opacity = a.End.Opacity = 1;
        a.Sequence.Frames.Add(0); a.Sequence.Frames.Add(1);
        a.Sequence.TotalSteps = 4;
        a.EndAnimation.Add(new TNextAnimation(901, 1, TNextAnimation.TOnly.NONE));
        a.EndBorder.Add(new TNextAnimation(901, 1, TNextAnimation.TOnly.NONE));
        return a;
    }
    static FormPet Pet(Animations a, Xml xml, Point p, bool smooth)
    {
        var pet = new FormPet(a, xml);
        pet.Name = "audit";
        pet.StartPosition = FormStartPosition.Manual;
        pet.Size = new Size(40, 40);
        pet.Location = p;
        using (var b = new Bitmap(40, 40))
        {
            pet.AddImage(b); pet.AddImage(b);
            // ImageList creates native copies lazily; keep its inputs alive until that happens.
            IntPtr copied = ((ImageList)Field(pet, "imageList1")).Handle;
        }
        Set(pet, "PositionX", (double)p.X); Set(pet, "PositionY", (double)p.Y);
        Set(pet, "IsMovingLeft", true);
        var setting = pet.GetType().GetField("UseSmoothMovements", F);
        if (setting != null) setting.SetValue(pet, smooth);
        else if (pet.GetType().GetProperty("UseSmoothMovements", F) != null)
        {
            object data = typeof(FormPet).Assembly.GetType("DesktopPet.Program").GetField("MyData", F).GetValue(null);
            Call(data, "SetIntValue", "Smooth", smooth ? 1 : 0);
        }
        Set(pet, "CurrentAnimation", a.GetAnimation(900)); Set(pet, "AnimationStep", 0);
        if (pet.GetType().GetMethod("ResetMotion", F) != null) Call(pet, "ResetMotion");
        return pet;
    }
    [DllImport("user32.dll")] static extern bool GetWindowRect(IntPtr w, out Rect r);
    struct Rect { public int Left, Top, Right, Bottom; }
    [STAThread] static int Main(string[] args)
    {
        try
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            var app = typeof(FormPet).Assembly;
            object data = app.GetType("DesktopPet.Program").GetField("MyData", F).GetValue(null);
            Call(data, "SetScale", 1);
            Call(data, "SetVolume", 0.0);
            var xml = new Xml(1); xml.spriteWidth = xml.spriteHeight = 40;
            var a = new Animations(xml);
            a.SaveAnimation(Animation(900, -20, 0), 900);
            a.SaveAnimation(Animation(901, 0, 0), 901);
            var area = Screen.PrimaryScreen.WorkingArea;
            // At the instant the collision frame is selected, its presentation must be at the boundary.
            using (var pet = Pet(a, xml, new Point(area.Left + 10, area.Bottom - 40), true))
            {
                Call(pet, "NextStep");
                FinishTransition(pet);
                int id = ((TAnimation)Field(pet, "CurrentAnimation")).ID;
                Assert(id == 901 && pet.Left == area.Left, "collision-position-and-frame",
                    "animation=" + id + ", logicalX=" + Field(pet, "PositionX") + ", visibleX=" + pet.Left + ", border=" + area.Left);
                // Probe the production constructor's inputs AFTER NextStep, without creating visible children.
                // This is not an end-to-end HasAnimationChild/PlayChild test. Their earlier creation order
                // already exists in the baseline and is separately identified by source review.
                Point anchor = new Point(pet.Left - Screen.PrimaryScreen.Bounds.X, pet.Top - Screen.PrimaryScreen.Bounds.Y);
                using (var child = new FormPet(a, xml, anchor, false, 0))
                {
                    Assert(xml.parentX == (double)Field(pet, "PositionX"), "child-anchor-at-collision",
                        "parentAnchorX=" + xml.parentX + ", collisionX=" + Field(pet, "PositionX"));
                }
            }
            bool hasMotion = typeof(FormPet).GetField("motion", F) != null;
            // An ordinary frame must start from the previous frame's endpoint after its interval has elapsed.
            if (hasMotion) using (var pet = Pet(a, xml, new Point(area.Left + 200, area.Bottom - 40), true))
            {
                Call(pet, "NextStep");
                object track = Field(pet, "motion");
                Set(track, "started", -1000.0); // Simulate an elapsed interval without dispatching any window events.
                Set(pet, "AnimationStep", 1);
                Call(pet, "NextStep");
                Assert(pet.Left == area.Left + 180, "sprite-tick-commits-elapsed-position",
                    "visibleX=" + pet.Left + ", previousEndpoint=" + (area.Left + 180));
            }
            // A stationary sequence must not inherit a full interval of movement from its predecessor.
            if (hasMotion) using (var pet = Pet(a, xml, new Point(area.Left + 200, area.Bottom - 40), true))
            {
                Set(pet, "AnimationStep", 4);
                Call(pet, "NextStep");
                FinishTransition(pet);
                Assert(!((bool)Call(Field(pet, "motion"), "IsMoving", 0.0)), "sequence-transition-motion",
                    "animation=" + ((TAnimation)Field(pet, "CurrentAnimation")).ID + ", logicalX=" + Field(pet, "PositionX") + ", visibleX=" + pet.Left);
            }
            if (typeof(FormPet).GetField("UseSmoothMovements", F) != null || typeof(FormPet).GetProperty("UseSmoothMovements", F) != null)
            {
                using (var pet = Pet(a, xml, new Point(area.Left + 10, area.Bottom - 40), false))
                {
                    Call(pet, "NextStep");
                    FinishTransition(pet);
                    Assert(pet.Left == area.Left, "disabled-smoothing-collision-control",
                        "visibleX=" + pet.Left + ", border=" + area.Left);
                }
                // Hidden native support HWND, moved by code; no foreground or input operations.
                using (var platform = new Form())
                using (var pet = Pet(a, xml, new Point(area.Left + 200, area.Top + 200), false))
                {
                    platform.StartPosition = FormStartPosition.Manual;
                    platform.Bounds = new Rectangle(area.Left + 100, area.Top + 400, 600, 100);
                    IntPtr handle = platform.Handle;
                    Rect r; if (!GetWindowRect(handle, out r)) throw new Exception("Hidden support HWND is unavailable");
                    object nativeRect = Activator.CreateInstance(app.GetType("DesktopPet.NativeMethods+RECT"));
                    Set(nativeRect, "Left", r.Left); Set(nativeRect, "Top", r.Top);
                    Set(nativeRect, "Right", r.Right); Set(nativeRect, "Bottom", r.Bottom);
                    Set(pet, "hwndWindow", handle); Set(pet, "currentWindowSize", nativeRect);
                    Call(pet, "NextStep"); // Disabled interpolation moves Left, but its track can remain at the spawn.
                    int before = pet.Left;
                    platform.Left += 40;
                    Call(pet, "FollowWindow");
                    Assert(pet.Left == before + 40, "disabled-smoothing-window-follow",
                        "before=" + before + ", actual=" + pet.Left + ", expected=" + (before + 40));
                }
                // Exercise the same setting write used by the options UI, in the isolated runtime config.
                Call(data, "SetIntValue", "Smooth", 0);
                using (var existing = Pet(a, xml, new Point(200, 200), false))
                {
                    Call(data, "SetIntValue", "Smooth", 1);
                    bool saved = (bool)Call(data, "GetSmoothMovements");
                    Assert((bool)Field(existing, "UseSmoothMovements") == saved, "existing-pet-applies-option",
                        "setting=" + saved + ", existing=" + Field(existing, "UseSmoothMovements"));
                    // This overload is used by every production child; the default remains false.
                    using (var parent = new FormPet(a, xml))
                    using (var child = new FormPet(a, xml, parent.Location, false, 0))
                    {
                        Assert((bool)Field(parent, "UseSmoothMovements") == saved, "new-parent-reads-option-control",
                            "setting=" + saved + ", parent=" + Field(parent, "UseSmoothMovements"));
                        Assert((bool)Field(child, "UseSmoothMovements") == (bool)Field(parent, "UseSmoothMovements"),
                            "child-inherits-smoothing", "parent=" + Field(parent, "UseSmoothMovements") + ", child=" + Field(child, "UseSmoothMovements"));
                    }
                    Call(data, "SetIntValue", "Smooth", 0);
                }
            }
            Console.WriteLine("AUDIT assertions={0}, failures={1}; hidden state checks, not visible desktop acceptance", checks, failures);
            return failures == 0 ? 0 : 1;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 2; }
    }
}
