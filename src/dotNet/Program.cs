using System;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;

namespace DesktopPet
{
        /// <summary>
        /// Main for the application. Once the application is started, this class will create all objects.
        /// </summary>
    static class Program
    {
            /// <summary>
            /// Mutual Exclusion, to allow only 1 instance of this application.
            /// </summary>
            /// <remarks>
            /// Mutex can be made static so that GC doesn't recycle same effect with GC.KeepAlive(mutex) at the end of main
            /// </remarks>
        static Mutex mutex = new Mutex(false, "eSheep_Running");

        /// <summary>
        /// Second Mutual Exclusion, to allow 2 instances of this application.
        /// </summary>
        static Mutex mutex2 = new Mutex(false, "eSheep_Running2");

        /// <summary>
        /// StartUp is the main program.
        /// </summary>
        public static StartUp Mainthread;

        /// <summary>
        /// Argument: load local animation XML.
        /// </summary>
        public static string ArgumentLocalXML = "";

        /// <summary>
        /// Argument: load animation XML from web.
        /// </summary>
        public static string ArgumentWebXML = "";
        
        /// <summary>
        /// Argument: open the installer when application starts.
        /// </summary>
        public static string ArgumentInstall = "";

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            int iMutexIndex = 0;

            string resource1 = "DesktopPet.dll.NAudio.dll";
            EmbeddedAssembly.Load(resource1, "NAudio.dll");
            
            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            Application.EnableVisualStyles();
            // if you like to wait a few seconds in case that the instance is just 
            // shutting down
            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    iMutexIndex = 1;
                    try
                    {
                        if (!mutex2.WaitOne(TimeSpan.FromSeconds(1), false))
                        {
                            MessageBox.Show("Application is already running! Only 2 instances are allowed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            return;
                        }
                    }
                    catch(Exception)
                    {

                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Can't execute application: " + ex.Message);
                return;
            }
            
                // Check and parse the arguments
            string SearchStringLocalXml = "localxml=";
            string SearchStringWebXml = "webxml=";
            string SearchStringInstall = "install=";
            foreach (string s in args)
            {
                if(s.IndexOf(SearchStringLocalXml) >= 0)
                {
                    ArgumentLocalXML = s.Substring(s.IndexOf(SearchStringLocalXml) + SearchStringLocalXml.Length);
                    if(ArgumentLocalXML.IndexOf(" ") >= 0)
                    {
                        ArgumentLocalXML = ArgumentLocalXML.Substring(0, ArgumentLocalXML.IndexOf(" "));
                    }
                }
                else if (s.IndexOf(SearchStringWebXml) >= 0)
                {
                    ArgumentWebXML = s.Substring(s.IndexOf(SearchStringWebXml) + SearchStringWebXml.Length);
                    if (ArgumentWebXML.IndexOf(" ") >= 0)
                    {
                        ArgumentWebXML = ArgumentWebXML.Substring(0, ArgumentWebXML.IndexOf(" "));
                    }
                }
                else if (s.IndexOf(SearchStringInstall) >= 0)
                {
                    ArgumentInstall = s.Substring(s.IndexOf(SearchStringInstall) + SearchStringInstall.Length);
                    if (ArgumentInstall.IndexOf(" ") >= 0)
                    {
                        ArgumentInstall = ArgumentInstall.Substring(0, ArgumentInstall.IndexOf(" "));
                    }
                }
            }

            Application.SetCompatibleTextRenderingDefault(false);

            // Show the system tray icon.					
            using (ProcessIcon pi = new ProcessIcon())
            {
                pi.Display();

                Mainthread = new StartUp(pi);
                
                // Make sure the application runs!
                Application.Run();
            }

            if (iMutexIndex == 0)
                mutex.ReleaseMutex();
            else if (iMutexIndex == 1)
                mutex2.ReleaseMutex();
        }
    }
}
