using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PetEditor
{
    public partial class ProjectStructure : Form
    {
        private List<int> spawnsList = new List<int>();
        private List<int> animationsList = new List<int>();
        private List<int> childsList = new List<int>();
        private List<int> soundsList = new List<int>();

        public ProjectStructure()
        {
            InitializeComponent();
        }

        private void ProjectStructure_Load(object sender, EventArgs e)
        {
            treeView1.ExpandAll();
        }
        
        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var selected = e.Node.Text;
            var selText = selected.ToLower().Replace(" *", "");
            MainWindow.MainWin.SelectSection(selText);

            switch(selText)
            {
                case "spawns": tabControl1.SelectedIndex = 0; break;
                case "animations": tabControl1.SelectedIndex = 1; break;
                case "childs": tabControl1.SelectedIndex = 2; break;
                case "sounds": tabControl1.SelectedIndex = 3; break;
            }
        }

        public void SpawnEditing(bool finished = false)
        {
            if (finished)
            {
                treeView1.Nodes[0].Nodes["Node3"].Text = "Spawns";
                //tabControl1.TabPages["tabPage3"].Text = "Spawns";
            }
            else
            {
                treeView1.Nodes[0].Nodes["Node3"].Text = "Spawns *";
                
            }
        }

        public void AnimationEditing(bool finished = false)
        {
            if (finished)
            {
                treeView1.Nodes[0].Nodes["Node4"].Text = "Animations";
                //tabControl1.TabPages["tabPage3"].Text = "Spawns";
            }
            else
            {
                treeView1.Nodes[0].Nodes["Node4"].Text = "Animations *";
            }
        }

        public void ChildEditing(bool finished = false)
        {
            if (finished)
            {
                treeView1.Nodes[0].Nodes["Node5"].Text = "Childs";
                //tabControl1.TabPages["tabPage3"].Text = "Spawns";
            }
            else
            {
                treeView1.Nodes[0].Nodes["Node5"].Text = "Childs *";
            }
        }

        public void SoundEditing(bool finished = false)
        {
            if (finished)
            {
                treeView1.Nodes[0].Nodes["Node6"].Text = "Sounds";
                //tabControl1.TabPages["tabPage3"].Text = "Spawns";
            }
            else
            {
                treeView1.Nodes[0].Nodes["Node6"].Text = "Sounds *";
            }
        }

        public void XmlView()
        {
            treeView1.SelectedNode = treeView1.Nodes[1].Nodes["Node11"];
        }

        public void UpdateData()
        {
            UpdateSpawns();
            UpdateAnimations();
            UpdateChilds();
            UpdateSounds();
        }

        private void UpdateSpawns()
        {
            var s = Program.AnimationXML.Spawns;
            listView1.Items.Clear();
            if (s == null || s.Spawn == null) return;
            int tot = 0;
            spawnsList.Clear();
            foreach (var n in s.Spawn)
            {
                spawnsList.Add(n.Id);
                var lv = listView1.Items.Add(" Spawn #" + n.Id);
                lv.ImageIndex = 0;
                tot += n.Probability;
            }
            int lvindex = 0;
            foreach (var n in s.Spawn)
            {
                listView1.Items[lvindex++].Text += " (" + ((int)(n.Probability * 1000 / tot) / 10).ToString() + "%)";
            }
        }

        private void listView1_SelectedIndexChanged_1(object sender, EventArgs e)
        {
            if (listView1.SelectedIndices.Count == 0) return;

            MainWindow.MainWin.SelectSection("spawns");
            if(!MainWindow.MainWin.SetSpawnEdit(spawnsList[listView1.SelectedIndices[0]]))
            {
                
            }
        }

        private void UpdateAnimations()
        {
            var a = Program.AnimationXML.Animations;
            listView2.Items.Clear();
            if (a == null || a.Animation == null) return;

            listView2.SmallImageList = XmlTools.AnimationIcons;
            animationsList.Clear();
            foreach (var n in a.Animation)
            {
                animationsList.Add(n.Id);
                var lv = listView2.Items.Add(n.Id + " - " + n.Name);
                try
                {
                    lv.ImageIndex = n.Sequence.Frame[0];
                }
                catch (Exception) { }
            }
        }
        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView2.SelectedIndices.Count == 0) return;

            MainWindow.MainWin.SelectSection("animations");
            MainWindow.MainWin.SetAnimationEdit(animationsList[listView2.SelectedIndices[0]]);
        }

        private void UpdateChilds()
        {
            var c = Program.AnimationXML.Childs;
            listView3.Items.Clear();
            if (c == null || c.Child == null) return;
            listView3.SmallImageList = XmlTools.AnimationIcons;
            childsList.Clear();
            foreach (var n in c.Child)
            {
                childsList.Add(n.Id);
                var lv = listView3.Items.Add(" Child #" + n.Id + " - " + XmlTools.GetXmlAnimationNode(n.Id).Name);
                lv.ImageIndex = XmlTools.GetXmlAnimationNode(n.Next).Sequence.Frame[0];
            }
        }
        private void listView3_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView3.SelectedIndices.Count == 0) return;

            MainWindow.MainWin.SelectSection("childs");
            if (!MainWindow.MainWin.SetChildEdit(childsList[listView3.SelectedIndices[0]]))
            {

            }
        }

        private void UpdateSounds()
        {
            var s = Program.AnimationXML.Sounds;
            listView4.Items.Clear();
            if (s == null || s.Sound == null) return;
            soundsList.Clear();
            listView4.SmallImageList = XmlTools.AnimationIcons;
            int tot = 0;
            foreach (var n in s.Sound)
            {
                soundsList.Add(n.Id);
                var lv = listView4.Items.Add(" Sound #" + n.Id + " - " + XmlTools.GetXmlAnimationNode(n.Id).Name);
                lv.ImageIndex = n.Id;
                tot += n.Probability;
            }
            if (tot == 0) tot = 1;
            int lvindex = 0;
            foreach (var n in s.Sound)
            {
                listView4.Items[lvindex++].Text += " (" + ((int)(n.Probability * 1000 / tot) / 10).ToString() + "%)";
            }
        }
        private void listView4_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (listView4.SelectedIndices.Count == 0) return;

            MainWindow.MainWin.SelectSection("sounds");
            if (!MainWindow.MainWin.SetSoundEdit(soundsList[listView4.SelectedIndices[0]]))
            {

            }
        }

        private void insertNodeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            treeView1.SelectedNode = treeView1.Nodes[1].Nodes[0];
            var cms = contextMenuStrip1;
            var lv = cms.SourceControl as ListView;
            if(lv == listView1) // spawn
            {
                var node = new XmlData.SpawnNode();
                for(var j=1;j<=listView1.Items.Count+2;j++)
                {
                    if(XmlTools.GetXmlSpawnNode(j) == null)
                    {
                        node.Id = j;
                        node.Next = new XmlData.NextNode();
                        node.Probability = 10;
                        node.X = "";
                        node.Y = "";
                        XmlTools.UpdateXmlSpawnNode(null, node);
                        MainWindow.MainWin.UpdateData();
                        break;
                    }
                }
            }
            else if(lv == listView2) // animations
            {
                var node = new XmlData.AnimationNode();
                for (var j = 1; j <= listView2.Items.Count + 2; j++)
                {
                    if (XmlTools.GetXmlAnimationNode(j) == null)
                    {
                        node.Id = j;
                        node.Name  = "New";
                        XmlTools.UpdateXmlAnimationNode(null, node);
                        MainWindow.MainWin.UpdateData();
                        break;
                    }
                }
            }
            else if (lv == listView3) // childs
            {
                var node = new XmlData.ChildNode();
                for (var j = 1; j <= listView3.Items.Count + 2; j++)
                {
                    if (XmlTools.GetXmlChildNode(j) == null)
                    {
                        node.Id = j;
                        node.Next = j;
                        XmlTools.UpdateXmlChildNode(null, node);
                        MainWindow.MainWin.UpdateData();
                        break;
                    }
                }
            }
            else if (lv == listView4) // sounds
            {
                var node = new XmlData.SoundNode();
                for (var j = 1; j <= listView4.Items.Count + 2; j++)
                {
                    if (XmlTools.GetXmlSoundNode(j) == null)
                    {
                        node.Id = j;
                        XmlTools.UpdateXmlSoundNode(null, node);
                        MainWindow.MainWin.UpdateData();
                        break;
                    }
                }
            }
        }

        
    }
}
