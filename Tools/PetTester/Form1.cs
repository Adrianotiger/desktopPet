using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace DesktopPet
{
    public partial class Form1 : Form
    {
        string XmlFileName;
        string XmlContent = "";
        Xml XmlClass;
        Animations XmlAni;
        XmlData.RootNode XmlNode;

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

        private void Form1_DragDrop(object sender, DragEventArgs e)
        {
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            if(files.Length != 1)
            { 
                MessageBox.Show("Please insert only 1 file.");
            }
            else if(files[0].Substring(files[0].LastIndexOf("\\")) != "\\animations.xml") 
            { 
                MessageBox.Show("The animation must be inside a file called 'animations.xml', not '" + files[0] + "'.");
            }
            else
            {
                XmlFileName = files[0];
                tableLayoutPanel1.Visible = true;
                OpenXMLFile();
            }
        }

        private void checkBox1_Click(object sender, EventArgs e)
        {
            switch((sender as CheckBox).Tag)
            {
                case 0:  (sender as CheckBox).CheckState = CheckState.Unchecked; break;
                case 1: (sender as CheckBox).CheckState = CheckState.Indeterminate; break;
                case 2: (sender as CheckBox).CheckState = CheckState.Checked; break;
            }
            (sender as CheckBox).Checked = !(sender as CheckBox).Checked;
        }

        private async void OpenXMLFile()
        {
            checkBox1.CheckState = CheckState.Indeterminate;
            checkBox1.Tag = 1;

            int bytesRead = 0;
            byte[] buffer = new byte[1024 * 64];
            try
            {
                using (var fs = File.OpenRead(XmlFileName))
                {
                    do
                    {
                        bytesRead = await fs.ReadAsync(buffer, 0, 1024 * 64);
                        XmlContent += Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    } while (bytesRead > 0);
                    fs.Close();
                }

                checkBox1.CheckState = CheckState.Checked;
                checkBox1.Tag = 2;
                label2.Text = "SUCCESS";
            }
            catch(Exception ex)
            {
                label2.Text = "FAILED: " + ex.Message;
            }

            if(checkBox1.CheckState == CheckState.Checked)
            {
                AnalyseXMLFile();
            }
        }

        public async void AnalyseXMLFile()
        {
            checkBox2.CheckState = CheckState.Indeterminate;
            checkBox2.Tag = 1;

            XmlClass = new Xml();
            XmlAni = new Animations(XmlClass);

            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(XmlData.RootNode));
                using (MemoryStream stream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        await writer.WriteAsync(XmlContent);
                        writer.Flush();

                        stream.Position = 0;
                        XmlNode = (XmlData.RootNode)mySerializer.Deserialize(stream);
                    }
                }

                checkBox2.CheckState = CheckState.Checked;
                checkBox2.Tag = 2;
                label3.Text = "SUCCESS";
            }
            catch (Exception ex)
            {
                label3.Text = "FAILED: " + ex.Message;
            }

            AnalyseXMLError();

            if (checkBox2.CheckState == CheckState.Checked)
            {
                
            }
        }

        public async void AnalyseXMLError()
        {
            textBox1.Visible = true;
            textBox1.Text = "";
            int iErrQty = 0;

            XmlReaderSettings settings = new XmlReaderSettings();
            settings.ValidationFlags = System.Xml.Schema.XmlSchemaValidationFlags.ProcessIdentityConstraints | System.Xml.Schema.XmlSchemaValidationFlags.ProcessInlineSchema | System.Xml.Schema.XmlSchemaValidationFlags.ProcessSchemaLocation | System.Xml.Schema.XmlSchemaValidationFlags.ReportValidationWarnings;
            settings.ValidationType = ValidationType.Schema;
            settings.Async = true;
            settings.ValidationEventHandler += (s, e) =>
            {
                XmlReader s2 = s as XmlReader;

                if (iErrQty++ > 5) return;
                if (s2 != null)
                {
                    textBox1.Text += " - Error on: " + s2.Name + "\r\n";
                }
                else
                {
                    textBox1.Text += " - Error on: " + s.ToString() + "\r\n";
                }
                textBox1.Text += "    -> Exception: \r\n";
                textBox1.Text += "             -> Line: " + e.Exception.LineNumber + "\r\n";
                textBox1.Text += "             -> Position: " + e.Exception.LinePosition + "\r\n";
                textBox1.Text += "    -> Severity: " + e.Severity.ToString() + "\r\n";
                textBox1.Text += "    -> Message: " + e.Message.ToString() + "\r\n";
                textBox1.Text += "------------------------------------------\r\n";
            };

            StreamReader xmlStream = new StreamReader(XmlFileName);

            using (XmlReader reader = XmlReader.Create(xmlStream, settings))
            {
                while (await reader.ReadAsync())
                {
                    
                }
            }

        }
    }
}
