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

// Real sprite and motion timers remain enabled across transitions. Reflection is
// used only to select the starting animation and inspect state, not to advance time.
class ContinuousTransitionTests
{
    static readonly BindingFlags F = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
    static object Field(object o, string n) { return o.GetType().GetField(n, F).GetValue(o); }
    static void Set(object o, string n, object v) { o.GetType().GetField(n, F).SetValue(o, v); }
    static object Call(object o, string n, params object[] a) { return o.GetType().GetMethod(n, F).Invoke(o, a); }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static TAnimation Current(FormPet p) { return (TAnimation)Field(p, "CurrentAnimation"); }
    static double X(FormPet p) { return (double)Field(p, "PositionX"); }
    static double Y(FormPet p) { return (double)Field(p, "PositionY"); }
    static Timer SpriteTimer(FormPet p) { return (Timer)Field(p, "timer1"); }
    static void CheckFrame(FormPet pet, int frame)
    {
        var displayed = ((PictureBox)Field(pet, "pictureBox1")).Image;
        Check(displayed != null, "Expected sprite was not published");
        using (var actual = new Bitmap(displayed))
        using (var expected = new Bitmap((Image)Call(pet, "GetSprite", frame)))
        {
            Check(actual.Size == expected.Size, "Displayed sprite size changed");
            for (int y = 0; y < actual.Height; y++)
                for (int x = 0; x < actual.Width; x++)
                    Check(actual.GetPixel(x, y) == expected.GetPixel(x, y), "Displayed sprite differs from expected frame " + frame);
        }
    }
    static void Wait(Func<bool> done, int timeout)
    {
        var clock = Stopwatch.StartNew();
        while (!done() && clock.ElapsedMilliseconds < timeout) { Application.DoEvents(); Thread.Sleep(1); }
        Check(done(), "Timed out waiting for native animation timers");
    }
    static FormPet Pet(Animations animations, Xml xml, int display, int id)
    {
        var pet = new FormPet(animations, xml) { Name = "continuous-transition-test" };
        foreach (Image sprite in xml.sprites) pet.AddImage(sprite);
        pet.Show(xml.spriteWidth, xml.spriteHeight);
        Set(pet, "DisplayIndex", display);
        Rectangle area = Screen.AllScreens[display].WorkingArea;
        pet.Location = new Point(area.Right - pet.Width - 20, area.Bottom - pet.Height);
        Set(pet, "PositionX", (double)pet.Left); Set(pet, "PositionY", (double)pet.Top);
        Set(pet, "IsMovingLeft", true);
        Call(pet, "ResetMotion"); Call(pet, "SetNewAnimation", id);
        pet.Opacity = 1;
        return pet;
    }
    static void StockRun(Animations animations, Xml xml, int display)
    {
        using (var pet = Pet(animations, xml, display, 35))
        {
            TAnimation start = Current(pet);
            double origin = X(pet), previousEndpoint = origin, finalEndpoint = origin;
            bool terminalObserved = false, published = false;
            int nextTicks = 0, terminalMoves = 0;
            double scheduledAt = 0;
            int duration = 0;
            var watch = Stopwatch.StartNew();
            pet.LocationChanged += (s, e) => { if (terminalObserved && !published) terminalMoves++; };
            SpriteTimer(pet).Tick += (s, e) => {
                if (Current(pet).ID == 35)
                {
                    int nextStep = (int)Field(pet, "AnimationStep");
                    // Stock run_begin has eight frames and no repeats. Validate
                    // actual pixels on every tick, including its terminal hold.
                    CheckFrame(pet, start.Sequence.Frames[Math.Min(nextStep, start.Sequence.TotalSteps) - 1]);
                    if (nextStep == start.Sequence.TotalSteps) previousEndpoint = X(pet);
                    if (nextStep == start.Sequence.TotalSteps + 1)
                    {
                        Check(Math.Abs(pet.Left - previousEndpoint) <= 1, "Final segment teleported at sequence boundary");
                        Check(X(pet) < previousEndpoint, "Terminal displacement was discarded");
                        terminalObserved = true; finalEndpoint = X(pet);
                        scheduledAt = watch.Elapsed.TotalMilliseconds; duration = SpriteTimer(pet).Interval;
                    }
                }
                else if (!published)
                {
                    Check(Current(pet).ID == 7 && terminalObserved, "run published before its final segment received an interval");
                    CheckFrame(pet, animations.GetAnimation(7).Sequence.Frames[0]);
                    Check(watch.Elapsed.TotalMilliseconds - scheduledAt >= duration - 5, "Terminal interval was shortened");
                    Check(pet.Left == (int)finalEndpoint, "run published before its endpoint was reached");
                    Check(terminalMoves >= 2, "Terminal segment did not produce intermediate native positions");
                    double expected = origin;
                    for (int step = 0; step <= start.Sequence.TotalSteps; step++)
                        expected += start.Start.X.Value + (start.End.X.Value - start.Start.X.Value) * (double)step / (start.Sequence.TotalSteps - 1);
                    Check(Math.Abs(X(pet) - expected) < .001, "Transition changed the legacy logical travel distance");
                    published = true;
                }
                else nextTicks++;
            };
            SpriteTimer(pet).Start();
            Wait(() => published && nextTicks >= 2, 6000);
            Check(Current(pet).ID == 7 && SpriteTimer(pet).Interval == 100, "Successor did not keep running at its XML cadence");
            Console.WriteLine("PASS stock run_begin -> run: actual frame sequence and terminal hold, final interpolation, distance and successor cadence");
        }
    }
    static void RepeatedFrames(Animations animations, Xml xml, int display, object data, bool smooth)
    {
        var start = Fixture(930, -1, 0, 931);
        start.Sequence.Frames.Clear(); start.Sequence.Frames.AddRange(new[] { 2, 3, 4, 5 });
        start.Sequence.RepeatFrom = 2; start.Sequence.TotalSteps = 6;
        animations.SaveAnimation(start, 930);
        animations.SaveAnimation(Fixture(931, 0, 0, 931), 931);
        Call(data, "SetIntValue", "Smooth", smooth ? 1 : 0);
        using (var pet = Pet(animations, xml, display, 930))
        {
            // Explicit oracle, independent of the product's modulo expression.
            int[] expected = smooth ? new[] { 2, 3, 4, 5, 4, 5, 5 } : new[] { 2, 3, 4, 5, 4, 5 };
            int ticks = 0; bool done = false;
            SpriteTimer(pet).Tick += (s, e) => {
                if (Current(pet).ID == 930)
                {
                    Check(ticks < expected.Length, "Repeated sequence published an extra tick");
                    CheckFrame(pet, expected[ticks++]);
                }
                else
                {
                    Check(Current(pet).ID == 931 && ticks == expected.Length, "Repeated sequence ended at the wrong frame");
                    CheckFrame(pet, 0); done = true; SpriteTimer(pet).Stop();
                }
            };
            SpriteTimer(pet).Start(); Wait(() => done, 4000);
            Console.WriteLine("PASS actual repeated frame sequence with RepeatFrom=2, smooth={0}", smooth);
        }
    }
    static void StockOffset(Animations animations, Xml xml, int display)
    {
        using (var pet = Pet(animations, xml, display, 10))
        {
            bool published = false;
            bool verticalDrift = false;
            int nextTicks = 0;
            SpriteTimer(pet).Tick += (s, e) => {
                if (Current(pet).ID != 1) return;
                published = true; nextTicks++;
                Check((double)Field(pet, "OffsetY") == 0 && pet.Top == (int)Y(pet), "walk inherited the previous OffsetY");
            };
            pet.LocationChanged += (s, e) => {
                // This event can run inside the product's sprite-tick try/catch.
                // Record failure here and assert outside it, avoiding its error dialog.
                if (published && pet.Top != (int)Y(pet)) verticalDrift = true;
            };
            SpriteTimer(pet).Start();
            Wait(() => nextTicks >= 3, 6000);
            Check(!verticalDrift, "Old OffsetY drifted during the successor animation");
            Console.WriteLine("PASS stock fall hard -> walk: correct first-frame OffsetY and no subsequent vertical drift");
        }
    }
    static TAnimation Fixture(int id, int x, int offset, int next)
    {
        var a = new TAnimation("continuous child fixture", id);
        a.Start.X.Value = a.End.X.Value = x;
        a.Start.OffsetY = a.End.OffsetY = offset;
        a.Start.Interval.Value = a.End.Interval.Value = 120;
        a.Start.Opacity = a.End.Opacity = 1;
        a.Sequence.Frames.Add(0); a.Sequence.Frames.Add(1); a.Sequence.TotalSteps = 2;
        a.EndAnimation.Add(new TNextAnimation(next, 1, TNextAnimation.TOnly.NONE));
        return a;
    }
    static void OffsetChild(Animations animations, Xml xml, int display)
    {
        animations.SaveAnimation(Fixture(900, -4, 2, 901), 900);
        animations.SaveAnimation(Fixture(901, 0, -3, 903), 901);
        animations.SaveAnimation(Fixture(902, 0, 0, 902), 902);
        animations.SaveAnimation(Fixture(903, 0, -3, 903), 903);
        var childInfo = new TChild { AnimationID = 901, Next = 902 };
        childInfo.Position.X = xml.GetXMLCompute("imageX+imageW", "child X");
        childInfo.Position.Y = xml.GetXMLCompute("imageY", "child Y");
        animations.SheepChild[901] = new List<TChild> { childInfo };
        using (var pet = Pet(animations, xml, display, 900))
        {
            FormPet child = null;
            int childTicks = 0;
            SpriteTimer(pet).Tick += (s, e) => {
                if (Current(pet).ID == 900)
                    Check(((IList)Field(pet, "childs")).Count == 0, "Child appeared before old movement completed");
                else if (child == null)
                {
                    Check(Current(pet).ID == 901, "Unexpected successor");
                    Check(pet.Top == (int)(Y(pet) + Current(pet).Start.OffsetY), "New offset missing at child publication");
                    var children = (IList)Field(pet, "childs");
                    Check(children.Count == 1, "Child publication count changed");
                    child = (FormPet)children[0];
                    Check(child.Top == pet.Top && child.Left == pet.Left + xml.spriteWidth, "Child anchor uses the old offset");
                    SpriteTimer(child).Tick += (cs, ce) => { childTicks++; };
                }
            };
            try
            {
                SpriteTimer(pet).Start();
                Wait(() => Current(pet).ID == 903 && childTicks >= 3, 4000);
                Check(child.Top == pet.Top && child.Left == pet.Left + xml.spriteWidth, "Parent and child diverged after continuous playback");
                Check(SpriteTimer(pet).Enabled && SpriteTimer(child).Enabled, "Test bypassed continuous parent/child timers");
                Console.WriteLine("PASS nonzero/negative OffsetY with real child publication and consecutive live sequences");
            }
            finally { foreach (Form c in (IList)Field(pet, "childs")) c.Dispose(); }
        }
    }
    static void PendingControls(Animations animations, Xml xml, int display, object data)
    {
        animations.SaveAnimation(Fixture(910, -8, 0, 911), 910);
        animations.SaveAnimation(Fixture(911, 0, 0, 911), 911);
        animations.SaveAnimation(Fixture(912, 0, 0, 912), 912);
        Call(data, "SetIntValue", "Smooth", 1);
        using (var pet = Pet(animations, xml, display, 910))
        {
            // Select only the terminal step; completion still uses the real timers.
            Set(pet, "AnimationStep", 2);
            Call(pet, "Timer1_Tick", null, EventArgs.Empty);
            Check(Current(pet).ID == 910, "Movement did not defer its transition");
            Call(data, "SetIntValue", "Smooth", 0);
            Wait(() => !((Timer)Field(pet, "motionTimer")).Enabled, 1000);
            Check(pet.Left == (int)X(pet) && !(bool)Field(pet, "timerResolutionRequested"), "Disabling during final movement did not settle/release timing");
            Call(data, "SetIntValue", "Smooth", 1);
            Wait(() => Current(pet).ID == 911, 1000);
            Check(pet.Left == (int)X(pet), "Re-enabling resurrected a terminal trajectory");
        }
        int originalSync = animations.AnimationSync;
        animations.AnimationSync = 912;
        try
        {
            using (var pet = Pet(animations, xml, display, 910))
            {
                Set(pet, "AnimationStep", 2); Call(pet, "Timer1_Tick", null, EventArgs.Empty);
                pet.Sync();
                int ticks = 0;
                SpriteTimer(pet).Tick += (s, e) => { Check(Current(pet).ID == 912, "Canceled transition overwrote explicit sync"); ticks++; };
                Wait(() => ticks >= 3, 2000);
            }
        }
        finally { animations.AnimationSync = originalSync; }
        using (var pet = Pet(animations, xml, display, 910))
        {
            Set(pet, "AnimationStep", 2); Call(pet, "Timer1_Tick", null, EventArgs.Empty);
            int callbacks = 0; SpriteTimer(pet).Tick += (s, e) => { callbacks++; };
            pet.Close();
            var clock = Stopwatch.StartNew();
            Wait(() => clock.ElapsedMilliseconds >= 180, 1000);
            Check(callbacks == 0 && !(bool)Field(pet, "timerResolutionRequested") && (IntPtr)Field(pet, "followHook") == IntPtr.Zero,
                "Closing during pending movement leaked a callback or timing resource");
        }
        Console.WriteLine("PASS pending transition: live off/on, explicit sync cancellation, close cleanup");
    }
    static void PendingInterruptions(Animations animations, Xml xml, int display, object data)
    {
        animations.SaveAnimation(Fixture(920, -8, 0, 921), 920);
        animations.SaveAnimation(Fixture(921, 0, 0, 921), 921);
        Call(data, "SetIntValue", "Smooth", 1);
        int originalToss = animations.AnimationToss;
        try
        {
            foreach (string action in new[] { "drag-release", "toss-without-animation", "toss-with-animation", "respawn", "close" })
            using (var pet = Pet(animations, xml, display, 920))
            {
                pet.Top = Screen.AllScreens[display].WorkingArea.Top + 200;
                Set(pet, "PositionY", (double)pet.Top); Call(pet, "ResetMotion");
                Set(pet, "AnimationStep", 2); Call(pet, "Timer1_Tick", null, EventArgs.Empty);
                Check((bool)Field(pet, "transitionPending"), "Interruption did not start during pending movement");
                bool stale = false; int callbacks = 0;
                SpriteTimer(pet).Tick += (s, e) => { callbacks++; if (Current(pet).ID == 921) stale = true; };
                if (action == "respawn") pet.Play(false, 0);
                else if (action == "close") pet.Close();
                else
                {
                    // Invoke production handlers without moving the cursor or
                    // injecting desktop input. Toss velocity is deterministic.
                    animations.AnimationToss = action == "toss-without-animation" ? -1 : animations.AnimationDrag;
                    Call(pet, "PictureBox1_MouseDown", null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    Check(!(bool)Field(pet, "transitionPending") && (bool)Field(pet, "IsDragging"), "MouseDown did not cancel pending movement");
                    if (action.StartsWith("toss"))
                    {
                        double now = ((Stopwatch)Field(pet, "motionClock")).Elapsed.TotalMilliseconds;
                        Call(Field(pet, "dragVelocity"), "Reset", (double)pet.Left - 100, (double)pet.Top, now - 25);
                        Call(Field(pet, "dragVelocity"), "Add", (double)pet.Left, (double)pet.Top, now - 10);
                    }
                    Call(pet, "PictureBox1_MouseUp", null, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                    if (action.StartsWith("toss")) Check((bool)Field(pet, "IsTossing"), "MouseUp did not enter toss physics");
                    Check(!(bool)Field(pet, "IsDragging") && !((PictureBox)Field(pet, "pictureBox1")).Capture, "MouseUp retained drag capture");
                }
                if (action != "close") Check(!(bool)Field(pet, "transitionPending"), action + " retained a pending successor");
                var wait = Stopwatch.StartNew(); Wait(() => wait.ElapsedMilliseconds >= 450, 1500);
                Check(!stale, action + " resurrected the old successor after its deadline");
                if (action == "close")
                    Check(callbacks == 0 && !(bool)Field(pet, "timerResolutionRequested") && (IntPtr)Field(pet, "followHook") == IntPtr.Zero,
                        "Closing retained a callback, timer resolution or hook");
                Console.WriteLine("PASS pending interruption: {0}", action);
            }
        }
        finally { animations.AnimationToss = originalToss; }
    }
    [STAThread] static int Main(string[] args)
    {
        // Surface callback assertion failures rather than allowing WinForms to show
        // a dialog and continue a failed test.
        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
        try
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            int scale = args.Length > 0 ? int.Parse(args[0]) : 1;
            int display = args.Length > 1 ? int.Parse(args[1]) : 0;
            string suite = args.Length > 2 ? args[2] : "all";
            var app = typeof(FormPet).Assembly;
            var program = app.GetType("DesktopPet.Program");
            program.GetField("Mainthread", F).SetValue(null, FormatterServices.GetUninitializedObject(app.GetType("DesktopPet.StartUp")));
            object data = program.GetField("MyData", F).GetValue(null);
            Call(data, "SetScale", scale); Call(data, "SetVolume", 0.0); Call(data, "SetWindowForeground", false);
            Call(data, "SetXml", File.ReadAllText("motion-fixture.xml"), "continuous-transition-tests");
            using (var xml = new Xml(scale))
            {
                Check(xml.ReadXML(), "Stock XML failed to load");
                var animations = new Animations(xml); xml.LoadAnimations(animations);
                Call(data, "SetIntValue", "Smooth", 1);
                if (suite != "offset-only") StockRun(animations, xml, display);
                if (suite == "run-only") return 0;
                foreach (bool smooth in new[] { true, false })
                {
                    Call(data, "SetIntValue", "Smooth", smooth ? 1 : 0);
                    StockOffset(animations, xml, display);
                    OffsetChild(animations, xml, display);
                    RepeatedFrames(animations, xml, display, data, smooth);
                    Console.WriteLine("PASS continuous mode={0}, scale={1}, display={2}", smooth, scale, display);
                }
                PendingControls(animations, xml, display, data);
                PendingInterruptions(animations, xml, display, data);
            }
            return 0;
        }
        catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
