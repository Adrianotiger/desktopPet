using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DesktopPet
{
    class Program
    {
        public static LocalData.LocalData MyData;

        public class MainThread
        {
            public struct ErrorMsg
            {
                public string AudioErrorMessage;
            };
            public ErrorMsg ErrorMessages;
        };

        public static MainThread Mainthread = new MainThread();

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            try
            {
                MyData = new LocalData.LocalData();
            }
            catch(Exception ex)
            {
                int k = 0;
            }

            Application.Run(new Form1());
        }
    }

    class StartUp
    {
        public enum DEBUG_TYPE
        {
            /// <summary>
            /// Only info, to show what is happening.
            /// </summary>
            info = 1,
            /// <summary>
            /// Something important happened or something that was not expected.
            /// </summary>
            warning = 2,
            /// <summary>
            /// An error is occurred. The application need to do something that was not expected.
            /// </summary>
            error = 3,
        }

        public static void AddDebugInfo(DEBUG_TYPE type, string text)
        {

        }
    }

    class Properties
    {
        public struct Rsc
        {
            public string animations;
        };
        public static Rsc Resources;
    }
}
