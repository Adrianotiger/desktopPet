using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PetEditor
{
    static class Program
    {
        public static XmlData.RootNode AnimationXML { get; set; }
        public static Log LogForm { get; set; }

        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            AnimationXML = new XmlData.RootNode();
            LogForm = null;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new MainWindow());
        }

        public static void AddLog(string text, string action, bool isError = false, bool isWarning = false)
        {
            if(LogForm != null)
            {
                if (isError) LogForm.AddErrorLog(text, action);
                else if (isWarning) LogForm.AddWarningLog(text, action);
                else LogForm.AddLog(text, action);
            }
        }
    }
}
