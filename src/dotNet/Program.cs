using System;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace DesktopPet
{
    /// <summary>
    /// Main for the application. Once the application is started, this class will create all objects.
    /// </summary>
    class Program
    {
        /// <summary>
        /// StartUp is the main program.
        /// </summary>
        public static StartUp Mainthread;

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return EmbeddedAssembly.Get(args.Name);
        }

        /// <summary>
        /// Mutual Exclusion, to allow only 1 instance of this application.
        /// </summary>
        /// <remarks>
        /// Mutex can be made static so that GC doesn't recycle same effect with GC.KeepAlive(mutex) at the end of main
        /// </remarks>
        static readonly Mutex mutex = new Mutex(false, "eSheep_Running");

        /// <summary>
        /// Second Mutual Exclusion, to allow 2 instances of this application.
        /// </summary>
        static readonly Mutex mutex2 = new Mutex(false, "eSheep_Running2");

		public static LocalData MyData = new LocalData();

		/// <summary>
		/// Open the option dialog, to show some options like reset XML animation or load animation from the webpage.
		/// </summary>
		public static void OpenOptionDialog()
		{
			FormOptions2 formoptions2 = new FormOptions2("Pet List", FormOptions2.WindowType.Pets);
			formoptions2.ShowDialog();
		}

#if PORTABLE
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

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            int iMutexIndex = 0;

            //Application.EnableVisualStyles(); << Disable Visual Styles, so it gives the Windows95 look
            Application.SetCompatibleTextRenderingDefault(false);

            EmbeddedAssembly.Load("DesktopPet.Portable.NAudio.dll", "NAudio.dll");
            EmbeddedAssembly.Load("DesktopPet.Portable.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

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
                    catch (Exception)
                    {

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Can't execute application: " + ex.Message);
                return;
            }

            // Check and parse the arguments
            string SearchStringLocalXml = "localxml=";
            string SearchStringWebXml = "webxml=";
            string SearchStringInstall = "install=";
            bool loadExternalXml = false;
			foreach (string s in args)
            {
                if (s.IndexOf(SearchStringLocalXml) >= 0)
                {
                    ArgumentLocalXML = s.Substring(s.IndexOf(SearchStringLocalXml) + SearchStringLocalXml.Length);
                    if (ArgumentLocalXML.IndexOf(" ") >= 0)
                    {
						ArgumentLocalXML = ArgumentLocalXML.Substring(0, ArgumentLocalXML.IndexOf(" "));
                    }
                    loadExternalXml = true;
				}
                else if (s.IndexOf(SearchStringWebXml) >= 0)
                {
                    ArgumentWebXML = s.Substring(s.IndexOf(SearchStringWebXml) + SearchStringWebXml.Length);
                    if (ArgumentWebXML.IndexOf(" ") >= 0)
                    {
                        ArgumentWebXML = ArgumentWebXML.Substring(0, ArgumentWebXML.IndexOf(" "));
                    }
					loadExternalXml = true;
				}
                else if (s.IndexOf(SearchStringInstall) >= 0)
                {
                    ArgumentInstall = s.Substring(s.IndexOf(SearchStringInstall) + SearchStringInstall.Length);
                    if (ArgumentInstall.IndexOf(" ") >= 0)
                    {
                        ArgumentInstall = ArgumentInstall.Substring(0, ArgumentInstall.IndexOf(" "));
                    }
					loadExternalXml = true;
				}
            }

            if (loadExternalXml)
            {
				MyData.SetXml(MyData.LoadXML(), "");
			}

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

#else


		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
        static void Main(string[] args)
        {
			int iMutexIndex = 0;

			//Application.EnableVisualStyles(); << Disable Visual Styles, so it gives the Windows95 look
			Application.SetCompatibleTextRenderingDefault(false);

			EmbeddedAssembly.Load("DesktopPet.Portable.NAudio.dll", "NAudio.dll");
			EmbeddedAssembly.Load("DesktopPet.Portable.Newtonsoft.Json.dll", "Newtonsoft.Json.dll");

			AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

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
					catch (Exception)
					{

					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Can't execute application: " + ex.Message);
				return;
			}

			//Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(true);

            AppDomain.CurrentDomain.AssemblyResolve += new ResolveEventHandler(CurrentDomain_AssemblyResolve);

            //MyData = new LocalData.LocalData(Windows.Storage.ApplicationData.Current.LocalFolder.Path, Application.ExecutablePath);

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
#endif

        /// <summary>
        /// Check if application is started from the installation path.
        /// </summary>
        /// <returns>true if the executed application is installed.</returns>
        public static bool IsApplicationInstalled()
        {
            string appPath = Application.StartupPath;
            string installPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "DesktopPet");
            return (string.Compare(appPath, installPath) == 0);
        }
    }
}
