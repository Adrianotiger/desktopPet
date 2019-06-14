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
        public enum LOG_TYPE
        {
            MESSAGE = 1,
            WARNING = 2,
            ERROR = 3
        };

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

        public static void AddLog(string text, string action, LOG_TYPE type = LOG_TYPE.MESSAGE, Control emitter = null)
        {
            if(LogForm != null)
            {
                if (type == LOG_TYPE.ERROR) LogForm.AddErrorLog(text, action, emitter);
                else if (type == LOG_TYPE.WARNING) LogForm.AddWarningLog(text, action, emitter);
                else LogForm.AddLog(text, action);
            }
        }
    }
}
