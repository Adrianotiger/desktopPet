using System;
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
            /// The main entry point for the application.
            /// </summary>
        [STAThread]
        static void Main()
        {
            int iMutexIndex = 0;

            Application.EnableVisualStyles();
            // if you like to wait a few seconds in case that the instance is just 
            // shutting down
            try
            {
                if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
                {
                    iMutexIndex = 1;
                    if (!mutex2.WaitOne(TimeSpan.FromSeconds(1), false))
                    {
                        MessageBox.Show("Application is already running! Only 2 instances are allowed.", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        return;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("Can't execute application: " + ex.Message);
                return;
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
