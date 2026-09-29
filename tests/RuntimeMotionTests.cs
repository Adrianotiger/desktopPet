using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

class RuntimeMotionTests
{
    static BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static;
    static object Field(object o, string n) { return o.GetType().GetField(n, flags).GetValue(o); }
    static void Set(object o, string n, object v) { o.GetType().GetField(n, flags).SetValue(o, v); }
    static object Call(object o, string n, params object[] args) { return o.GetType().GetMethod(n, flags).Invoke(o, args); }
    static void Check(bool ok, string message) { if (!ok) throw new Exception(message); }
    static void Pump(int ms) { var clock=Stopwatch.StartNew(); while(clock.ElapsedMilliseconds<ms) { Application.DoEvents(); Thread.Sleep(1); } }

    [STAThread] static int Main(string[] args)
    {
        try
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);
            Assembly app = Assembly.LoadFrom("DesktopPet.exe");
            var program = app.GetType("DesktopPet.Program",true);
            object startup=FormatterServices.GetUninitializedObject(app.GetType("DesktopPet.StartUp",true));
            program.GetField("Mainthread",flags).SetValue(null,startup);
            object data=program.GetField("MyData",flags).GetValue(null);
            int scale=args.Length>0 ? int.Parse(args[0]) : 1;
            int display=args.Length>1 ? int.Parse(args[1]) : Array.FindIndex(Screen.AllScreens, s=>s.Primary);
            Call(data,"SetScale",scale);
            Call(data,"SetVolume",0.0);
            Call(data,"SetWindowForeground",false);
            Call(data,"SetXml",File.ReadAllText("motion-fixture.xml"),"motion-tests");
            object xml=Activator.CreateInstance(app.GetType("DesktopPet.Xml"),new object[]{scale});
            Check((bool)Call(xml,"ReadXML"), "Pet XML could not load");
            object animations=Activator.CreateInstance(app.GetType("DesktopPet.Animations"),new object[]{xml});
            Call(xml,"LoadAnimations",animations);
            Console.WriteLine("Stock eSheep fixture: scale={0}, display={1}",scale,display);
            using(Form child=(Form)Activator.CreateInstance(app.GetType("DesktopPet.FormPet"),new object[]{animations,xml,new Point(100,100),false,display}))
            {
                child.Name="child1";
                Check(Field(child,"motionTimer")!=null,"Child animation renderer not initialized");
                Call(child,"ResetMotion");
                Call(child,"QueueMotion",100);
                child.Dispose();
                Check(!(bool)Field(child,"timerResolutionRequested"),"Child timer resolution not released");
                Console.WriteLine("PASS child animation initializes and disposes its motion renderer");
            }
            using(Form pet=(Form)Activator.CreateInstance(app.GetType("DesktopPet.FormPet"),new object[]{animations,xml}))
            {
                foreach(Image sprite in (IList)Field(xml,"sprites")) Call(pet,"AddImage",sprite);
                pet.GetType().GetMethod("Show",new Type[]{typeof(int),typeof(int)}).Invoke(pet,new object[]{(int)Field(xml,"spriteWidth"),(int)Field(xml,"spriteHeight")});
                Set(pet,"DisplayIndex",display);
                Rectangle screen=Screen.AllScreens[display].WorkingArea;
                pet.Location=new Point(screen.Left+200,screen.Top+200);
                pet.Opacity=1;
                Call(pet,"ResetMotion");
                int firstX=pet.Left;
                Set(pet,"PositionX",(double)firstX+150); Set(pet,"PositionY",(double)pet.Top);
                int moves=0; int lastX=pet.Left;
                pet.LocationChanged += (s,e) => { if(pet.Left!=lastX) { moves++; lastX=pet.Left; } };
                Call(pet,"QueueMotion",600);
                Pump(700);
                Check(pet.Left==firstX+150,"Presentation missed endpoint");
                Check(moves>=20,"Presentation is still running at animation cadence: "+moves);
                Console.WriteLine("PASS native window motion: "+moves+" position changes during a 600 ms segment");
                Check(!((Timer)Field(pet,"motionTimer")).Enabled,"Idle renderer did not stop");

                Set(pet,"dragAnchor",new Point(12,9));
                Set(pet,"IsDragging",true);
                var watch=Stopwatch.StartNew();
                for(int i=0;i<120;i++) {
                    var cursor=new Point(screen.Left+300+i,screen.Top+300+i);
                    Call(pet,"UpdateDragPosition",cursor,(double)i*8+1000);
                    Check(pet.Left==cursor.X-12 && pet.Top==cursor.Y-9,"Drag position lags mouse event");
                }
                Set(pet,"IsDragging",false);
                Console.WriteLine("PASS 120 immediate drag positions; animation timer remained disabled");

                // Exercise real window following without a blocking sleep/pump loop.
                using(Form platform=new Form()) {
                    platform.Text="DesktopPet motion test";
                    platform.TopMost=true; // Keep unrelated desktop windows from occluding the collision fixture.
                    platform.StartPosition=FormStartPosition.Manual;
                    platform.Bounds=new Rectangle(screen.Left+150,screen.Top+Math.Min(450,screen.Height-200),600,150);
                    platform.Show();
                    object rect=Activator.CreateInstance(app.GetType("DesktopPet.NativeMethods+RECT",true));
                    Set(rect,"Left",platform.Left); Set(rect,"Right",platform.Right);
                    Set(rect,"Top",platform.Top); Set(rect,"Bottom",platform.Bottom);
                    Set(pet,"hwndWindow",platform.Handle); Set(pet,"currentWindowSize",rect);
                    pet.Location=new Point(platform.Left+100,platform.Top-pet.Height);
                    Set(pet,"PositionX",(double)pet.Left); Set(pet,"PositionY",(double)pet.Top);
                    Call(pet,"ResetMotion");
                    int before=pet.Left;
                    platform.Left+=40; platform.Top+=20;
                    watch.Restart();
                    Check((IntPtr)Field(pet,"followHook")!=IntPtr.Zero,"Window event subscription failed");
                    Check(!((Timer)Field(pet,"motionTimer")).Enabled,"Stationary support needs no high-rate polling");
                    Pump(40);
                    Check(watch.ElapsedMilliseconds<100,"Window following blocked UI thread");
                    Check(pet.Left==before+40 && pet.Top==platform.Top-pet.Height,"Window following offset incorrect");
                    Console.WriteLine("PASS attached window movement follows immediately without blocking");
                    Set(pet,"hwndWindow",IntPtr.Zero);
                    pet.Location=new Point(platform.Left+100,platform.Top-pet.Height-5);
                    Set(pet,"PositionX",(double)pet.Left); Set(pet,"PositionY",(double)pet.Top);
                    Set(pet,"IsTossing",true); Set(pet,"tossVertVel",10.0);
                    Set(pet,"tossUpdatedAt",0.0);
                    Set(pet,"hwndWindow",platform.Handle);
                    Check(!(bool)Call(pet,"CheckTopWindow",false),"Collision fixture is covered by another window");
                    Set(pet,"hwndWindow",IntPtr.Zero);
                    Call(pet,"AdvanceToss",30.0);
                    Check(!(bool)Field(pet,"IsTossing") && pet.Bottom==platform.Top && (IntPtr)Field(pet,"hwndWindow")==platform.Handle,"Toss passed through supporting window");
                    Console.WriteLine("PASS tossed pet lands on a real supporting window");
                    platform.Close();
                    Check((bool)Call(pet,"CheckTopWindow",true),"Closed supporting window was not invalidated");
                    Set(pet,"hwndWindow",IntPtr.Zero);
                    Call(pet,"ResetMotion");
                    Check((IntPtr)Field(pet,"followHook")==IntPtr.Zero,"Detached support hook was not released");
                    Check(!((Timer)Field(pet,"followFallback")).Enabled,"Detached support poller did not stop");
                }

                // Physics uses logical coordinates even while the displayed window is between steps.
                ((Timer)Field(pet,"motionTimer")).Stop();
                Set(pet,"PositionX",(double)screen.Left+300);
                Set(pet,"PositionY",(double)screen.Bottom-pet.Height);
                Set(pet,"IsMovingLeft",true);
                pet.Location=new Point(screen.Left+300,screen.Bottom-pet.Height);
                Call(pet,"ResetMotion");
                Call(pet,"SetNewAnimation",1);
                Set(pet,"AnimationStep",0);
                Call(pet,"NextStep");
                int walkTarget=screen.Left+300-2*(int)Call(data,"GetScale");
                Check((double)Field(pet,"PositionX")==walkTarget,"Walk distance changed: "+Field(pet,"PositionX"));
                Check(((Timer)Field(pet,"timer1")).Interval==200,"Sprite interval changed");
                Check(pet.Left==screen.Left+300,"Logic tick still jumps the native window");
                Pump(250);
                Check(pet.Left==walkTarget,"Walk interpolation missed endpoint");
                Console.WriteLine("PASS walking retains original distance and 200 ms sprite interval");
                Set(pet,"PositionX",(double)screen.Left+1);
                Set(pet,"PositionY",(double)screen.Bottom-pet.Height-100);
                pet.Location=new Point(screen.Left+1,screen.Bottom-pet.Height-100);
                Call(pet,"ResetMotion");
                Set(pet,"IsTossing",true);
                Type vector=pet.GetType().GetField("TossForce",flags).FieldType;
                Set(pet,"TossForce",Activator.CreateInstance(vector,new object[]{-20f,0f}));
                Set(pet,"tossVertVel",0f);
                ((Timer)Field(pet,"timer1")).Interval=30;
                double tossTime=((Stopwatch)Field(pet,"motionClock")).Elapsed.TotalMilliseconds;
                Set(pet,"tossUpdatedAt",tossTime);
                Call(pet,"AdvanceToss",tossTime+30);
                Check(pet.Left>screen.Left && pet.Left<screen.Left+10,"Bounce lost remaining horizontal travel");
                Check((double)Field(pet,"PositionY")>screen.Bottom-pet.Height-100,"Bounce paused vertical movement");
                Check(((Timer)Field(pet,"timer1")).Interval==30,"Sprite cadence changed");
                Set(pet,"PositionY",(double)screen.Bottom-pet.Height-2);
                Set(pet,"tossVertVel",10f);
                Call(pet,"AdvanceToss",tossTime+60);
                Check(!(bool)Field(pet,"IsTossing") && pet.Bottom==screen.Bottom,"Toss landing overshot taskbar");
                Console.WriteLine("PASS continuous bounce preserves residual travel and vertical movement; landing clamps correctly");
                // A delayed frame and several short frames must integrate the same flight.
                double[] results = new double[4];
                for (int scenario=0;scenario<2;scenario++) {
                    Set(pet,"PositionX",(double)screen.Left+300);
                    Set(pet,"PositionY",(double)screen.Top+5);
                    Set(pet,"TossForce",Activator.CreateInstance(vector,new object[]{10f,-4f}));
                    Set(pet,"tossVertVel",-4.0); Set(pet,"IsTossing",true);
                    Set(pet,"tossUpdatedAt",0.0);
                    if(scenario==0) Call(pet,"AdvanceToss",30.0);
                    else for(int t=5;t<=30;t+=5) Call(pet,"AdvanceToss",(double)t);
                    results[scenario*2]=(double)Field(pet,"PositionX");
                    results[scenario*2+1]=(double)Field(pet,"PositionY");
                }
                Check(Math.Abs(results[0]-results[2])<.001 && Math.Abs(results[1]-results[3])<.001,"Flight depends on render cadence");
                Set(pet,"IsTossing",false);
                Console.WriteLine("PASS elapsed-time flight is independent of render cadence");
                Set(pet,"spritesFlipped",true);
                Image flipped=(Image)Call(pet,"GetSprite",0);
                Check(Object.ReferenceEquals(flipped,Call(pet,"GetSprite",0)),"Mirrored frame was regenerated");
                Set(pet,"spritesFlipped",false);
                using(Bitmap original=new Bitmap((Image)Call(pet,"GetSprite",0)))
                using(Bitmap mirror=new Bitmap(flipped)) {
                    for(int y=0;y<original.Height;y++) for(int x=0;x<original.Width;x++)
                        Check(original.GetPixel(x,y)==mirror.GetPixel(original.Width-1-x,y),"Mirrored cache altered the original sprite");
                }
                Console.WriteLine("PASS lazy mirrored sprite cache preserves original pixels");
                Call(pet,"Play",false,0);
                ((Timer)Field(pet,"timer1")).Stop();
                object track=Field(pet,"motion");
                Check(!(bool)Call(track,"IsMoving",((Stopwatch)Field(pet,"motionClock")).Elapsed.TotalMilliseconds),"Respawn kept stale interpolation");
                Console.WriteLine("PASS respawn resets presentation instead of sliding across monitors");
                pet.Close();
                Check(((Timer)Field(pet,"motionTimer")).Enabled==false,"Disposed renderer still running");
                Check(!(bool)Field(pet,"timerResolutionRequested"),"Timer resolution not released after disposal");
                Check((IntPtr)Field(pet,"followHook")==IntPtr.Zero,"Window event hook leaked");
            }
            // Each pet owns and releases its timer request independently.
            using(Form first=(Form)Activator.CreateInstance(app.GetType("DesktopPet.FormPet")))
            using(Form second=(Form)Activator.CreateInstance(app.GetType("DesktopPet.FormPet"))) {
                first.Location=new Point(100,100); second.Location=new Point(200,100);
                Call(first,"ResetMotion"); Call(second,"ResetMotion");
                Set(first,"PositionX",150.0); Set(second,"PositionX",250.0);
                Set(first,"PositionY",100.0); Set(second,"PositionY",100.0);
                Call(first,"QueueMotion",200); Call(second,"QueueMotion",200);
                Set(first,"IsDragging",true);
                first.Dispose();
                Check(!(bool)Field(first,"IsDragging"),"Disposal left drag capture active");
                Check(!(bool)Field(first,"timerResolutionRequested"),"First pet leaked its timer request");
                Check(((Timer)Field(second,"motionTimer")).Enabled,"Disposing one pet stopped another");
                Pump(250);
                Check(second.Left==250,"Remaining pet stopped moving");
            }
            Console.WriteLine("PASS concurrent pets and disposal during dragging");
            ((IDisposable)xml).Dispose();

            return 0;
        }
        catch(Exception ex) { Console.Error.WriteLine(ex); return 1; }
    }
}
