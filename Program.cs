using System;
using System.Threading;
using System.Windows.Forms;

namespace desktopPet
{
    static class Program
    {
        // Mutex can be made static so that GC doesn't recycle
        // same effect with GC.KeepAlive(mutex) at the end of main
        static Mutex mutex = new Mutex(false, "eSheep_Running");

        public static StartUp Mainthread;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            // if you like to wait a few seconds in case that the instance is just 
            // shutting down
            if (!mutex.WaitOne(TimeSpan.FromSeconds(1), false))
            {
                MessageBox.Show("eSheep is already running!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

            mutex.ReleaseMutex();
        }
    }
}
