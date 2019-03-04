using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
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
            checkBox1.Checked = false;
            checkBox2.Checked = false;
            checkBox3.Checked = false;
            label2.Text = "-";
            label3.Text = "-";
            label4.Text = "-";
            checkBox1.Tag = 0;
            checkBox2.Tag = 0;
            checkBox3.Tag = 0;
            textBox1.Visible = false;
            timer1.Enabled = false;
            XmlClass = null;
            XmlAni = null;
            XmlNode = null;
            XmlContent = "";

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

            if (checkBox1.CheckState == CheckState.Checked)
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

                MemoryStream imageStream = null;
                imageStream = new MemoryStream(Convert.FromBase64String(XmlNode.Image.Png));

                var image = new Bitmap(imageStream);
                // no longer need stream
                imageStream.Close();
                XmlClass.spriteWidth = image.Width / XmlNode.Image.TilesX;
                XmlClass.spriteHeight = image.Height / XmlNode.Image.TilesY;
                XmlClass.sprites = BuildSprites(image, XmlClass.spriteWidth, XmlClass.spriteHeight);
                image.Dispose();

                XmlClass.bitmapIcon = new MemoryStream(Convert.FromBase64String(XmlNode.Header.Icon));

                checkBox2.CheckState = CheckState.Checked;
                checkBox2.Tag = 2;
                label3.Text = "XML IS VALID";
            }
            catch (Exception ex)
            {
                label3.Text = "FAILED: " + ex.Message;
            }

            AnalyseXMLError();

            if (checkBox2.CheckState == CheckState.Checked)
            {
                await AnalyseAnimations();
            }
        }

        private IList<Bitmap> BuildSprites(Bitmap spriteSheet, int width, int height)
        {
            var sprites = new List<Bitmap>();

            for (var yOffset = 0; yOffset < spriteSheet.Height; yOffset += height)
            {
                for (var xOffset = 0; xOffset < spriteSheet.Width; xOffset += width)
                {
                    var bmpImage = new Bitmap(width, height, spriteSheet.PixelFormat);
                    var destRectangle = new Rectangle(0, 0, width, height);
                    using (var graphics = Graphics.FromImage(bmpImage))
                    {
                        var sourceRectangle = new Rectangle(xOffset, yOffset, width, height);
                        graphics.DrawImage(spriteSheet, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    sprites.Add(bmpImage);
                }
            }
            return sprites;
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

        private void button1_Click(object sender, EventArgs e)
        {
            WebClient wc = new WebClient();
            var xmlString = "";
            try
            {
                xmlString = wc.DownloadString(textBox2.Text);
            }
            catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message);
            }

            if(xmlString != "")
            {
                XmlFileName = Path.GetTempFileName();
                using (var sw = File.CreateText(XmlFileName))
                {
                    sw.Write(xmlString);
                    sw.Flush();
                }
                tableLayoutPanel1.Visible = true;
                OpenXMLFile();
            }
        }

        private async Task<bool> AnalyseAnimations()
        {
            checkBox3.CheckState = CheckState.Indeterminate;
            checkBox3.Tag = 1;
            var errors = 0;
            var warnings = 0;
            var totLinks = 0;
            List<int> aniId = new List<int>(32);
            List<int> aniExecution = new List<int>(32);
            List<int> spawnId = new List<int>(16);

            if (XmlNode.Spawns.Spawn.Length < 1)
            {
                textBox1.Text += "SPAWN ERROR: The animation need at least 1 spawn.\r\n";
                errors++;
            }
            else if (XmlNode.Animations.Animation.Length < 3)
            {
                textBox1.Text += "ANIMATION ERROR: The animation need at least 3 animations.\r\n";
                errors++;
            }
            else
            {
                var fall = false;
                var drag = false;
                var kill = false;
                var sync = false;

                pictureBox1.Width = XmlClass.spriteWidth;
                pictureBox1.Height = XmlClass.spriteHeight;
                //pictureBox1.Top = Height - tableLayoutPanel1.Top;
                //pictureBox1.Left = Width - tableLayoutPanel1.Left * 2;
                timer1.Tag = 0;
                timer1.Enabled = true;
                timer1.Start();

                pictureBox2.Image = new Bitmap(XmlClass.bitmapIcon);

                if(pictureBox2.Image.Width != 48 || pictureBox2.Image.Height != 48)
                {
                    textBox1.Text += "ICON ERROR: Size must be 48x48 (not " + pictureBox2.Image.Width + "x" + pictureBox2.Image.Height + ").\r\n";
                    errors++;
                }

                foreach (var s in XmlNode.Spawns.Spawn)
                {
                    if (spawnId.Contains(s.Id))
                    {
                        textBox1.Text += "SPAWN ERROR: The spawn ID " + s.Id + " is present twice.\r\n";
                        errors++;
                    }
                    else
                    {
                        spawnId.Add(s.Id);
                    }
                }

                foreach (var a in XmlNode.Animations.Animation)
                {
                    if (a.Name == "fall") fall = true;
                    if (a.Name == "drag") drag = true;
                    if (a.Name == "kill") kill = true;
                    if (a.Name == "sync") sync = true;

                    if (a.Border != null && a.Border.Next != null) totLinks += a.Border.Next.Length;
                    if (a.Gravity != null && a.Gravity.Next != null) totLinks += a.Gravity.Next.Length;
                    if (a.Sequence != null && a.Sequence.Next != null) totLinks += a.Sequence.Next.Length;

                    if (aniId.Contains(a.Id))
                    {
                        textBox1.Text += "ANIMATION ERROR: The animation ID " + a.Id + " is present twice.\r\n";
                        errors++;
                    }
                    else
                    {
                        aniId.Add(a.Id);
                    }
                }
                if(!fall)
                {
                    textBox1.Text += "ANIMATION WARNING: Please add an animation with the name 'fall' for a falling pet.\r\n";
                    warnings++;
                }
                if (!drag)
                {
                    textBox1.Text += "ANIMATION ERROR: Please add an animation with the name 'drag' for a pet that is taken with a mouse.\r\n";
                    errors++;
                }
                if (!kill)
                {
                    textBox1.Text += "ANIMATION ERROR: Please add an animation with the name 'kill' for a pet that will be removed.\r\n";
                    errors++;
                }
                if (!sync)
                {
                    textBox1.Text += "ANIMATION WARNING: Please add an animation with the name 'sync' for syncing the pets.\r\n";
                    warnings++;
                }
            }

            if(errors == 0)
            {
                int spawns = 0;
                int childs = 0;
                int animations = 0;
                int links = 0;
                string errorMessage = "";
                
                try
                {
                    errorMessage = "Loading Xml animations";
                    // check all animations
                    XmlClass.AnimationXML = XmlNode;
                    XmlClass.LoadAnimations(XmlAni);

                    aniExecution.Add(XmlAni.AnimationDrag);
                    aniExecution.Add(XmlAni.AnimationFall);
                    aniExecution.Add(XmlAni.AnimationKill);
                    aniExecution.Add(XmlAni.AnimationSync);

                    // check spawns
                    foreach (var s in XmlNode.Spawns.Spawn)
                    {
                        errorMessage = "spawn " + s.Id;
                        XmlClass.GetXMLCompute(s.X, "spawn x");
                        XmlClass.GetXMLCompute(s.Y, "spawn y");
                        if(!spawnId.Contains(s.Id))
                        {
                            textBox1.Text += "SPAWN ERROR: On spawn " + s.Id + ": animation Id is not available.\r\n";
                            errors++;
                        }
                        if (s.Probability > 0)
                            aniExecution.Add(s.Next.Value);
                        spawns++;
                    }

                    // check childs
                    if (XmlNode.Childs.Child != null)
                    {
                        foreach (var c in XmlNode.Childs.Child)
                        {
                            errorMessage = "child " + c.Id;
                            XmlClass.GetXMLCompute(c.X, "spawn x");
                            XmlClass.GetXMLCompute(c.Y, "spawn y");
                            if (!aniId.Contains(c.Id))
                            {
                                textBox1.Text += "CHILD ERROR: On child " + c.Id + ": parent animation Id is not available.\r\n";
                                errors++;
                            }
                            if(!aniId.Contains(c.Next))
                            {
                                textBox1.Text += "CHILD ERROR: On child " + c.Id + ": next animation Id is not available.\r\n";
                                errors++;
                            }
                            childs++;
                        }
                    }

                    // check animations
                    foreach (var a in XmlNode.Animations.Animation)
                    {
                        errorMessage = "animation " + a.Id + " - " + a.Name;
                        XmlClass.GetXMLCompute(a.Start.X, "start x");
                        XmlClass.GetXMLCompute(a.Start.Y, "start y");
                        XmlClass.GetXMLCompute(a.End.X, "end x");
                        XmlClass.GetXMLCompute(a.End.Y, "end y");
                        if (!aniId.Contains(a.Id))
                        {
                            textBox1.Text += "ANIMATION ERROR: On animation " + a.Id + ": animation Id is not available.\r\n";
                            errors++;
                        }
                        if (a.Border != null)
                        {
                            foreach (var an in a.Border.Next)
                            {
                                if (!aniId.Contains(an.Value))
                                {
                                    textBox1.Text += "ANIMATION ERROR: On animation " + a.Id + ": border Next Id " + an.Value + " is not available.\r\n";
                                    errors++;
                                }
                            }
                        }
                        if (a.Gravity != null)
                        {
                            foreach (var an in a.Gravity.Next)
                            {
                                if (!aniId.Contains(an.Value))
                                {
                                    textBox1.Text += "ANIMATION ERROR: On animation " + a.Id + ": gravity Next Id " + an.Value + " is not available.\r\n";
                                    errors++;
                                }
                            }
                        }
                        if (a.Sequence != null)
                        {
                            if (a.Sequence.Next == null)
                            {
                                if(a.Name != "kill")
                                {
                                    textBox1.Text += "ANIMATION WARNING: On animation " + a.Id + ": this sequence does not have a next node, pet will respawn after this sequence.\r\n";
                                    warnings++;
                                }
                            }
                            else
                            {
                                foreach (var an in a.Sequence.Next)
                                {
                                    if (!aniId.Contains(an.Value))
                                    {
                                        textBox1.Text += "ANIMATION ERROR: On animation " + a.Id + ": sequence Next Id " + an.Value + " is not available.\r\n";
                                        errors++;
                                    }
                                }
                            }
                        }

                        animations++;
                    }

                    // check links
                    for(var k=0;k<100;k++)
                    {
                        List<int> aniAdds = new List<int>();
                        foreach (var aId in aniExecution)
                        {
                            errorMessage = "check links, pass " + k + " id " + aId;
                            XmlData.AnimationNode ani = null;
                            foreach(var xa in XmlNode.Animations.Animation)
                            {
                                if(xa.Id == aId)
                                {
                                    ani = xa;
                                    break;
                                }
                            }
                            if(ani == null)
                            {
                                continue;
                            }

                            if (ani.Gravity != null)
                            {
                                foreach (var an in ani.Gravity.Next)
                                {
                                    if (!aniExecution.Contains(an.Value) && !aniAdds.Contains(an.Value))
                                        aniAdds.Add(an.Value);
                                }
                            }
                            if (ani.Border != null)
                            {
                                foreach (var an in ani.Border.Next)
                                {
                                    if (!aniExecution.Contains(an.Value) && !aniAdds.Contains(an.Value))
                                        aniAdds.Add(an.Value);
                                }
                            }
                            if (ani.Sequence != null && ani.Sequence.Next != null)
                            {
                                foreach (var an in ani.Sequence.Next)
                                {
                                    if (!aniExecution.Contains(an.Value) && !aniAdds.Contains(an.Value))
                                        aniAdds.Add(an.Value);
                                }
                            }
                        }

                        aniExecution.AddRange(aniAdds);

                        if(aniExecution.Count == aniId.Count)
                        {
                            break;
                        }
                    }

                    if (aniExecution.Count != aniId.Count)
                    {
                        foreach(var ai in aniId)
                        {
                            if(!aniExecution.Contains(ai))
                            {
                                textBox1.Text += "ANIMATION WARNING: On animation " + ai + ": This ID is never played.\r\n";
                                warnings++;
                            }
                        }
                    }

                    await UpdateAnimationsState(spawns, XmlNode.Spawns.Spawn.Length,
                                                    animations, XmlNode.Animations.Animation.Length,
                                                    childs, XmlNode.Childs != null && XmlNode.Childs.Child != null ? XmlNode.Childs.Child.Length : 0,
                                                    links, totLinks);
                }
                catch(Exception ex)
                {
                    textBox1.Text += "ERROR: " + errorMessage + "\r\n" + ex.Message;
                }
            }

            checkBox3.CheckState = CheckState.Checked;
            checkBox3.Tag = 2;
            //label4.Text = "COMPLETED";

            textBox1.Text += "Errors: " + errors + ", Warnings: " + warnings + "\r\n";

            if(errors == 0)
            {

            }

            return errors == 0;
        }

        private async Task UpdateAnimationsState(int checkSpawn, int totSpawn, int checkAnimation, int totAnimation, int checkChild, int totChild, int checkLinks, int totLinks)
        {
            label4.Invoke(new Action(() => {
                label4.Text = "Spawns: " + checkSpawn + " / " + totSpawn + " (" + (checkSpawn * 100 / totSpawn).ToString() + "%) \r\n";
                if (totChild > 0)
                    label4.Text += "Chils: " + checkChild + " / " + totChild + " (" + (checkChild * 100 / totChild).ToString() + "%) \r\n";
                label4.Text += "Animations: " + checkAnimation + " / " + totAnimation + " (" + (checkAnimation * 100 / totAnimation).ToString() + "%) \r\n";
                label4.Text += "Animations links: " + totLinks + " \r\n";
            }));
            //await Task.Delay(100);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            int tag = (int)timer1.Tag;
            tag = (tag + 1) % XmlClass.sprites.Count;
            timer1.Tag = tag;

            pictureBox1.Image = XmlClass.sprites[tag];
        }
    }
}
