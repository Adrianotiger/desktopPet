using Microsoft.Win32;
using NAudio.Wave;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;


namespace PetEditor
{
    public partial class Tools : Form
    {
        IEnumerable<int> lineIndexes = null;
        XmlReaderSettings xmlSettings = null;

        Animations petAnimations;
        
        protected static class PetValues
        {
            public static double OffsetY { get; set; } = 0;
            public static int AnimationStep { get; set; } = -1;
            public static TAnimation CurrentAnimation { get; set; }
            public static bool IsDragging { get; set; } = false;
            public static double Opacity { get; set; } = 1.0;
            public static double PositionX { get; set; } = 0;
            public static double PositionY { get; set; } = 0;
            public static bool IsMovingLeft { get; set; } = true;
            public static bool OverWindow { get; set; } = false;
            public static ImageList Images { get; set; }
        }

        public Tools()
        {
            InitializeComponent();
        }

        private void XmlViewer_Load(object sender, EventArgs e)
        {

        }

        public void SelectSection(string section)
        {
            switch (section)
            {
                case "view xml": tabControl1.SelectedIndex = 0; break;
                case "compile":  tabControl1.SelectedIndex = 1; break;
                case "test/run": tabControl1.SelectedIndex = 2; break;
                case "graphviz":
                    {
                        tabControl1.SelectedIndex = 3;
                        webViewCompatible1.NavigateToString("<h2>LOADING...</h2>");
                        webViewCompatible1.Update();
                        Update();
                        GenerateViz(0.5, false);
                    }
                    break;
            }

        }

        public void LoadXML()
        {
            XmlDocument xmldoc = new XmlDocument();
            var xmlString = XmlTools.GenerateXmlString();
            xmldoc.LoadXml(xmlString);
            XmlNode xmlnode;
            xmlnode = xmldoc.ChildNodes[1];
            treeView1.Nodes.Clear();
            var rootNode = xmldoc.DocumentElement.Name;
            //treeView1.Nodes.Add("<" + rootNode + ">");
            TreeNode tNode = new TreeNode("<" + rootNode + ">");
            //tNode = treeView1.Nodes[0];

            listView1.Items.Clear();
            listView1.Items.Add("Loading XML...").BackColor = Color.LightGray;

            ThreadStart t = new ThreadStart(() =>
            {
                AddNode(xmlnode, tNode);

                listView1.Invoke(new MethodInvoker(() =>
                {
                    treeView1.Nodes.Add(tNode);
                    treeView1.Nodes[0].Expand();

                    listView1.Items[0].Text = "LOADING XSD...";
                }));

                if (xmlSettings == null)
                {
                    xmlSettings = new XmlReaderSettings();
                    xmlSettings.Schemas.Add("https://esheep.petrucci.ch/", "https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Resources/animations.xsd");
                    xmlSettings.ValidationFlags =
                        XmlSchemaValidationFlags.ProcessIdentityConstraints |
                        XmlSchemaValidationFlags.ProcessInlineSchema |
                        XmlSchemaValidationFlags.ProcessSchemaLocation |
                        XmlSchemaValidationFlags.AllowXmlAttributes |
                        XmlSchemaValidationFlags.ReportValidationWarnings;
                    xmlSettings.ValidationType = ValidationType.Schema;
                    xmlSettings.IgnoreComments = true;
                    xmlSettings.IgnoreWhitespace = true;
                    xmlSettings.CloseInput = true;

                    xmlSettings.ValidationEventHandler += new ValidationEventHandler(Settings_ValidationEventHandler);
                }
                
                StringReader ms = new StringReader(xmlString);
                using (XmlReader reader = XmlReader.Create(ms, xmlSettings))
                {
                    listView1.Invoke(new MethodInvoker(() =>
                    {
                        Regex rx = new Regex(@"\[CDATA\[([A-Za-z0-9\/+=]*)\]\]", RegexOptions.Multiline);
                        Regex rx2 = new Regex(@"\<base64\>([A-Za-z0-9\/+=]*)\</base64\>", RegexOptions.Multiline);
                        xmlString = rx.Replace(xmlString, "[CDATA[... base64 image ...]]");
                        richTextBox1.Text = rx2.Replace(xmlString, "<base64>... base64 mp3 ... </base64>");

                        lineIndexes = Regex.Matches(xmlString, "\n").Cast<Match>().Select(m => m.Index);

                        listView1.Items.Clear();
                    }));
                    while (reader.Read()) ;
                    reader.Close();
                    listView1.Invoke(new MethodInvoker(() =>
                    {
                        if (listView1.Items.Count == 0)
                        {
                            listView1.Items.Add("NO ERRORS").BackColor = Color.LightGreen;
                        }
                    }));
                }
            });
            Thread childThread = new Thread(t);
            childThread.Start();
        }

        private void Settings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            listView1.Invoke(new MethodInvoker(() =>
            {
                var lv = listView1.Items.Add(e.Severity.ToString());
                lv.SubItems.Add((sender as XmlReader).Name);
                lv.SubItems.Add(e.Exception.LineNumber.ToString());
                lv.SubItems.Add(e.Exception.LinePosition.ToString());
                lv.SubItems.Add(e.Exception.Message.ToString());
                lv.Tag = (sender as XmlReader);
                if(e.Severity == XmlSeverityType.Error)
                {
                    lv.BackColor = Color.LightPink;
                }
                else
                {
                    lv.BackColor = Color.LightSalmon;
                }
                lv.ToolTipText = e.Exception.Message;
            }));
        }

