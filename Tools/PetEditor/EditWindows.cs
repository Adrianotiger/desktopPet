using NAudio.Wave;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetEditor
{
    public partial class EditWindows : Form
    {
        Dictionary<string, TextBox> EditValues = new Dictionary<string, TextBox>();
        PictureBox icon = null;
        PictureBox image = null;
        bool spawnsLoaded = false;
        bool animationLoaded = false;
        bool childsLoaded = false;
        bool soundsLoaded = false;
        AnimationStatistics stats = null;
        Mp3FileReader AudioReader;

        List<XmlData.AnimationNode> EditAnimationNode = new List<XmlData.AnimationNode>();
        List<XmlData.SpawnNode> EditSpawnNode = new List<XmlData.SpawnNode>();
        List<XmlData.ChildNode> EditChildNode = new List<XmlData.ChildNode>();
        List<XmlData.SoundNode> EditSoundNode = new List<XmlData.SoundNode>();
        int EditAnimationNodeIndex = -1;
        int EditSpawnNodeIndex = -1;
        int EditChildNodeIndex = -1;
        int EditSoundNodeIndex = -1;

        public EditWindows()
        {
            InitializeComponent();
        }

        private void EditWindows_Load(object sender, EventArgs e)
        {
        }

        public void SelectSection(string section)
        {
            switch(section)
            {
                case "header": tabControl1.SelectedIndex = 0; break;
                case "image": tabControl1.SelectedIndex = 1; break;
                case "spawns": tabControl1.SelectedIndex = 2; break;
                case "animations": tabControl1.SelectedIndex = 3; break;
                case "childs": tabControl1.SelectedIndex = 4; break;
                case "sounds": tabControl1.SelectedIndex = 5; break;
            }
        }

        public void UpdateData()
        {
            LoadHeader();
            LoadImage();
            LoadSpawns();
            LoadAnimations();
            LoadChilds();
            LoadSounds();
        }

        private void LoadHeader()
        {
            var h = Program.AnimationXML.Header;
            if (h == null)
            {
                h = new XmlData.HeaderNode();
                h.Application = "1";
            }
            if (!EditValues.ContainsKey("author"))
            {
                tableLayoutPanel1.Controls.Clear();
                tableLayoutPanel1.RowCount = 1;
                EditValues.Add("author", AddTextBoxToTable("Author: ", h.Author, tableLayoutPanel1));
                EditValues.Add("project", AddTextBoxToTable("Project: ", h.Title, tableLayoutPanel1));
                EditValues.Add("version", AddTextBoxToTable("Version: ", h.Version, tableLayoutPanel1));
                EditValues.Add("info", AddTextBoxToTable("Info: ", h.Info, tableLayoutPanel1, false, true));
                EditValues.Add("application", AddTextBoxToTable("Application: ", h.Application, tableLayoutPanel1, true));
                EditValues.Add("petname", AddTextBoxToTable("Pet name: ", h.Petname, tableLayoutPanel1));
                icon = AddImageToTable("Icon: ", h.Icon, tableLayoutPanel1);

                foreach(var tb in EditValues)
                {
                    tb.Value.TextChanged += (s, e) =>
                    {
                        switch(tb.Key)
                        {
                            case "author": Program.AnimationXML.Header.Author = tb.Value.Text; break;
                            case "project": Program.AnimationXML.Header.Title = tb.Value.Text; break;
                            case "version": Program.AnimationXML.Header.Version = tb.Value.Text; break;
                            case "info": Program.AnimationXML.Header.Info = tb.Value.Text.Replace("\n", "[br]"); break;
                            case "petname": Program.AnimationXML.Header.Petname = tb.Value.Text; break;
                        }
                    };
                }
                icon.BackgroundImageChanged += (s, e) =>
                {
                    if (icon.BackgroundImage == null) return;

                    Bitmap icoBmp = new Bitmap(icon.Image);

                    icon.BackgroundImage = null;

                    if (icon.Image.PhysicalDimension.Height != 48 || icon.Image.PhysicalDimension.Width != 48)
                    {
                        Program.AddLog("Icon is not 48x48, resizing...", "LOAD ICON");

                        icoBmp = new Bitmap(48, 48);

                        var graph = Graphics.FromImage(icoBmp);
                        graph.DrawImage(icon.Image, 0, 0, 48, 48);
                    }
                    
                    IntPtr hIco = icoBmp.GetHicon();
                    Icon ico = Icon.FromHandle(hIco);

                    var stream = new MemoryStream();
                    ico.Save(stream);
                    var bytes = stream.ToArray();
                    Program.AnimationXML.Header.Icon = Convert.ToBase64String(bytes);
                    icon.Image = icoBmp;
                };
            }
            else
            {
                EditValues["author"].Text = h.Author;
                EditValues["project"].Text = h.Title;
                EditValues["version"].Text = h.Version;
                EditValues["info"].Text = h.Info;
                EditValues["application"].Text = h.Application;
                EditValues["petname"].Text = h.Petname;
                icon.Image = Image.FromStream(new MemoryStream(Convert.FromBase64String(h.Icon)));
            }
        }

        private void LoadImage()
        {
            var h = Program.AnimationXML.Image;
            if (h == null)
            {
                h = new XmlData.ImageNode();
                h.Transparency = "Magenta";
            }
            if (!EditValues.ContainsKey("tilesx"))
            {
                tableLayoutPanel2.Controls.Clear();
                tableLayoutPanel2.RowCount = 1;
                EditValues.Add("tilesx", AddTextBoxToTable("Tiles X: ", h.TilesX, tableLayoutPanel2));
                EditValues.Add("tilesy", AddTextBoxToTable("Tiles Y: ", h.TilesY, tableLayoutPanel2));
                EditValues.Add("transparency", AddTextBoxToTable("Transparency Key: ", h.Transparency, tableLayoutPanel2));
                image = AddImageToTable("Image: ", h.Png, tableLayoutPanel2);

                for (var j=EditValues.Count - 3;j<EditValues.Count;j++)
                {
                    var el = EditValues.ElementAt(j);
                    el.Value.TextChanged += (s, e) =>
                    {
                        switch (el.Key)
                        {
                            case "tilesx":
                                try
                                {
                                    Program.AnimationXML.Image.TilesX = int.Parse(el.Value.Text);
                                    DrawLinesOnPictures(pictureBox1, Image.FromStream(
                                        new MemoryStream(Convert.FromBase64String(Program.AnimationXML.Image.Png))), 
                                        Program.AnimationXML.Image.TilesX, 
                                        Program.AnimationXML.Image.TilesY);
                                }
                                catch(Exception)
                                {
                                    el.Value.Text = Program.AnimationXML.Image.TilesX.ToString();
                                }
                                break;
                            case "tilesy":
                                try
                                {
                                    Program.AnimationXML.Image.TilesY = int.Parse(el.Value.Text);
                                    DrawLinesOnPictures(pictureBox1, Image.FromStream(
                                        new MemoryStream(Convert.FromBase64String(Program.AnimationXML.Image.Png))), 
                                        Program.AnimationXML.Image.TilesX, 
                                        Program.AnimationXML.Image.TilesY);
                                }
                                catch (Exception)
                                {
                                    el.Value.Text = Program.AnimationXML.Image.TilesY.ToString();
                                }

                                break;
                            case "transparency": Program.AnimationXML.Image.Transparency = el.Value.Text; break;
                        }
                    };
                }
                image.BackgroundImageChanged += (s, e) =>
                {
                    var stream = new MemoryStream();
                    image.Image.Save(stream, image.Image.RawFormat);
                    var bytes = stream.ToArray();
                    Program.AnimationXML.Image.Png = Convert.ToBase64String(bytes);
                    DrawLinesOnPictures(pictureBox1, image.Image, Program.AnimationXML.Image.TilesX, Program.AnimationXML.Image.TilesY);

                    XmlTools.GenerateImageIcons(image.Image, Program.AnimationXML.Image.TilesX, Program.AnimationXML.Image.TilesY);

                    panel1.Height = Math.Max(100, image.Image.Height / Program.AnimationXML.Image.TilesY);
                    panel1.Width = Math.Max(200, image.Image.Width / Program.AnimationXML.Image.TilesX);
                };
            }
            else
            {
                EditValues["tilesx"].Text = h.TilesX.ToString();
                EditValues["tilesy"].Text = h.TilesY.ToString();
                EditValues["transparency"].Text = h.Transparency;
                try
                {
                    image.Image = Image.FromStream(new MemoryStream(Convert.FromBase64String(h.Png)));
                    DrawLinesOnPictures(pictureBox1, image.Image, h.TilesX, h.TilesY);
                    XmlTools.GenerateImageIcons(image.Image, h.TilesX, h.TilesY);

                    panel1.Height = Math.Max(100, image.Image.Height / h.TilesY);
                    panel1.Width = Math.Max(200, image.Image.Width / h.TilesX);
                }
                catch(Exception ex)
                {
                    Program.AddLog("Invalid base 64 image (unable to load): " + ex.Message, "LOADING PET IMAGES", Program.LOG_TYPE.ERROR, pictureBox1);
                }
            }
        }

        private void LoadSpawns()
        {
            if(!spawnsLoaded)
            {
                spawnsLoaded = true;
                richTextBox1.KeyDown += richTextBox_KeyDown;
                richTextBox2.KeyDown += richTextBox_KeyDown;
            }
            var listItems = XmlTools.GetAnimationList();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(listItems);
            /*
            comboBox4.Items.Clear();
            comboBox4.Items.AddRange(listItems);
            comboBox5.Items.Clear();
            comboBox5.Items.AddRange(listItems);
            */
            tabPage3.Tag = null;
            groupBox1.Enabled = false;
            groupBox2.Enabled = false;
            groupBox3.Enabled = false;
        }

        public bool SetSpawnEdit(int spawnId)
        {
            if(EditSpawnNode.Count > 1 && EditSpawnNodeIndex > 0)
            {
                MessageBox.Show("Please save your spawn before opening another one.", "spawn not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            var node = XmlTools.GetXmlSpawnNode(spawnId);
            if (node != null)
            {
                FillSpawnFormFromNode(node);

                tabPage3.Tag = spawnId;

                EditSpawnNode.Clear();
                EditSpawnNode.Add(node);
                EditSpawnNodeIndex = 0;
                DisableEditToolbar();

                UpdateSpawnPreview(node);
                return true;
            }
            return false;
        }

        private void FillSpawnFormFromNode(XmlData.SpawnNode node)
        {
            tabPage3.Tag = null;
            groupBox1.Enabled = true;
            groupBox2.Enabled = true;
            groupBox3.Enabled = true;

            label2.Text = node.Id.ToString();
            numericUpDown1.Value = node.Probability;
            richTextBox1.Text = node.X;
            richTextBox2.Text = node.Y;

            for (var j = 0; j < comboBox1.Items.Count; j++)
            {
                if (node.Next.Value == ((XmlNodeCombo)comboBox1.Items[j]).Id)
                {
                    comboBox1.SelectedIndex = j;
                    break;
                }
            }
            tabPage3.Tag = node.Id;
        }

        private XmlData.SpawnNode FillNodeFromSpawnForm()
        {
            XmlData.SpawnNode node = new XmlData.SpawnNode();
            node.Next = new XmlData.NextNode();

            node.Id = int.Parse(tabPage3.Tag.ToString());
            if(comboBox1.SelectedItem != null)
                node.Next.Value = ((XmlNodeCombo)comboBox1.SelectedItem).Id;
            node.Probability = (int)numericUpDown1.Value;
            node.X = richTextBox1.Text;
            node.Y = richTextBox2.Text;
            UpdateSpawnPreview(node);

            if (EditSpawnNode.Count > EditSpawnNodeIndex + 1)
            {
                EditSpawnNode.RemoveRange(EditSpawnNodeIndex + 1, EditSpawnNode.Count - EditSpawnNodeIndex - 1);
            }
            return node;
        }

        private void UpdateSpawnPreview(XmlData.SpawnNode node)
        {
            var pInfo = pictureBox3.GetType().GetProperty("ImageRectangle",
                                    System.Reflection.BindingFlags.Public |
                                    System.Reflection.BindingFlags.NonPublic |
                                    System.Reflection.BindingFlags.Instance);

            Rectangle rectangle = (Rectangle)pInfo.GetValue(pictureBox3, null);
            var scaleX = (double)pictureBox3.Width / rectangle.Width;
            var scaleY = (double)pictureBox3.Height / rectangle.Height;
            var screenScaleX = ((double)Screen.PrimaryScreen.Bounds.Width / (pictureBox3.Width * 0.57));
            var screenScaleY = ((double)Screen.PrimaryScreen.Bounds.Height / (pictureBox3.Height * 0.33));
            var imageZoom = 3;
            var imgW = image.Width * imageZoom / screenScaleX;
            var imgH = image.Height * imageZoom / screenScaleX;

            if (comboBox1.SelectedItem != null)
            {
                try
                {
                    var imageIndex = XmlTools.GetXmlAnimationNode(((XmlNodeCombo)comboBox1.SelectedItem).Id).Sequence.Frame[0];
                    pictureBox4.Image = XmlTools.AnimationIcons.Images[imageIndex];
                    pictureBox4.Width = (int)imgW;
                    pictureBox4.Height = (int)imgW;
                    pictureBox4.Left = rectangle.X + (int)(pictureBox3.Width * 0.21875 / scaleX);
                    pictureBox4.Top = rectangle.Y + (int)(pictureBox3.Height * 0.3375 / scaleY);
                }
                catch(Exception ex)
                {
                    Program.AddLog("Unable to get frame image - " + ex.Message, "SPAWN animation", Program.LOG_TYPE.WARNING, tabPage2);
                }
                XmlTools.StatisticsDataInput vals = new XmlTools.StatisticsDataInput
                {
                    Area = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height),
                    Screen = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                    Image = new Rectangle(0, 0, image.Width * imageZoom, image.Height * imageZoom)
                };
                try
                {
                    pictureBox4.Left += (int)(XmlTools.EvalValue(node.X, vals, richTextBox1) / screenScaleX / scaleX);
                }
                catch (Exception){}
                try
                {
                    pictureBox4.Top += (int)(XmlTools.EvalValue(node.Y, vals, richTextBox2) * 0.97 / screenScaleY / scaleY) - (int)(imgH / 5);
                }
                catch (Exception){}
            }
        }

        private void spawn_Edited(object sender, EventArgs e)
        {
            if (tabPage3.Tag == null) return;

            var node = FillNodeFromSpawnForm();

            EditSpawnNode.Add(node);
            EditSpawnNodeIndex = EditSpawnNode.Count - 1;
            EditSpawn(false, true, false);
            tabControl1.TabPages["tabPage3"].Text = "Spawns *";
            //XmlTools.UpdateXmlSpawnNode(node);

        }

        private void LoadAnimations()
        {
            if (!animationLoaded)
            {
                animationLoaded = true;
                richTextBox4.KeyDown += richTextBox_KeyDown;
                richTextBox5.KeyDown += richTextBox_KeyDown;
                richTextBox6.KeyDown += richTextBox_KeyDown;
                richTextBox8.KeyDown += richTextBox_KeyDown;
                richTextBox9.KeyDown += richTextBox_KeyDown;
                richTextBox10.KeyDown += richTextBox_KeyDown;
                richTextBox12.KeyDown += richTextBox_KeyDown;

                listView1.SmallImageList = XmlTools.AnimationIcons;
                listView1.LargeImageList = XmlTools.AnimationImages;
                listView1.ItemDrag += (s, e) =>
                {
                    DoDragDrop(listView1.SelectedItems[0], DragDropEffects.Move);
                };
                listView1.DragEnter += (s, e) =>
                {
                    if (!e.Data.GetDataPresent(typeof(ListViewItem)))
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    e.Effect = DragDropEffects.Move;
                };
                listView1.DragOver += (s, e) =>
                {
                    if (!e.Data.GetDataPresent(typeof(ListViewItem)))
                    {
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    Point cp = listView1.PointToClient(new Point(e.X, e.Y));
                    ListViewItem hoverItem = listView1.GetItemAt(cp.X, cp.Y);
                    if (hoverItem != null)
                    {
                        hoverItem.EnsureVisible();
                        e.Effect = DragDropEffects.None;
                        return;
                    }
                    e.Effect = DragDropEffects.Move;
                };
                listView1.DragDrop += (s, e) =>
                {
                    if (!e.Data.GetDataPresent(typeof(ListViewItem)))
                    {
                        return;
                    }
                    Point cp = listView1.PointToClient(new Point(e.X, e.Y));
                    ListViewItem hoverItem = null;
                    for (var j = 0; j < 100; j++)
                    {
                        hoverItem = listView1.GetItemAt(cp.X, cp.Y);
                        if (hoverItem != null || j == 90)
                        {
                            var item = (ListViewItem)e.Data.GetData(typeof(ListViewItem));
                            var itemIndex = item.Index;
                            var dropIndex = hoverItem.Index;
                            listView1.BeginUpdate();
                            listView1.Items.RemoveAt(itemIndex);
                            var lv = listView1.Items.Insert(hoverItem.Index + 1, (ListViewItem)item.Clone());
                            listView1.EndUpdate();

                            listView1.Sort();
                            break;
                        }
                        cp.X -= 5;
                    }
                    animation_Edited(null, null);
                };
                listView1.ListViewItemSorter = new CompareByIndex(listView1);

                listView2.SmallImageList = XmlTools.AnimationIcons;
                listView3.SmallImageList = XmlTools.AnimationIcons;
                listView4.SmallImageList = XmlTools.AnimationIcons;
            }
            /*
            var listItems = XmlTools.GetAnimationList();
            comboBox1.Items.Clear();
            comboBox1.Items.AddRange(listItems);
            comboBox4.Items.Clear();
            comboBox4.Items.AddRange(listItems);
            comboBox5.Items.Clear();
            comboBox5.Items.AddRange(listItems);
            */
            toolStripComboBox2.Items.Clear();
            toolStripComboBox2.Items.AddRange(XmlTools.GetAnimationList());
            tabPage4.Tag = null;
            groupBox5.Enabled = false;
            groupBox6.Enabled = false;
            groupBox7.Enabled = false;
            groupBox8.Enabled = false;
            listView1.LargeImageList = XmlTools.AnimationImages;
            listView1.SmallImageList = XmlTools.AnimationImages;
        }
        
        public bool SetAnimationEdit(int animationId)
        {
            if (EditAnimationNode.Count > 1 && EditAnimationNodeIndex > 0)
            {
                MessageBox.Show("Please save your animation before opening another one.", "animation not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            var node = XmlTools.GetXmlAnimationNode(animationId);
            if (node != null)
            {
                FillAnimationFormFromNode(node);

                tabPage4.Tag = animationId;

                EditAnimationNode.Clear();
                EditAnimationNode.Add(node);
                EditAnimationNodeIndex = 0;
                DisableEditToolbar();

                return true;
            }
            return false;
        }

        public void FillAnimationFormFromNode(XmlData.AnimationNode node)
        {
            tabPage4.Tag = null;
            groupBox5.Enabled = true;
            groupBox6.Enabled = true;
            groupBox7.Enabled = true;
            groupBox8.Enabled = true;

            comboBox2.Items.Clear();
            comboBox2.SelectedIndex = comboBox2.Items.Add(node.Id);
            var ids = XmlTools.GetAnimationsId();
            for (int idBase = 1; idBase < ids.Count + 10; idBase++)
            {
                if (!ids.Contains(idBase))
                {
                    comboBox2.Items.Add(idBase);
                }
            }

            richTextBox3.Text = node.Name;

            richTextBox4.Text = node.Start.X;
            richTextBox5.Text = node.Start.Y;
            richTextBox6.Text = node.Start.Interval;
            numericUpDown2.Value = (decimal)node.Start.OffsetY;
            numericUpDown3.Value = (decimal)node.Start.Opacity;

            richTextBox8.Text = node.End.X;
            richTextBox9.Text = node.End.Y;
            richTextBox10.Text = node.End.Interval;
            numericUpDown6.Value = (decimal)node.End.OffsetY;
            numericUpDown4.Value = (decimal)node.End.Opacity;
            if (node.Sequence.Action == "flip")
                comboBox3.SelectedIndex = 1;
            else
                comboBox3.SelectedIndex = 0;

            richTextBox12.Text = node.Sequence.RepeatCount;
            numericUpDown5.Value = (decimal)node.Sequence.RepeatFromFrame;

            listView1.Items.Clear();
            int id = 0;
            foreach (var f in node.Sequence.Frame)
            {
                listView1.Items.Add(f.ToString() + " (" + id + ")", f);
                id++;
            }

            listView2.Items.Clear();
            if (node.Sequence.Next != null)
            {
                foreach (var n in node.Sequence.Next)
                {
                    var lvi = listView2.Items.Add(n.Value.ToString());
                    var n2 = XmlTools.GetXmlAnimationNode(n.Value);
                    try
                    {
                        lvi.ImageIndex = n2.Sequence.Frame[0];
                        lvi.SubItems.Add(n2.Name);
                    }
                    catch (Exception)
                    {
                        Program.AddLog("Animation " + n.Value + " does not have frame Index", "Open Node " + n.Value, Program.LOG_TYPE.ERROR, tabPage4);
                        lvi.SubItems.Add("--");
                    }
                    lvi.SubItems.Add(n.Probability.ToString());
                    lvi.SubItems.Add(n.OnlyFlag);
                }
            }
            listView2.Height = (listView2.Items.Count + 2) * 33;

            listView3.Items.Clear();
            if (node.Border != null && node.Border.Next != null)
            {
                foreach (var n in node.Border.Next)
                {
                    var lvi = listView3.Items.Add(n.Value.ToString());
                    var n2 = XmlTools.GetXmlAnimationNode(n.Value);
                    try
                    {
                        lvi.ImageIndex = n2.Sequence.Frame[0];
                        lvi.SubItems.Add(n2.Name);
                    }
                    catch (Exception)
                    {
                        Program.AddLog("Animation " + n.Value + " does not have frame Index", "Open Node " + n.Value, Program.LOG_TYPE.ERROR, tabPage4);
                        lvi.SubItems.Add("--");
                    }
                    lvi.SubItems.Add(n.Probability.ToString());
                    lvi.SubItems.Add(n.OnlyFlag);
                }
            }
            listView3.Height = (listView3.Items.Count + 2) * 33;

            listView4.Items.Clear();
            if (node.Gravity != null && node.Gravity.Next != null)
            {
                foreach (var n in node.Gravity.Next)
                {
                    var lvi = listView4.Items.Add(n.Value.ToString());
                    var n2 = XmlTools.GetXmlAnimationNode(n.Value);
                    try
                    {
                        lvi.ImageIndex = n2.Sequence.Frame[0];
                        lvi.SubItems.Add(n2.Name);
                    }
                    catch (Exception)
                    {
                        Program.AddLog("Animation " + n.Value + " does not have frame Index", "Open Node " + n.Value, Program.LOG_TYPE.ERROR, tabPage4);
                        lvi.SubItems.Add("--");
                    }
                    lvi.SubItems.Add(n.Probability.ToString());
                    lvi.SubItems.Add(n.OnlyFlag);
                }
            }
            listView4.Height = (listView4.Items.Count + 2) * 33;

            groupBox8.Height = flowLayoutPanel1.Height + 50;

            UpdateAnimationStatistics(node);

            tabPage4.Tag = node.Id;
        }

        private void UpdateAnimationStatistics(XmlData.AnimationNode node)
        {
            XmlTools.StatisticsDataInput vals = new XmlTools.StatisticsDataInput
            {
                Area = new Point(Screen.PrimaryScreen.WorkingArea.Width, Screen.PrimaryScreen.WorkingArea.Height),
                Screen = new Point(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height),
                Image = new Rectangle(0, 0, image.Width, image.Height)
            };

            stats = XmlTools.GetAnimationStatistics(node, vals, tabPage4);
            label28.Text = stats.TotalX.ToString();
            label29.Text = stats.TotalY.ToString();
            label30.Text = stats.TotalFrames.ToString();
            label32.Text = stats.TotalTime.ToString();

            timer1.Tag = 0;
            timer1.Interval = stats.Start.Interval;
            if(stats.TotalFrames > 0) timer1.Enabled = true;
        }

        private XmlData.AnimationNode FillNodeFromAnimationForm()
        {
            XmlData.AnimationNode node = new XmlData.AnimationNode();

            node.Id = int.Parse(comboBox2.Text);
            node.Name = richTextBox3.Text;
            node.Sequence = new XmlData.SequenceNode();
            node.Sequence.RepeatCount = richTextBox12.Text;
            node.Sequence.RepeatFromFrame = (int)numericUpDown5.Value;
            node.Sequence.Action = comboBox3.Text;
            node.Sequence.Frame = new int[listView1.Items.Count];
            for(var k=0;k<listView1.Items.Count;k++)
            {
                node.Sequence.Frame[k] = listView1.Items[k].ImageIndex;
            }
            node.Sequence.Next = new XmlData.NextNode[listView2.Items.Count];
            for (var k = 0; k < listView2.Items.Count; k++)
            {
                node.Sequence.Next[k] = new XmlData.NextNode();
                node.Sequence.Next[k].Value = int.Parse(listView2.Items[k].SubItems[0].Text);
                node.Sequence.Next[k].Probability = int.Parse(listView2.Items[k].SubItems[2].Text);
                node.Sequence.Next[k].OnlyFlag = listView2.Items[k].SubItems[3].Text;
            }
            if (listView3.Items.Count > 0)
            {
                node.Border = new XmlData.HitNode();
                node.Border.Next = new XmlData.NextNode[listView3.Items.Count];
                for (var k = 0; k < listView3.Items.Count; k++)
                {
                    node.Border.Next[k] = new XmlData.NextNode();
                    node.Border.Next[k].Value = int.Parse(listView3.Items[k].SubItems[0].Text);
                    node.Border.Next[k].Probability = int.Parse(listView3.Items[k].SubItems[2].Text);
                    node.Border.Next[k].OnlyFlag = listView3.Items[k].SubItems[3].Text;
                }
            }
            else
            {
                node.Border = null;
            }
            if (listView4.Items.Count > 0)
            {
                node.Gravity = new XmlData.HitNode();
                node.Gravity.Next = new XmlData.NextNode[listView4.Items.Count];
                for (var k = 0; k < listView4.Items.Count; k++)
                {
                    node.Gravity.Next[k] = new XmlData.NextNode();
                    node.Gravity.Next[k].Value = int.Parse(listView4.Items[k].SubItems[0].Text);
                    node.Gravity.Next[k].Probability = int.Parse(listView4.Items[k].SubItems[2].Text);
                    node.Gravity.Next[k].OnlyFlag = listView4.Items[k].SubItems[3].Text;
                }
            }
            else
            {
                node.Gravity = null;
            }
            node.Start = new XmlData.MovingNode();
            node.Start.Interval = richTextBox6.Text;
            node.Start.OffsetY = (int)numericUpDown2.Value;
            node.Start.Opacity = (double)numericUpDown3.Value;
            node.Start.X = richTextBox4.Text;
            node.Start.Y = richTextBox5.Text;
            node.End = new XmlData.MovingNode();
            node.End.Interval = richTextBox10.Text;
            node.End.OffsetY = (int)numericUpDown6.Value;
            node.End.Opacity = (double)numericUpDown4.Value;
            node.End.X = richTextBox8.Text;
            node.End.Y = richTextBox9.Text;


            if (EditAnimationNode.Count > EditAnimationNodeIndex + 1)
            {
                EditAnimationNode.RemoveRange(EditAnimationNodeIndex + 1, EditAnimationNode.Count - EditAnimationNodeIndex - 1);
            }
            return node;
        }

        private void animation_Edited(object sender, EventArgs e)
        {
            if (tabPage4.Tag == null) return;

            var node = FillNodeFromAnimationForm();

            EditAnimationNode.Add(node);
            EditAnimationNodeIndex = EditAnimationNode.Count - 1;
            EditAnimation(false, true, false);
            tabControl1.TabPages["tabPage4"].Text = "Animations*";

            XmlTools.ProofAnimation(EditAnimationNode[0], node);

            UpdateAnimationStatistics(node);
        }

        private void LoadChilds()
        {
            if (!childsLoaded)
            {
                childsLoaded = true;
                richTextBox11.KeyDown += richTextBox_KeyDown;
                richTextBox7.KeyDown += richTextBox_KeyDown;
            }
            var listItems = XmlTools.GetAnimationList();
            comboBox4.Items.Clear();
            comboBox4.Items.AddRange(listItems);
            comboBox5.Items.Clear();
            comboBox5.Items.AddRange(listItems);
            tabPage5.Tag = null;
            groupBox4.Enabled = false;
            groupBox10.Enabled = false;
            groupBox11.Enabled = false;
        }

        public bool SetChildEdit(int childId)
        {
            if (EditChildNode.Count > 1 && EditChildNodeIndex > 0)
            {
                MessageBox.Show("Please save your child before opening another one.", "child not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            var node = XmlTools.GetXmlChildNode(childId);
            if (node != null)
            {
                FillChildFormFromNode(node);

                tabPage5.Tag = childId;

                EditChildNode.Clear();
                EditChildNode.Add(node);
                EditChildNodeIndex = 0;
                DisableEditToolbar();

                //UpdateSpawnPreview(node);
                return true;
            }
            return false;
        }

        private void FillChildFormFromNode(XmlData.ChildNode node)
        {
            tabPage5.Tag = null;
            groupBox4.Enabled = true;
            groupBox10.Enabled = true;
            groupBox11.Enabled = true;

            richTextBox11.Text = node.X;
            richTextBox7.Text = node.Y;

            for (var j = 0; j < comboBox4.Items.Count; j++)
            {
                if (node.Id == ((XmlNodeCombo)comboBox4.Items[j]).Id)
                {
                    comboBox4.SelectedIndex = j;
                    break;
                }
            }
            for (var j = 0; j < comboBox5.Items.Count; j++)
            {
                if (node.Next == ((XmlNodeCombo)comboBox5.Items[j]).Id)
                {
                    comboBox5.SelectedIndex = j;
                    break;
                }
            }
            tabPage5.Tag = node.Id;
        }

        private XmlData.ChildNode FillNodeFromChildForm()
        {
            XmlData.ChildNode node = new XmlData.ChildNode();

            if (comboBox4.SelectedItem != null)
                node.Id = ((XmlNodeCombo)comboBox4.SelectedItem).Id;
            if (comboBox5.SelectedItem != null)
                node.Next = ((XmlNodeCombo)comboBox5.SelectedItem).Id;
            node.X = richTextBox11.Text;
            node.Y = richTextBox7.Text;
            //UpdateSpawnPreview(node);

            if (EditChildNode.Count > EditChildNodeIndex + 1)
            {
                EditChildNode.RemoveRange(EditChildNodeIndex + 1, EditChildNode.Count - EditChildNodeIndex - 1);
            }
            return node;
        }

        private void child_Edited(object sender, EventArgs e)
        {
            if (tabPage5.Tag == null) return;

            var node = FillNodeFromChildForm();

            EditChildNode.Add(node);
            EditChildNodeIndex = EditChildNode.Count - 1;
            EditChild(false, true, false);
            tabControl1.TabPages["tabPage5"].Text = "Childs *";
            //XmlTools.UpdateXmlSpawnNode(node);

        }

        private void LoadSounds()
        {
            if (!soundsLoaded)
            {
                soundsLoaded = true;
            }
            var listItems = XmlTools.GetAnimationList();
            comboBox6.Items.Clear();
            comboBox6.Items.AddRange(listItems);
            tabPage6.Tag = null;
            comboBox6.Tag = -1;
            groupBox13.Enabled = false;
            groupBox12.Enabled = false;
            groupBox14.Enabled = false;
            groupBox15.Enabled = false;
        }

        public bool SetSoundEdit(int soundIndex)
        {
            if (EditSoundNode.Count > 1 && EditSoundNodeIndex > 0)
            {
                MessageBox.Show("Please save your sound before opening another one.", "sound not saved", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            comboBox6.Tag = -1;
            var node = XmlTools.GetXmlSoundNode(soundIndex);
            if (node != null)
            {
                comboBox6.Tag = soundIndex;
                FillSoundFormFromNode(node);
                
                EditSoundNode.Clear();
                EditSoundNode.Add(node);
                EditSoundNodeIndex = 0;
                DisableEditToolbar();

                UpdateSoundPreview(node);
                return true;
            }
            return false;
        }

        private void FillSoundFormFromNode(XmlData.SoundNode node)
        {
            tabPage6.Tag = null;
            groupBox13.Enabled = true;
            groupBox12.Enabled = true;
            groupBox14.Enabled = true;

            numericUpDown7.Value = node.Probability;
            numericUpDown8.Value = node.Loop;

            textBox1.Text = "loading...";

            BackgroundWorker bg = new BackgroundWorker();
            bg.DoWork += new DoWorkEventHandler((s,e)=>
            {
                textBox1.Invoke(new MethodInvoker(()=>{
                    textBox1.Text = node.Base64;

                    for (var j = 0; j < comboBox6.Items.Count; j++)
                    {
                        if (node.Id == ((XmlNodeCombo)comboBox6.Items[j]).Id)
                        {
                            comboBox6.SelectedIndex = j;
                            break;
                        }
                    }
                    tabPage6.Tag = node.Id;
                }));
            });
            bg.RunWorkerAsync();
        }

        private XmlData.SoundNode FillNodeFromSoundForm()
        {
            XmlData.SoundNode node = new XmlData.SoundNode();

            if (comboBox6.SelectedItem != null)
                node.Id = ((XmlNodeCombo)comboBox6.SelectedItem).Id;
            
            node.Probability = (int)numericUpDown7.Value;
            node.Loop = (int)numericUpDown8.Value;
            node.Base64 = textBox1.Text;
            UpdateSoundPreview(node);

            if (EditSoundNode.Count > EditSoundNodeIndex + 1)
            {
                EditSoundNode.RemoveRange(EditSoundNodeIndex + 1, EditSoundNode.Count - EditSoundNodeIndex - 1);
            }
            return node;
        }

        private void sound_Edited(object sender, EventArgs e)
        {
            if (tabPage6.Tag == null) return;

            var node = FillNodeFromSoundForm();

            EditSoundNode.Add(node);
            EditSoundNodeIndex = EditSoundNode.Count - 1;
            EditSound(false, true, false);
            tabControl1.TabPages["tabPage6"].Text = "Sounds *";
        }

        private void UpdateSoundPreview(XmlData.SoundNode node)
        {
            try
            {
                MemoryStream ms = new MemoryStream(Convert.FromBase64String(node.Base64));
                AudioReader = new Mp3FileReader(ms);

                label41.Text = "Size: " + (((int)(AudioReader.Length / 102.4)) / 10.0) + " kb";
                label42.Text = (int)AudioReader.TotalTime.TotalSeconds + "." + (AudioReader.TotalTime.TotalMilliseconds - ((int)AudioReader.TotalTime.TotalSeconds * 1000)).ToString("000") + " s";
                label43.Text = "Sample rate: " + AudioReader.Mp3WaveFormat.SampleRate.ToString() + "Hz";
                label44.Text = "Bits: " + AudioReader.WaveFormat.BitsPerSample.ToString() + " bits";
                label45.Text = "Encoding: " + AudioReader.Mp3WaveFormat.Encoding.ToString();
                groupBox15.Enabled = true;
            }
            catch(Exception ex)
            {
                groupBox15.Enabled = false;
                Program.AddLog(ex.Message, "Sound Preview", Program.LOG_TYPE.WARNING, tabPage5);
            }
        }

        private TextBox AddTextBoxToTable(string text, object value, TableLayoutPanel panel, bool onlyread = false, bool multiline = false)
        {
            var l = new Label
            {
                Text = text,
                Parent = panel,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.SetRow(l, panel.RowCount - 1);
            panel.SetColumn(l, 0);
            var t = new TextBox
            {
                Text = "",
                Dock = DockStyle.Fill,
                Parent = panel,
                Multiline = multiline,
                Height = multiline ? 100 : 0,
                ReadOnly = onlyread
            };
            if(value == null)
            {

            }
            else if (value.GetType().Equals(typeof(String)))
            {
                t.Text = value.ToString();
            }
            else if (value.GetType().Equals(typeof(int)))
            {
                t.Text = value.ToString();
            }
            panel.SetRow(t, panel.RowCount - 1);
            panel.SetColumn(t, 1);
            panel.RowCount++;
            return t;
        }

        private PictureBox AddImageToTable(string text, string base64, TableLayoutPanel panel)
        {
            if (base64 == null) base64 = "";
            var iconStream = new MemoryStream(Convert.FromBase64String(base64));

            var l = new Label
            {
                Text = text,
                Parent = panel,
                TextAlign = ContentAlignment.MiddleCenter
            };
            panel.SetRow(l, panel.RowCount - 1);
            panel.SetColumn(l, 0);

            var gb = new Panel
            {
                Parent = panel
            };

            var t = new PictureBox
            {
                Width = 48,
                Height = 48,
                BorderStyle = BorderStyle.Fixed3D,
                Margin = new Padding(0),
                Padding = new Padding(0),
                SizeMode = PictureBoxSizeMode.StretchImage,
                Image = base64 != "" ? Image.FromStream(iconStream) : null,
                Parent = gb
            };

            var b = new Button
            {
                Text = "Load image...",
                Dock = DockStyle.Bottom,
                Parent = gb
            };

            b.Click += (s, e) =>
            {
                OpenFileDialog openFileDialog = new OpenFileDialog();
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Filter = "Image (*.png, *.ico)|*.png;*.ico|All files (*.*)|*.*";
                if (openFileDialog.ShowDialog(this) == DialogResult.OK)
                {
                    t.Image = Image.FromFile(openFileDialog.FileName);
                    t.BackgroundImage = t.Image;
                }
            };

            panel.SetRow(gb, panel.RowCount - 1);
            panel.SetColumn(gb, 1);
            panel.RowCount++;
            return t;
        }

        private void tabPage1_Click(object sender, EventArgs e)
        {

        }

        private void DrawLinesOnPictures(PictureBox pb, Image original, int tilesX, int tilesY)
        {
            Bitmap bmp = new Bitmap(original);
            if(bmp.PhysicalDimension.Width % tilesX != 0)
            {
                Program.AddLog("Picture width (" + bmp.PhysicalDimension.Width + ") is not divisible for " + tilesX, "Tiles", Program.LOG_TYPE.WARNING, pictureBox1);
            }
            if (bmp.PhysicalDimension.Height % tilesY != 0)
            {
                Program.AddLog("Picture height (" + bmp.PhysicalDimension.Height + ") is not divisible for " + tilesY, "Tiles", Program.LOG_TYPE.WARNING, pictureBox1);
            }
            for (var y = 0; y < bmp.PhysicalDimension.Height; y += 3)
            {
                for (var j = 0; j < tilesX; j++)
                {
                    bmp.SetPixel((int)((bmp.PhysicalDimension.Width / tilesX) * j), y, Color.Red);
                }
            }
            for (var x = 0; x < bmp.PhysicalDimension.Width; x += 3)
            {
                for (var j = 0; j < tilesY; j++)
                {
                    bmp.SetPixel(x, (int)((bmp.PhysicalDimension.Height / tilesY) * j), Color.Red);
                }
            }
            pb.Image = bmp;
        }

        private void richTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            var rtb = (RichTextBox)sender;
            List<int> accetptedChars = new List<int> {3, 8, 12, 16, 17, 18, 35, 36, 37, 38, 39, 40, 46, '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ' ', '+', '-', '*', '/', '(', ')', 96, 97, 98, 99, 100, 101, 102, 103, 104, 105, 106, 107, 108, 109, 110, 111 };
            if (!accetptedChars.Contains(e.KeyValue))
            {
                Point p = rtb.PointToScreen(rtb.GetPositionFromCharIndex(0));
                p.Y += 20;
                contextMenuStrip1.Tag = rtb;
                contextMenuStrip1.Show(p);
                e.Handled = true;
            }
            else if(e.KeyValue == 8)
            {
                if(rtb.SelectionLength > 0)
                {
                    return;
                }
                for (var j = 0; j < 10; j++)
                {
                    if (rtb.SelectionStart > 0)
                    {
                        rtb.SelectionStart--;
                        rtb.SelectionLength++;
                        rtb.SelectedText = "";
                    }
                    if (!rtb.SelectionProtected || rtb.SelectionStart==0)
                    {
                        if(j > 0)
                        {
                            if(rtb.SelectionStart > 0) rtb.SelectionStart++;
                            rtb.SelectionProtected = false;
                            rtb.SelectedText = "";
                        }
                        if (rtb.SelectionStart == 0 && rtb.Text == "")
                        {
                            rtb.SelectAll();
                            rtb.SelectionColor = Color.Black;
                            rtb.SelectionBackColor = Color.White;
                        }
                        break;
                    }
                }
                e.Handled = true;
            }
        }

        private void toolStripMenuItem_Click(object sender, EventArgs e)
        {
            RichTextBox rtb;
            if(contextMenuStrip1.Tag is RichTextBox)
            {
                rtb = (contextMenuStrip1.Tag as RichTextBox);
            }
            else
            {
                return;
            }
            int j = 0;
            List<string> keys = new List<string>() { "screenW", "screenH", "areaW", "areaH", "imageW", "imageH", "imageX", "imageY", "random", "randS" };
            for(;j<contextMenuStrip1.Items.Count;j++)
            {
                if(contextMenuStrip1.Items[j] == sender) break;
            }
            if(j < keys.Count)
            {
                var curPosition = rtb.SelectionStart;
                rtb.SelectedText = keys[j];

                rtb.SelectionStart = curPosition;
                rtb.SelectionLength = keys[j].Length;

                rtb.SelectionColor = Color.DarkBlue;
                rtb.SelectionBackColor = Color.Yellow;
                rtb.SelectionProtected = true;

                rtb.SelectionStart = curPosition + keys[j].Length;
                rtb.SelectionLength = 0;
            }

            toolStripMenuItem1.Tag = null;
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            if (pictureBox2.Tag == null) pictureBox2.Tag = -1;
            int val = int.Parse(pictureBox2.Tag.ToString()) + 1;
            if(listView1.Items.Count == 0)
            {
                timer1.Enabled = false;
                return;
            }
            if (val >= listView1.Items.Count) val = 0;
            var imageIndex = listView1.Items[val].ImageIndex;
            if (imageIndex < 0)
            {
                timer1.Enabled = false;
                return;
            }
            pictureBox2.Image = listView1.LargeImageList.Images[imageIndex];
            pictureBox2.Tag = val;
            int nextIndex = (int.Parse(timer1.Tag.ToString()) + 1) % stats.SubSteps.Count;
            timer1.Tag = nextIndex;
            timer1.Interval = stats.SubSteps[nextIndex].Interval;
            
        }

        private void contextMenuStrip3_Opening(object sender, CancelEventArgs e)
        {
            Point cp = listView1.PointToClient(new Point(MousePosition.X, MousePosition.Y));
            ListViewItem hoverItem = listView1.GetItemAt(cp.X, cp.Y);
            contextMenuStrip3.Tag = hoverItem;
            contextMenuStrip3.Items[1].Enabled = hoverItem != null;
        }

        private void removeFrameToolStripMenuItem_Click(object sender, EventArgs e)
        {
            listView1.Items.Remove((ListViewItem)contextMenuStrip3.Tag);
            animation_Edited(null, null);
        }

        private void toolStripMenuItem5_Click(object sender, EventArgs e)
        {
            var selection = new SelectFrameDialog();
            selection.SetImage(image.Image, Program.AnimationXML.Image.TilesX, Program.AnimationXML.Image.TilesY);
            if (selection.ShowDialog() == DialogResult.OK)
            {
                var index = selection.SelectedIndex;

                if (index >= 0)
                {
                    listView1.Items.Add(index + " (" + listView1.Items.Count + ")", index);
                }
                animation_Edited(null, null);
            }
        }

        private void contextMenuStrip2_Opening(object sender, CancelEventArgs e)
        {
            var cms = sender as ContextMenuStrip;
            var lv = cms.SourceControl as ListView;
            if(lv.SelectedIndices.Count == 0)
            {
                toolStripMenuItem7.Enabled = false;
                toolStripMenuItem6.Text = "ADD NEW";
                toolStripTextBox1.Text = "10";
                toolStripComboBox1.SelectedIndex = 0;
                toolStripComboBox2.SelectedIndex = -1;
            }
            else
            {
                toolStripMenuItem7.Enabled = true;
                toolStripMenuItem6.Text = "SAVE";
                toolStripTextBox1.Text = lv.SelectedItems[0].SubItems[2].Text;
                toolStripComboBox1.SelectedIndex = -1;
                foreach (var k in toolStripComboBox1.Items)
                {
                    if(k.ToString() == lv.SelectedItems[0].SubItems[3].Text)
                    {
                        toolStripComboBox1.SelectedItem = k;
                        break;
                    }
                }
                toolStripComboBox2.SelectedIndex = -1;
                foreach (var k in toolStripComboBox2.Items)
                {
                    if((k as XmlNodeCombo).Id.ToString() == lv.SelectedItems[0].SubItems[0].Text)
                    {
                        toolStripComboBox2.SelectedItem = k;
                        break;
                    }
                }
            }
        }

        // Add or Save next node
        private void toolStripMenuItem6_Click(object sender, EventArgs e)
        {
            var cms = contextMenuStrip2;
            var lv = cms.SourceControl as ListView;
            if (lv.SelectedIndices.Count == 0)
            {
                if (toolStripComboBox2.SelectedIndex >= 0 && toolStripComboBox1.SelectedIndex >= 0)
                {
                    var lvi = lv.Items.Add((toolStripComboBox2.SelectedItem as XmlNodeCombo).Id.ToString());
                    var n2 = XmlTools.GetXmlAnimationNode((toolStripComboBox2.SelectedItem as XmlNodeCombo).Id);
                    try
                    {
                        lvi.ImageIndex = n2.Sequence.Frame[0];
                    }
                    catch (Exception) { }
                    lvi.SubItems.Add((toolStripComboBox2.SelectedItem as XmlNodeCombo).Name);
                    lvi.SubItems.Add(toolStripTextBox1.Text);
                    lvi.SubItems.Add(toolStripComboBox1.Text);
                    lv.Height += 30;
                    lv.Parent.Height += 30;
                }
                else
                {
                    MessageBox.Show("Not all parameters are set.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                var lvi = lv.SelectedItems[0];
                lvi.SubItems[0].Text = (toolStripComboBox2.SelectedItem as XmlNodeCombo).Id.ToString();
                var n2 = XmlTools.GetXmlAnimationNode((toolStripComboBox2.SelectedItem as XmlNodeCombo).Id);
                try
                {
                    lvi.ImageIndex = n2.Sequence.Frame[0];
                }
                catch (Exception) { }
                lvi.SubItems[1].Text = (toolStripComboBox2.SelectedItem as XmlNodeCombo).Name;
                lvi.SubItems[2].Text = toolStripTextBox1.Text;
                lvi.SubItems[3].Text = toolStripComboBox1.Text;
            }
            animation_Edited(sender, e);
        }

        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if(tabControl1.SelectedTab.Name == "tabPage3")
            {
                EditSpawn(!(EditSpawnNode.Count > 1 && EditSpawnNodeIndex > 0), EditSpawnNodeIndex > 0, EditSpawnNodeIndex < EditSpawnNode.Count - 1);
            }
            else if (tabControl1.SelectedTab.Name == "tabPage4")
            {
                EditAnimation(!(EditAnimationNode.Count > 1 && EditAnimationNodeIndex > 0), EditAnimationNodeIndex > 0, EditAnimationNodeIndex < EditAnimationNode.Count - 1);
            }
            else if (tabControl1.SelectedTab.Name == "tabPage5")
            {
                EditChild(!(EditChildNode.Count > 1 && EditChildNodeIndex > 0), EditChildNodeIndex > 0, EditChildNodeIndex < EditChildNode.Count - 1);
            }
            else if (tabControl1.SelectedTab.Name == "tabPage6")
            {
                EditSound(!(EditSoundNode.Count > 1 && EditSoundNodeIndex > 0), EditSoundNodeIndex > 0, EditSoundNodeIndex < EditSoundNode.Count - 1);
            }
            else
            {
                DisableEditToolbar();
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        public void Undo()
        {
            if(tabControl1.SelectedTab.Name == "tabPage3")
            {
                if(EditSpawnNodeIndex > 0)
                {
                    EditSpawnNodeIndex--;
                    var node = EditSpawnNode[EditSpawnNodeIndex];
                    FillSpawnFormFromNode(node);
                    if(EditSpawnNodeIndex==0)
                    {
                        EditSpawn(true, false, true);
                        tabControl1.SelectedTab.Text = "Spawns";
                    }
                    else
                    {
                        EditSpawn(false, true, true);
                    }
                    UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage4")
            {
                if (EditAnimationNodeIndex > 0)
                {
                    EditAnimationNodeIndex--;
                    var node = EditAnimationNode[EditAnimationNodeIndex];
                    FillAnimationFormFromNode(node);
                    if (EditAnimationNodeIndex == 0)
                    {
                        EditAnimation(true, false, true);
                        tabControl1.SelectedTab.Text = "Animations";
                    }
                    else
                    {
                        EditAnimation(false, true, true);
                    }
                    //UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage5") // childs
            {
                if (EditChildNodeIndex > 0)
                {
                    EditChildNodeIndex--;
                    var node = EditChildNode[EditChildNodeIndex];
                    FillChildFormFromNode(node);
                    if (EditChildNodeIndex == 0)
                    {
                        EditChild(true, false, true);
                        tabControl1.SelectedTab.Text = "Childs";
                    }
                    else
                    {
                        EditChild(false, true, true);
                    }
                    //UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage6") // sounds
            {
                if (EditSoundNodeIndex > 0)
                {
                    EditSoundNodeIndex--;
                    var node = EditSoundNode[EditSoundNodeIndex];
                    FillSoundFormFromNode(node);
                    if (EditSoundNodeIndex == 0)
                    {
                        EditSound(true, false, true);
                        tabControl1.SelectedTab.Text = "Sounds";
                    }
                    else
                    {
                        EditSound(false, true, true);
                    }
                    //UpdateSpawnPreview(node);
                }
            }
        }
        public void Redo()
        {
            if (tabControl1.SelectedTab.Name == "tabPage3")
            {
                if (EditSpawnNodeIndex < EditSpawnNode.Count - 1)
                {
                    EditSpawnNodeIndex++;
                    var node = EditSpawnNode[EditSpawnNodeIndex];
                    FillSpawnFormFromNode(node);
                    if (EditSpawnNodeIndex > 0)
                    {
                        EditSpawn(false, true, EditSpawnNodeIndex < EditSpawnNode.Count - 1);
                        tabControl1.SelectedTab.Text = "Spawns *";
                    }
                    UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage4")
            {
                if (EditAnimationNodeIndex < EditAnimationNode.Count - 1)
                {
                    EditAnimationNodeIndex++;
                    var node = EditAnimationNode[EditAnimationNodeIndex];
                    FillAnimationFormFromNode(node);
                    if (EditAnimationNodeIndex > 0)
                    {
                        EditAnimation(false, true, EditAnimationNodeIndex < EditAnimationNode.Count - 1);
                        tabControl1.SelectedTab.Text = "Animations *";
                    }
                    //UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage5") // childs
            {
                if (EditChildNodeIndex < EditChildNode.Count - 1)
                {
                    EditChildNodeIndex++;
                    var node = EditChildNode[EditChildNodeIndex];
                    FillChildFormFromNode(node);
                    if (EditChildNodeIndex > 0)
                    {
                        EditChild(false, true, EditChildNodeIndex < EditChildNode.Count - 1);
                        tabControl1.SelectedTab.Text = "Childs *";
                    }
                    //UpdateSpawnPreview(node);
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage6") // sounds
            {
                if (EditSoundNodeIndex < EditSoundNode.Count - 1)
                {
                    EditSoundNodeIndex++;
                    var node = EditSoundNode[EditSoundNodeIndex];
                    FillSoundFormFromNode(node);
                    if (EditSoundNodeIndex > 0)
                    {
                        EditSound(false, true, EditSoundNodeIndex < EditSoundNode.Count - 1);
                        tabControl1.SelectedTab.Text = "Sounds *";
                    }
                    //UpdateSpawnPreview(node);
                }
            }
        }

        public void Save()
        {
            if (tabControl1.SelectedTab.Name == "tabPage3")
            {
                if (EditSpawnNodeIndex > 0)
                {
                    var node = EditSpawnNode[EditSpawnNodeIndex];
                    XmlTools.UpdateXmlSpawnNode(EditSpawnNode[0], EditSpawnNode[EditSpawnNodeIndex]);
                    EditSpawn(true, false, false);
                    tabControl1.SelectedTab.Text = "Spawns";
                    EditSpawnNodeIndex = -1;
                    EditSpawnNode.Clear();
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage4")
            {
                if (EditAnimationNodeIndex > 0)
                {
                    var node = EditAnimationNode[EditAnimationNodeIndex];
                    XmlTools.UpdateXmlAnimationNode(EditAnimationNode[0], EditAnimationNode[EditAnimationNodeIndex]);
                    EditAnimation(true, false, false);
                    tabControl1.SelectedTab.Text = "Animations";
                    EditAnimationNodeIndex = -1;
                    EditAnimationNode.Clear();
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage5")
            {
                if (EditChildNodeIndex > 0)
                {
                    var node = EditChildNode[EditChildNodeIndex];
                    XmlTools.UpdateXmlChildNode(EditChildNode[0], EditChildNode[EditChildNodeIndex]);
                    EditChild(true, false, false);
                    tabControl1.SelectedTab.Text = "Childs";
                    EditChildNodeIndex = -1;
                    EditChildNode.Clear();
                }
            }
            else if (tabControl1.SelectedTab.Name == "tabPage6")
            {
                if (EditSoundNodeIndex > 0)
                {
                    var node = EditSoundNode[EditSoundNodeIndex];
                    XmlTools.UpdateXmlSoundNode(int.Parse(comboBox6.Tag.ToString()), EditSoundNode[EditSoundNodeIndex]);
                    EditSound(true, false, false);
                    tabControl1.SelectedTab.Text = "Sounds";
                    EditSoundNodeIndex = -1;
                    EditSoundNode.Clear();
                }
            }

            MainWindow.MainWin.UpdateData();
        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        public void EditSpawn(bool editFinished, bool UndoAvailable, bool RedoAvailable)
        {
            MainWindow.MainWin.EditSpawn(editFinished);
            toolStripButtonSave.Enabled = !editFinished;
            toolStripButtonUndo.Enabled = UndoAvailable;
            toolStripButtonRedo.Enabled = RedoAvailable;
        }

        public void EditAnimation(bool editFinished, bool UndoAvailable, bool RedoAvailable)
        {
            MainWindow.MainWin.EditAnimation(editFinished);
            toolStripButtonSave.Enabled = !editFinished;
            toolStripButtonUndo.Enabled = UndoAvailable;
            toolStripButtonRedo.Enabled = RedoAvailable;
            splitContainer3.Panel1.AutoScrollPosition = new Point(0, 0);
        }

        public void EditChild(bool editFinished, bool UndoAvailable, bool RedoAvailable)
        {
            MainWindow.MainWin.EditChild(editFinished);
            toolStripButtonSave.Enabled = !editFinished;
            toolStripButtonUndo.Enabled = UndoAvailable;
            toolStripButtonRedo.Enabled = RedoAvailable;
        }

        public void EditSound(bool editFinished, bool UndoAvailable, bool RedoAvailable)
        {
            MainWindow.MainWin.EditSound(editFinished);
            toolStripButtonSave.Enabled = !editFinished;
            toolStripButtonUndo.Enabled = UndoAvailable;
            toolStripButtonRedo.Enabled = RedoAvailable;
        }

        public void DisableEditToolbar()
        {
            toolStripButtonSave.Enabled = false;
            toolStripButtonUndo.Enabled = false;
            toolStripButtonRedo.Enabled = false;
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            Undo();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            Redo();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (var fs = File.Open(openFileDialog1.FileName, FileMode.Open))
                {
                    if (fs.Length > 1024 * 200)
                    {
                        MessageBox.Show("This file is too big! (" + (fs.Length / 1024) + " kb), the maximal allowed size is 200kb", "MP3 size", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        byte[] fsa = new byte[fs.Length];
                        fs.Read(fsa, 0, (int)fs.Length);
                        textBox1.Text = Convert.ToBase64String(fsa);
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(textBox1.Text));
            AudioReader = new Mp3FileReader(ms);
            var Audio = new WaveOut();
            Audio.Init(AudioReader);
            progressBar1.Value = 0;
            Audio.Play();
            timer2.Tag = 1;
            timer2.Start();
        }

        private void timer2_Tick(object sender, EventArgs e)
        {
            if(AudioReader.Position >= AudioReader.Length)
            {
                int count = int.Parse(timer2.Tag.ToString()) - 1;
                timer2.Tag = count;
                if (count <= 0)
                {
                    timer2.Stop();
                    progressBar1.Value = 100;
                }
                else
                {
                    progressBar1.Value = 0;
                    AudioReader.Position = 0;
                }
            }
            else
            {
                progressBar1.Value = Math.Min(100, (int)((AudioReader.Position * 100 / AudioReader.Length) * 1.4));
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MemoryStream ms = new MemoryStream(Convert.FromBase64String(textBox1.Text));
            AudioReader = new Mp3FileReader(ms);
            var Audio = new WaveOut();
            Audio.Init(AudioReader);
            progressBar1.Value = 0;
            Audio.Play();
            timer2.Tag = 10;
            timer2.Start();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            timer2.Enabled = false;
        }

        private void toolStripMenuItem7_Click(object sender, EventArgs e)
        {
            var cms = contextMenuStrip2;
            var lv = cms.SourceControl as ListView;
            if (lv.SelectedIndices.Count > 0)
            {
                lv.Items.Remove(lv.SelectedItems[0]);
                animation_Edited(sender, e);
            }
        }
    }
}