        private void AddNode(XmlNode inXmlNode, TreeNode inTreeNode)
        {
            XmlNode xNode;
            TreeNode tNode;
            XmlNodeList nodeList;
            int i = 0;
            if (inXmlNode.HasChildNodes)
            {
                nodeList = inXmlNode.ChildNodes;
                for (i = 0; i <= nodeList.Count - 1; i++)
                {
                    xNode = inXmlNode.ChildNodes[i];
                    var nodeName = "<" + xNode.Name;
                    if (xNode.Attributes != null)
                    {
                        for (var a = 0; a < xNode.Attributes.Count; a++)
                        {
                            nodeName += " " + xNode.Attributes[a].Name + "=\"" + xNode.Attributes[a].Value + "\"";
                        }
                    }
                    nodeName += ">";
                    if (!nodeList[i].HasChildNodes && inTreeNode.Nodes.Count == 0)
                    {
                        inTreeNode.Text += inXmlNode.InnerText + Regex.Replace(inTreeNode.Text.Replace("<", "</"), "[ ].+?(?=>)", "");
                    }
                    else
                    {
                        inTreeNode.Nodes.Add(nodeName);
                        tNode = inTreeNode.LastNode;
                        AddNode(xNode, tNode);
                    }
                }
            }
            else
            {
                if(inTreeNode.Text == null || inXmlNode.Name == "#text" || inTreeNode.Text == "" && inXmlNode.InnerText != "")
                    inTreeNode.Text = inXmlNode.InnerText.ToString();
            }
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                if (item.SubItems.Count > 2)
                {
                    if (int.TryParse(item.SubItems[2].Text, out int line))
                    {
                        richTextBox1.Focus();
                        richTextBox1.SelectionStart = lineIndexes.ElementAt(line - 2) + 1;
                        richTextBox1.SelectionLength = lineIndexes.ElementAt(line - 1) - lineIndexes.ElementAt(line - 2);
                        richTextBox1.Focus();
                    }
                }
            }
        }

        private void listView1_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            if (listView1.SelectedItems.Count > 0)
            {
                var item = listView1.SelectedItems[0];
                if (item.SubItems.Count > 3)
                {
                    listView1.Focus();
                    toolTip1.SetToolTip(listView1, item.SubItems[4].Text);
                    toolTip1.ShowAlways = true;
                }
            }
        }

        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void richTextBox1_DoubleClick(object sender, EventArgs e)
        {
            richTextBox1.SelectAll();
            richTextBox1.Copy();
        }

        private void tabPage2_Click(object sender, EventArgs e)
        {

        }

        private void tabControl1_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if(tabControl1.SelectedTab == tabPage2)
            {
                button2.Enabled = button3.Enabled = false;
                label1.Text = label4.Text = label6.Text = "PLEASE TEST...";
            }
            if (tabControl1.SelectedTab == tabPage3)
            {
                label8.Text = DateTime.Now.ToShortTimeString();
                label7.Text = DateTime.Now.ToShortDateString();

                var aniXml = new Xml();
                petAnimations = new Animations(aniXml);
                aniXml.AnimationXML = Program.AnimationXML;
                aniXml.LoadAnimations(petAnimations);

                PetValues.Images = XmlTools.AnimationImages;

                pictureBox6.Image = PetValues.Images.Images[0];
                spawnsToolStripMenuItem.DropDownItems.Clear();
                foreach (var s in Program.AnimationXML.Spawns.Spawn)
                {
                    var spawnItem = spawnsToolStripMenuItem.DropDownItems.Add("Spawn #" + s.Id + " -> " + XmlTools.GetXmlAnimationNode(s.Id).Name);
                    spawnItem.Click += (se, ev) =>
                    {
                        TSpawn spawn = petAnimations.SheepSpawn[s.Id];
                        PetValues.OffsetY = 0.0;
                        PetValues.Opacity = 1.0;
                        PetValues.PositionX = pictureBox6.Left = spawn.Start.X.GetValue(0);
                        PetValues.PositionY = pictureBox6.Top = spawn.Start.Y.GetValue(0);
                        pictureBox6.BackColor = Color.Transparent;
                        SetNewAnimationWithId(spawn.Next);                // Set next animation

                        timer1.Enabled = true;
                    };
                }
                contextMenuStrip1.Show(tabPage3, new Point(tabPage3.Width / 10, tabPage3.Height / 2));
                spawnsToolStripMenuItem.ShowDropDown();

                UpdateRunWindow();
            }
        }

        private void UpdateRunWindow()
        {
            WindowDimensions.Screen = new Point(tabPage3.Width, tabPage3.Height);
            WindowDimensions.Area = new Point(tabPage3.Width, tabPage3.Height - tableLayoutPanel1.Height);
            WindowDimensions.Image = new Point(pictureBox6.Image.Width, pictureBox6.Image.Height);

            label10.Text = "Screen Width: " + tabPage3.Width + "px";
            label11.Text = "Screen Height: " + tabPage3.Height + "px";
            label13.Text = "Working Area W.: " + (tabPage3.Width) + "px";
            label12.Text = "Working Area H.: " + (tabPage3.Height - tableLayoutPanel1.Height) + "px";
            label15.Text = "Pet Width: " + (pictureBox6.Image.Width) + "px";
            label14.Text = "Pet Height: " + (pictureBox6.Image.Height) + "px";
        }

        private void SetNewAnimationRequest(bool next, bool border, bool gravity, TNextAnimation.TOnly only)
        {
            // todo: if over window and there is no next, window will be removed
            nextToolStripMenuItem.DropDownItems.Clear();
            borderToolStripMenuItem.DropDownItems.Clear();
            gravityToolStripMenuItem.DropDownItems.Clear();
            if(next)
            {
                foreach(var ani in PetValues.CurrentAnimation.EndAnimation)
                {
                    var aniInfo = XmlTools.GetXmlAnimationNode(ani.ID);
                    var item = nextToolStripMenuItem.DropDownItems.Add(aniInfo.Id + " (" + aniInfo.Name + ")");
                    if(ani.only != TNextAnimation.TOnly.NONE && ani.only != only)
                    {
                        item.Enabled = false;
                    }
                    else
                    {
                        item.Click += (s, e) =>
                        {
                            SetNewAnimationWithId(aniInfo.Id);
                            timer1.Enabled = true;
                        };
                    }
                }
            }
            if (border)
            {
                foreach (var ani in PetValues.CurrentAnimation.EndBorder)
                {
                    var aniInfo = XmlTools.GetXmlAnimationNode(ani.ID);
                    var item = borderToolStripMenuItem.DropDownItems.Add(aniInfo.Id + " (" + aniInfo.Name + ")");
                    if (ani.only != TNextAnimation.TOnly.NONE && ani.only != only)
                    {
                        item.Enabled = false;
                    }
                    else
                    {
                        item.Click += (s, e) =>
                        {
                            SetNewAnimationWithId(aniInfo.Id);
                            timer1.Enabled = true;
                        };
                    }
                }
            }
            if (gravity)
            {
                foreach (var ani in PetValues.CurrentAnimation.EndGravity)
                {
                    var aniInfo = XmlTools.GetXmlAnimationNode(ani.ID);
                    var item = gravityToolStripMenuItem.DropDownItems.Add(aniInfo.Id + " (" + aniInfo.Name + ")");
                    if (ani.only != TNextAnimation.TOnly.NONE && ani.only != only)
                    {
                        item.Enabled = false;
                    }
                    else
                    {
                        item.Click += (s, e) =>
                        {
                            SetNewAnimationWithId(aniInfo.Id);
                            timer1.Enabled = true;
                        };
                    }
                }
            }
            contextMenuStrip2.Show(pictureBox6, new Point(pictureBox6.Width / 2, 0));
        }

        private void SetNewAnimationWithId(int id)
        {
            PetValues.AnimationStep = -1;
            PetValues.CurrentAnimation = petAnimations.GetAnimation(id);
            // Check if animation ID has a child. If so, the child will be created.
            if (petAnimations.HasAnimationChild(id))
            {
                Program.AddLog("Unable to create child at the moment (coming soon).", "CREATE CHILD", Program.LOG_TYPE.WARNING);
                /*
                // child creating childs... Maximum 5 sub-childs can be created
                if (Name.IndexOf("child") < 0 || int.Parse(Name.Substring(5)) < 5)
                {
                    foreach (TChild childInfo in Animations.GetAnimationChild(id))
                    {
                        FormPet child = new FormPet(Animations, Xml, new Point(Left, Top), !IsMovingLeft, DisplayIndex);
                        for (int i = 0; i < imageList1.Images.Count; i++)
                        {
                            child.addImage(imageList1.Images[i]);
                        }
                        // To detect if it is a child, the name of the form will be renamed.
                        if (Name.IndexOf("child") < 0) // first child
                        {
                            child.Name = "child1";
                        }
                        else if (Name.IndexOf("child") == 0) // second, fifth child
                        {
                            child.Name = "child" + (int.Parse(Name.Substring(5)) + 1).ToString();
                        }

                        child.Show(Width, Height);
                        child.PlayChild(id, childInfo);

                        childs.Add(child);
                    }
                }
                */
            }
            XmlTools.StatisticsDataInput vals = new XmlTools.StatisticsDataInput
            {
                Area = new Point(tabPage3.Width, tabPage3.Height - tableLayoutPanel1.Height),
                Screen = new Point(tabPage3.Width, tabPage3.Height),
                Image = new Rectangle(0, 0, pictureBox6.Width, pictureBox6.Height),
                Random = 0,
                SRandom = 0
            };
            var statsMin = XmlTools.GetAnimationStatistics(XmlTools.GetXmlAnimationNode(PetValues.CurrentAnimation.ID), vals, null);
            vals.Random = 100;
            vals.SRandom = 100;
            var statsMax = XmlTools.GetAnimationStatistics(XmlTools.GetXmlAnimationNode(PetValues.CurrentAnimation.ID), vals, null);

            label20.Text = "Id: " + id;
            label21.Text = "Name: " + PetValues.CurrentAnimation.Name;
            label22.Text = "Steps: " + PetValues.CurrentAnimation.Sequence.TotalSteps;
            if (statsMax.TotalFrames != statsMin.TotalFrames)
                label22.Text += " (" + statsMin.TotalFrames + " - " + statsMax.TotalFrames + ")";

            timer1.Interval = PetValues.CurrentAnimation.Start.Interval.GetValue(0);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            label1.Text = "Testing...";
            Update();
            LoadXML();
            Thread.Sleep(500);
            if(listView1.Items.Count == 1)
            {
                button2.Enabled = true;
                label1.Text = "SUCCESS!";
            }
            else
            {
                tabControl1.SelectedIndex = 0;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            label4.Text = "Check Info...";
            Update();
            listView2.Items.Clear();
            var header = Program.AnimationXML.Header;
            var image = Program.AnimationXML.Image;
            if (!int.TryParse(header.Application, out int appVer) || appVer > 1)
            {
                listView2.Items.Add("HEADER INFO").SubItems.Add("Wrong application version!");
            }
            if(header.Author.Contains("NAME") || header.Author.Contains("name") || header.Author.Length < 3)
            {
                listView2.Items.Add("HEADER INFO").SubItems.Add("Please add your name as author");
            }
            if(header.Petname.Length < 3)
            {
                listView2.Items.Add("HEADER INFO").SubItems.Add("Please add a pet name");
            }
            if (header.Title.Length < 3)
            {
                listView2.Items.Add("HEADER INFO").SubItems.Add("Please add a project name");
            }
            if(image.TilesX < 2)
            {
                listView2.Items.Add("IMAGE INFO").SubItems.Add("TilesX is not correct");
            }
            if (image.TilesY < 2)
            {
                listView2.Items.Add("IMAGE INFO").SubItems.Add("TilesY is not correct");
            }
            if(Program.AnimationXML.Spawns == null || Program.AnimationXML.Spawns.Spawn == null || Program.AnimationXML.Spawns.Spawn.Length == 0)
            {
                listView2.Items.Add("SPAWNS").SubItems.Add("You need at least 1 spawn in your xml");
            }
            if (Program.AnimationXML.Animations == null || Program.AnimationXML.Animations.Animation == null || Program.AnimationXML.Animations.Animation.Length < 3)
            {
                listView2.Items.Add("ANIMATIONS").SubItems.Add("You need at least 3 animations in your xml");
            }

            Thread.Sleep(400);
            label4.Text = "Check Images...";
            Update();
            Image im = null;
            try
            {
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(header.Icon));
                im = Image.FromStream(ms);
                if(im.Height != 48 || im.Width != 48)
                {
                    listView2.Items.Add("ICON").SubItems.Add("Icon size must be 48x48");
                }
                if(!ImageFormat.Icon.Equals(im.RawFormat))
                {
                    listView2.Items.Add("ICON").SubItems.Add("Icon is not a valid .ico (can't be a png or jpeg)");
                }
                
            }
            catch(Exception ex)
            {
                listView2.Items.Add("ICON").SubItems.Add("Unable to test icon:" + ex.Message);
            }
            try
            {
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(image.Png));
                im = Image.FromStream(ms);
                if (im.Width / image.TilesX < 10 || im.Height / image.TilesY < 10)
                {
                    listView2.Items.Add("IMAGE").SubItems.Add("A single tile should be at least 10x10 pixels!");
                }
                if (!ImageFormat.Png.Equals(im.RawFormat))
                {
                    listView2.Items.Add("IMAGE").SubItems.Add("Please use only PNG images");
                }

            }
            catch (Exception ex)
            {
                im = null;
                listView2.Items.Add("IMAGE").SubItems.Add("Unable to test image:" + ex.Message);
            }

            Thread.Sleep(400);

            if (listView2.Items.Count == 0)
            {
                button3.Enabled = true;
                label4.Text = "SUCCESS!";
            }
            else
            {
                label4.Text = "FAILED...";
            }

            if(im != null)
            {
                if((im.Width % image.TilesX) != 0)
                {
                    listView2.Items.Add("IMAGE").SubItems.Add("Warning: image width (" + im.Width + ") is not divisible for tilesX (" + image.TilesX + "), please change the size of your image to avoid wrong cutting.");
                }
                if ((im.Height % image.TilesY) != 0)
                {
                    listView2.Items.Add("IMAGE").SubItems.Add("Warning: image height (" + im.Height + ") is not divisible for tilesY (" + image.TilesY + "), please change the size of your image to avoid wrong cutting.");
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            label6.Text = "Collect Info...";
            Update();
            var header = Program.AnimationXML.Header;
            var image = Program.AnimationXML.Image;
            var spawns = Program.AnimationXML.Spawns.Spawn;
            var childs = Program.AnimationXML.Childs?.Child;
            var sounds = Program.AnimationXML.Sounds?.Sound;
            var animations = Program.AnimationXML.Animations.Animation;
            var totFrames = image.TilesX * image.TilesY;
            var spawnIds = new List<int>();
            var animationIds = new List<int>();
            var animationNames = new List<string>();
            var animationLinkTo = new Dictionary<int, int>();

            foreach (var ani in XmlTools.GetAnimationsId())
            {
                animationLinkTo.Add(ani, 0);
            }

            MemoryStream ms = new MemoryStream(Convert.FromBase64String(image.Png));
            var imagePng = Image.FromStream(ms);

            Thread.Sleep(300);
            label6.Text = "Check Spawns...";
            Update();
            XmlTools.StatisticsDataInput vals = new XmlTools.StatisticsDataInput
            {
                Area = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height),
                Screen = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                Image = new Rectangle(imagePng.Width, imagePng.Height, imagePng.Width, imagePng.Height),
                Random = 0,
                SRandom = 0
            };
            foreach (var spawn in spawns)
            {
                if(spawnIds.Contains(spawn.Id))
                {
                    listView2.Items.Add("SPAWN " + spawn.Id).SubItems.Add("This id is given twice, please remove one");
                }
                if(spawn.Probability <= 0)
                {
                    listView2.Items.Add("SPAWN " + spawn.Id).SubItems.Add("Probability is 0, this should never be set");
                }
                if(XmlTools.EvalValue(spawn.X, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("SPAWN " + spawn.Id).SubItems.Add("spawn X is not valid");
                }
                if(XmlTools.EvalValue(spawn.Y, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("SPAWN " + spawn.Id).SubItems.Add("spawn Y is not valid");
                }
                if (!XmlTools.GetAnimationsId().Contains(spawn.Next.Value))
                {
                    listView2.Items.Add("SPAWN " + spawn.Id).SubItems.Add("There is no animation with id " + spawn.Next.Value);
                }
                animationLinkTo[spawn.Next.Value]++;
                spawnIds.Add(spawn.Id);
            }
            Thread.Sleep(300);
            label6.Text = "Check Animations...";
            Update();
            foreach (var animation in animations)
            {
                if(animation.Name == "fall" || animation.Name == "drag" || animation.Name == "sync" || animation.Name == "kill")
                {
                    animationLinkTo[animation.Id]++;
                }
            }
            foreach (var animation in animations)
            {
                if (animationIds.Contains(animation.Id))
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("This id is given twice, please remove one");
                }
                if (animationNames.Contains(animation.Name))
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("The name \"" + animation.Name + "\" is given twice, please rename one");
                }
                if (animation.Border != null)
                {
                    foreach(var n in animation.Border.Next)
                    {
                        if(n.Probability == 0)
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Probability for Border-Next:" + n.Value + " is 0, this should never be set");
                        }
                        if(!XmlTools.GetAnimationsId().Contains(n.Value))
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("There is no animation Id (" + n.Value + ") for Border-Next. Please remove this node");
                        }
                    }
                }
                if (animation.Gravity != null)
                {
                    foreach (var n in animation.Gravity.Next)
                    {
                        if (n.Probability == 0)
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Probability for Gravity-Next:" + n.Value + " is 0, this should never be set");
                        }
                        if (!XmlTools.GetAnimationsId().Contains(n.Value))
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("There is no animation Id (" + n.Value + ") for Gravity-Next. Please remove this node");
                        }
                    }
                }
                if (XmlTools.EvalValue(animation.Start.Interval, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Start Interval is wrong");
                }
                if (XmlTools.EvalValue(animation.Start.X, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Start X is wrong");
                }
                if (XmlTools.EvalValue(animation.Start.Y, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Start Y is wrong");
                }
                if(animation.Start.Opacity < 0.0 || animation.Start.Opacity > 1.0)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Start Opacity should be between 0.0 (invisible) and 1.0 (no transparency)");
                }
                if (XmlTools.EvalValue(animation.End.Interval, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("End Interval is wrong");
                }
                if (XmlTools.EvalValue(animation.End.X, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("End X is wrong");
                }
                if (XmlTools.EvalValue(animation.End.Y, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("End Y is wrong");
                }
                if (animation.End.Opacity < 0.0 || animation.End.Opacity > 1.0)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("End Opacity should be between 0.0 (invisible) and 1.0 (no transparency)");
                }
                if (animation.Sequence.Next != null)
                {
                    foreach (var n in animation.Sequence.Next)
                    {
                        if (n.Probability == 0)
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Probability for Next:" + n.Value + " is 0, this should never be set");
                        }
                        if (!XmlTools.GetAnimationsId().Contains(n.Value))
                        {
                            listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("There is no animation Id (" + n.Value + ") for Next. Please remove this node");
                        }
                    }
                }
                if (XmlTools.EvalValue(animation.Sequence.RepeatCount, vals, null) == int.MinValue)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("Repeat count is wrong");
                }
                if(animation.Sequence.Frame.Length == 0)
                {
                    listView2.Items.Add("ANIMATION " + animation.Id).SubItems.Add("There is no frame inside this animation, this list can't be empty");
                }
                animationIds.Add(animation.Id);
                animationNames.Add(animation.Name);
            }
            if (!animationNames.Contains("fall"))
            {
                listView2.Items.Add("ANIMATION fall").SubItems.Add("There is no animation named 'fall', please add this key value to one of your animations");
            }
            if (!animationNames.Contains("drag"))
            {
                listView2.Items.Add("ANIMATION drag").SubItems.Add("There is no animation named 'drag', please add this key value to one of your animations");
            }
            if (!animationNames.Contains("kill"))
            {
                listView2.Items.Add("ANIMATION kill").SubItems.Add("There is no animation named 'kill', please add this key value to one of your animations");
            }
            if (!animationNames.Contains("sync"))
            {
                listView2.Items.Add("ANIMATION sync").SubItems.Add("There is no animation named 'sync', please add this key value to one of your animations");
            }
            for (var k = 0; k < animations.Length; k++)
            {
                foreach (var animation in animations)
                {
                    if (animationLinkTo[animation.Id] == 0) continue;
                    if (animation.Border != null)
                    {
                        foreach (var n in animation.Border.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                    if (animation.Gravity != null)
                    {
                        foreach (var n in animation.Gravity.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                    if (animation.Sequence.Next != null)
                    {
                        foreach (var n in animation.Sequence.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                }
            }
            foreach (var child in childs)
            {
                if(animationLinkTo[child.Id] > 0)
                {
                    animationLinkTo[child.Next]++;
                }
            }
            for (var k = 0; k < animations.Length; k++)
            {
                foreach (var animation in animations)
                {
                    if (animationLinkTo[animation.Id] == 0) continue;
                    if (animation.Border != null)
                    {
                        foreach (var n in animation.Border.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                    if (animation.Gravity != null)
                    {
                        foreach (var n in animation.Gravity.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                    if (animation.Sequence.Next != null)
                    {
                        foreach (var n in animation.Sequence.Next)
                        {
                            animationLinkTo[n.Value]++;
                        }
                    }
                }
            }
            foreach (var aniId in animationLinkTo)
            {
                if(aniId.Value == 0)
                {
                    listView2.Items.Add("ANIMATION " + aniId.Key).SubItems.Add("This animation is never played!");
                }
            }
            Thread.Sleep(300);
            label6.Text = "Check Childs...";
            Update();
            if (childs != null)
            {
                foreach (var child in childs)
                {
                    if (XmlTools.EvalValue(child.X, vals, null) == int.MinValue)
                    {
                        listView2.Items.Add("CHILD " + child.Id).SubItems.Add("child X is not valid");
                    }
                    if (XmlTools.EvalValue(child.Y, vals, null) == int.MinValue)
                    {
                        listView2.Items.Add("CHILD " + child.Id).SubItems.Add("child Y is not valid");
                    }
                    if (!XmlTools.GetAnimationsId().Contains(child.Next))
                    {
                        listView2.Items.Add("CHILD " + child.Id).SubItems.Add("There is no animation with id " + child.Next);
                    }
                }
            }
            Thread.Sleep(300);
            label6.Text = "Check Sounds...";
            Update();
            if (sounds != null)
            {
                foreach (var sound in sounds)
                {
                    try
                    {
                        ms = new MemoryStream(Convert.FromBase64String(sound.Base64));
                        var AudioReader = new Mp3FileReader(ms);
                        if(AudioReader.Length > 1024 * 200)
                        {
                            listView2.Items.Add("SOUND " + sound.Id).SubItems.Add("Sound file is bigger than 200kb");
                        }
                        if(AudioReader.Mp3WaveFormat.SampleRate < 200)
                        {
                            listView2.Items.Add("SOUND " + sound.Id).SubItems.Add("Not a valid MP3 file");
                        }
                    }
                    catch (Exception ex)
                    {
                        listView2.Items.Add("SOUND " + sound.Id).SubItems.Add("Unable to load mp3: " + ex.Message);
                    }
                    if (!XmlTools.GetAnimationsId().Contains(sound.Id))
                    {
                        listView2.Items.Add("SOUND " + sound.Id).SubItems.Add("There is no animation with id " + sound.Id);
                    }
                    if(sound.Probability <= 0)
                    {
                        listView2.Items.Add("SOUND " + sound.Id).SubItems.Add("Probability is 0, this sound can never be played!");
                    }
                }
            }
            Thread.Sleep(300);
            if (listView2.Items.Count == 0)
            {
                label6.Text = "SUCCESS!";
            }
            else
            {
                label6.Text = "FAILED...";
            }
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (PetValues.AnimationStep < 0) PetValues.AnimationStep = 0;
            try
            {
                NextStep();
                if (IsDisposed)
                {
                    timer1.Enabled = false;
                }
                else
                {
                    //PetValues.AnimationStep++;
                    //timer1.Enabled = true;
                }
            }
            catch (Exception ex) // if form is closed timer could continue to tick (why?)
            {
                Program.AddLog("Error on animation " + + PetValues.CurrentAnimation.ID + "/" + PetValues.CurrentAnimation.Name + ": " + ex.Message, "RUN ERROR", Program.LOG_TYPE.ERROR);
            }
            label18.Text = "Position: " + PetValues.PositionX + "," + PetValues.PositionY;
        }

        private void NextStep()
        {
            // If there is no repeat, we don't need to calculate the frame index.
            if (PetValues.AnimationStep < PetValues.CurrentAnimation.Sequence.Frames.Count)
            {
                pictureBox6.Image = PetValues.Images.Images[PetValues.CurrentAnimation.Sequence.Frames[PetValues.AnimationStep]];
            }
            else
            {
                int index = ((PetValues.AnimationStep - PetValues.CurrentAnimation.Sequence.Frames.Count + PetValues.CurrentAnimation.Sequence.RepeatFrom) % 
                    (PetValues.CurrentAnimation.Sequence.Frames.Count - PetValues.CurrentAnimation.Sequence.RepeatFrom)) +
                    PetValues.CurrentAnimation.Sequence.RepeatFrom;
                pictureBox6.Image = PetValues.Images.Images[PetValues.CurrentAnimation.Sequence.Frames[index]];
            }

            // Get interval, opacity and offset interpolated from START and END values.
            timer1.Interval = PetValues.CurrentAnimation.Start.Interval.Value + ((PetValues.CurrentAnimation.End.Interval.Value - PetValues.CurrentAnimation.Start.Interval.Value) * PetValues.AnimationStep / PetValues.CurrentAnimation.Sequence.TotalSteps);
            Opacity = PetValues.CurrentAnimation.Start.Opacity + (PetValues.CurrentAnimation.End.Opacity - PetValues.CurrentAnimation.Start.Opacity) * PetValues.AnimationStep / PetValues.CurrentAnimation.Sequence.TotalSteps;
            PetValues.OffsetY = PetValues.CurrentAnimation.Start.OffsetY + (double)((PetValues.CurrentAnimation.End.OffsetY - PetValues.CurrentAnimation.Start.OffsetY) * PetValues.AnimationStep / PetValues.CurrentAnimation.Sequence.TotalSteps);

            // If dragging is enabled, move the pet to the mouse position.
            if (PetValues.IsDragging)
            {
                PetValues.PositionX = pictureBox6.Left = tabPage3.PointToClient(Cursor.Position).X - WindowDimensions.Image.X / 2;
                PetValues.PositionY = pictureBox6.Top = tabPage3.PointToClient(Cursor.Position).Y - 2;
                timer1.Enabled = true;
                PetValues.AnimationStep++;
                return;
            }

            double x = PetValues.CurrentAnimation.Start.X.Value;
            double y = PetValues.CurrentAnimation.Start.Y.Value;
            // if TotalSteps is more than 1, we have to interpolate START and END values)
            if (PetValues.CurrentAnimation.Sequence.TotalSteps > 1)
            {
                x += ((PetValues.CurrentAnimation.End.X.Value - PetValues.CurrentAnimation.Start.X.Value) * 
                    (double)PetValues.AnimationStep / (PetValues.CurrentAnimation.Sequence.TotalSteps - 1.0));
                y += ((PetValues.CurrentAnimation.End.Y.Value - PetValues.CurrentAnimation.Start.Y.Value) * 
                    (double)PetValues.AnimationStep / (PetValues.CurrentAnimation.Sequence.TotalSteps - 1.0));
            }
            // If a new animation need to be started
            bool bNewAnimation = false;
            // If the pet is "flipped", mirror the movement
            if (!PetValues.IsMovingLeft) x = -x;

            if (x < 0)   // moving left (detect left borders)
            {
                if (!PetValues.OverWindow)
                {
                    if (PetValues.PositionX + x < 0)    // left screen border!
                    {
                        PetValues.PositionX = WindowDimensions.Screen.X;
                        x = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.VERTICAL);
                        bNewAnimation = true;
                    }
                }
                else
                {
                    if (PetValues.PositionX + x < panel1.Left)    // left window border!
                    {
                        PetValues.PositionX = panel1.Left;
                        x = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.WINDOW);
                        bNewAnimation = true;
                    }
                }
            }
            else if (x > 0)   // moving right (detect right borders)
            {
                if (!PetValues.OverWindow)
                {
                    if (PetValues.PositionX + x + WindowDimensions.Image.X > WindowDimensions.Area.X)    // right screen border!
                    {
                        PetValues.PositionX = WindowDimensions.Area.X - pictureBox6.Width;
                        x = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.VERTICAL);
                        bNewAnimation = true;
                    }
                }
                else
                {
                    if (PetValues.PositionX + x + WindowDimensions.Image.X > panel1.Left + panel1.Width)    // right window border!
                    {
                        PetValues.PositionX = panel1.Left + panel1.Width - WindowDimensions.Image.X;
                        x = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.WINDOW);
                        bNewAnimation = true;
                    }
                }
            }
            if (y > 0)   // moving down (detect taskbar and windows)
            {
                if (PetValues.CurrentAnimation.EndBorder.Count > 0)
                {
                    int bottomY = WindowDimensions.Area.Y;

                    if (PetValues.PositionY + y > bottomY - WindowDimensions.Image.Y) // border detected!
                    {
                        PetValues.PositionY = bottomY - WindowDimensions.Image.Y;
                        PetValues.OffsetY = 0;
                        y = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.TASKBAR);
                        bNewAnimation = true;
                    }
                    else
                    {
                        int iWindowTop = FallDetect((int)y);
                        if (iWindowTop > 0)
                        {
                            PetValues.PositionY = iWindowTop - WindowDimensions.Image.Y;
                            PetValues.OffsetY = 0;
                            y = 0;
                            SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.WINDOW);
                            bNewAnimation = true;
                        }
                    }
                }
            }
            else if (y < 0)  // moving up, detect upper screen border
            {
                if (PetValues.CurrentAnimation.EndBorder.Count > 0)
                {
                    if (PetValues.PositionY < 0) // border detected!
                    {
                        PetValues.PositionY = WindowDimensions.Area.Y;
                        y = 0;
                        SetNewAnimationRequest(false, true, false, TNextAnimation.TOnly.HORIZONTAL);
                        bNewAnimation = true;
                    }
                }
            }

            if (PetValues.AnimationStep >= PetValues.CurrentAnimation.Sequence.TotalSteps) // animation over
            {
                if (PetValues.CurrentAnimation.Sequence.Action == "flip")
                {
                    // flip all images
                    PetValues.IsMovingLeft = !PetValues.IsMovingLeft;
                    for (int i = 0; i < PetValues.Images.Images.Count; i++)
                    {
                        Image im = PetValues.Images.Images[i];
                        im.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        PetValues.Images.Images[i] = im;
                    }
                }
                if (PetValues.OverWindow)
                {
                    SetNewAnimationRequest(true, false, false, TNextAnimation.TOnly.WINDOW);
                    bNewAnimation = true;
                }
                else
                {
                    // If pet is outside the borders, spawn it again.
                    if (pictureBox6.Left < 0 - WindowDimensions.Area.X || pictureBox6.Left > WindowDimensions.Area.X)
                    {
                        // new spawn!
                        contextMenuStrip1.Show(tabPage3, new Point(tabPage3.Width / 10, tabPage3.Height / 2));
                    }
                    else
                    {
                        SetNewAnimationRequest(true, false, false,
                            PetValues.PositionY + WindowDimensions.Image.Y + y >= WindowDimensions.Area.Y - 2 ? TNextAnimation.TOnly.TASKBAR : TNextAnimation.TOnly.NONE
                            );
                        bNewAnimation = true;
                    }
                }
                if (PetValues.CurrentAnimation.ID == petAnimations.AnimationKill)
                {
                    double op;
                    if (timer1.Tag == null || !double.TryParse(timer1.Tag.ToString(), out op)) timer1.Tag = 1.0;
                    op = double.Parse(timer1.Tag.ToString());
                    timer1.Tag = op - 0.1;
                    PetValues.Opacity = op;
                    if (op <= 0.1)
                    {
                        Close();
                    }
                }
                else
                {
                    // Child doesn't have a spawn, they will be closed once the animation is over.
                    if (Name.IndexOf("child") == 0)
                    {
                        //Close();
                    }
                    else
                    {
                        //Play(false);
                    }
                }
            }
            // If there is a Gravity-Next animation, check if gravity is present.
            else if (PetValues.CurrentAnimation.Gravity)
            {
                if (!PetValues.OverWindow)
                {
                    if (PetValues.PositionY + y < WindowDimensions.Area.Y - WindowDimensions.Image.Y)
                    {
                        if (PetValues.PositionY + y + 3 >= WindowDimensions.Area.Y  - WindowDimensions.Image.Y) // allow 3 pixels to move without fall
                        {
                            y = WindowDimensions.Area.Y - (int)PetValues.PositionY - WindowDimensions.Image.Y;
                        }
                        else
                        {
                            SetNewAnimationRequest(false, false, true, TNextAnimation.TOnly.NONE);
                            bNewAnimation = true;
                        }
                    }
                }
                else
                {
                    if (PetValues.AnimationStep > 0 && CheckTopWindow(true))
                    {
                        PetValues.OverWindow = false;
                        SetNewAnimationRequest(false, false, true, TNextAnimation.TOnly.WINDOW);
                        bNewAnimation = true;
                    }
                }
            }

            // If a new animation was started, set the interval and the first animation frame image.
            if (bNewAnimation)
            {
                timer1.Enabled = false;
                //timer1.Interval = 1;    // execute immediately the first step of the next animation.
                //x = 0;                  // don't move the pet, if a new animation must be started
                //y = 0;                //  if falling, set the pet to the new position
                //pictureBox6.Image = PetValues.Images.Images[PetValues.CurrentAnimation.Sequence.Frames[0]];
            }

            // Set the new pet position (and offset) in the screen.
            PetValues.PositionX += x;
            PetValues.PositionY += y;

            pictureBox6.Left = (int)PetValues.PositionX;
            pictureBox6.Top = (int)(PetValues.PositionY + PetValues.OffsetY);

            if(!bNewAnimation)
            {
                timer1.Enabled = true;
                PetValues.AnimationStep++;
            }
        }

        /// <summary>
        /// Detect if pet is still falling or if taskbar/window was detected.
        /// </summary>
        /// <param name="y">Y moves in pixels for the next step (function will detect if window/taskbar is inside the movement).</param>
        /// <returns>Y position of the window or taskbar. -1 if pet is still falling.</returns>
        private int FallDetect(int y)
        {
            // If vertical position is in the falling range and pet is over window and window is at least 20 pixels under the screen border
            if (PetValues.PositionY + WindowDimensions.Image.Y < panel1.Top && PetValues.PositionY + WindowDimensions.Image.Y + y >= panel1.Top &&
                PetValues.PositionX >= panel1.Left - WindowDimensions.Image.X / 2 && PetValues.PositionX + WindowDimensions.Image.X <= panel1.Right + WindowDimensions.Image.X / 2 &&
                PetValues.PositionY > 20)
            {
                // Pet need to walk over THIS window!
                PetValues.OverWindow = true;
                
                // If window is not covered by other windows, set this as current window for the pet.
                if (!CheckTopWindow(false))
                {
                    return panel1.Top;                                 // return the position for the pet
                }
                else
                {
                    PetValues.OverWindow = false;
                }
            }
            return -1;      // no windows detected.
        }

        /// <summary>
        /// Check if current window handler is still valid (if another window cover the visual of this window, it must not be used as window)
        /// </summary>
        /// <param name="bCheck">Check if it is still valid. Set false if window is not proofed, true if pet is already walking on a window => check if window is still valid.</param>
        /// <returns>True if window is still valid and present. False if window is not anymore there.</returns>
        /// <seealso cref="NativeMethods.GetWindow(IntPtr, int)"/>
        /// <seealso cref="NativeMethods.GetTitleBarInfo(IntPtr, ref NativeMethods.TITLEBARINFO)"/>
        private bool CheckTopWindow(bool bCheck)
        {
            // Check only if we have a valid window handler
            if (PetValues.OverWindow)
            {
                return true;
            }
            return false;
        }

        private void pictureBox6_MouseDown(object sender, MouseEventArgs e)
        {
            if (petAnimations.AnimationDrag >= 0)
            {
                SetNewAnimationWithId(petAnimations.AnimationDrag);
                PetValues.IsDragging = true;
                timer1.Enabled = true;
            }
            else
            {
                Program.AddLog("There is no drag animation on this pet", "RUN PET", Program.LOG_TYPE.WARNING);
            }
        }

        private void pictureBox6_MouseUp(object sender, MouseEventArgs e)
        {
            if(PetValues.IsDragging)
            {
                SetNewAnimationRequest(true, true, true, TNextAnimation.TOnly.NONE);
                timer1.Enabled = false;
                PetValues.IsDragging = false;
            }
        }

        private void GenerateViz(double zoom, bool save)
        {
            var stringHTML = XmlToDot.ProcessXml(Program.AnimationXML);
            var mainstring = "<html><body style='zoom:" + (zoom*100) + "%;'>SCRIPT ERROR: maybe your browser is not able to render this viz.</body>";
            mainstring += "<script>var zoom=20;function zoomOut(){zoom*=2;document.body.style.zoom=zoom+'%';} function zoomIn(){zoom/=2;document.body.style.zoom=zoom+'%';}</script>";
            mainstring += "<script>" + Properties.Resources.viz + "</script>";
            mainstring += "<script>" + Properties.Resources.lite_render + "</script>";
            mainstring += "<script>document.body.innerHTML = 'Loading...';var viz = new Viz();";
            mainstring += "document.body.innerHTML = '';";
            mainstring += "</script>";
            mainstring += "<div style='display:block;position:fixed;right:0px;top:0px;width:200px;height:100px;'><span onClick='zoom*=2;document.body.style.zoom=zoom + \"%\";'>+</span><span onClick='zoom*=0.5;document.body.style.zoom=zoom + \"%\";'>-</span><span onClick='window.print();'>SAVE</span></div>";
            mainstring += "<script>";
            mainstring += "viz.renderSVGElement('XXX').then(function(element){document.body.appendChild(element);}).catch(function(error){document.body.appendChild(document.createTextNode(error)); viz = new Viz(); });</script></html>";
            mainstring = mainstring.Replace("XXX", stringHTML);
            webViewCompatible1.NavigateToString(mainstring);
        }

    }
}
