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

namespace PetEditor
{
    public partial class MainWindow : Form
    {
        public ProjectStructure formProject = null;
        public EditWindows editWindows = null;
        public Log log = null;
        public Tools xmlViewer = null;
        public static MainWindow MainWin;
        private List<string> Recent = new List<string>();
        private WebForm formChat = null;
        List<bool> mustSave = new List<bool> {false, false, false, false, false, false, false, false};
        bool xmlModified = false;

        public MainWindow()
        {
            InitializeComponent();
            MainWin = this;
        }

        public void EditSpawn(bool editFinished)
        {
            formProject.SpawnEditing(editFinished);
            mustSave[2] = !editFinished;
            if (!editFinished) XmlModified();
        }

        public void EditAnimation(bool editFinished)
        {
            formProject.AnimationEditing(editFinished);
            mustSave[3] = !editFinished;
            if (!editFinished) XmlModified();
        }

        public void EditChild(bool editFinished)
        {
            formProject.ChildEditing(editFinished);
            mustSave[4] = !editFinished;
            if (!editFinished) XmlModified();
        }

        public void EditSound(bool editFinished)
        {
            formProject.SoundEditing(editFinished);
            mustSave[5] = !editFinished;
            if(!editFinished) XmlModified();
        }

        private void XmlModified(bool saved = false)
        {
            if(saved)
            {
                this.Text = this.Text.Replace(" *", "");
                xmlModified = false;
            }
            else if(this.Text.IndexOf(" *") < 0)
            {
                this.Text += " *";
                xmlModified = true;
            }
        }

        private void ShowNewForm(object sender, EventArgs e)
        {
            bool messageshow = false;
            foreach (var k in mustSave)
            {
                if (k)
                {
                    messageshow = true;
                    if (MessageBox.Show("There are still nodes not saved. Do you really want dismiss your changes?", "save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        return;
                    }
                    for (var j = 0; j < mustSave.Count; j++)
                    {
                        mustSave[j] = false;
                    }
                    break;
                }
            }
            if(!messageshow && xmlModified)
            {
                if (MessageBox.Show("Current project is not saved, do you want close without saving the changes?", "save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    return;
                }
            }

            toolStripButton1.Enabled = true;
            toolStripButton2.Enabled = true;

            if (formProject != null)
            {
                formProject.Hide();
                editWindows.Hide();
                log.Hide();
                formProject.Close();
                editWindows.Close();
                log.Close();
            }

            richTextBox1.Hide();
            formChat.Hide();

            editWindows = new EditWindows();
            editWindows.MdiParent = this;
            editWindows.Parent = splitContainer2.Panel1;
            editWindows.Dock = DockStyle.Fill;
            editWindows.Show();

            log = new Log();
            log.MdiParent = this;
            log.Parent = splitContainer2.Panel2;
            log.Dock = DockStyle.Fill;
            log.Show();

            formProject = new ProjectStructure();
            formProject.MdiParent = this;
            formProject.Parent = splitContainer1.Panel1;
            formProject.Dock = DockStyle.Fill;
            formProject.Show();

            xmlViewer = new Tools();
            xmlViewer.MdiParent = this;
            xmlViewer.Parent = splitContainer2.Panel1;
            xmlViewer.Dock = DockStyle.Fill;

            Program.LogForm = log;

            XmlTools.LoadXML("");

            XmlTools.FillMissingDataOnXML();

            UpdateData();

            if(e != null)
            {
                UpdateData();
            }

            for (var k = 0; k < 8; k++) mustSave[k] = false;
            XmlModified(true);
        }

        private void OpenFile(object sender, EventArgs e)
        {
            
        }

        public void UpdateData()
        {
            Enabled = false;

            editWindows.UpdateData();
            formProject.UpdateData();

            Enabled = true;
        }

        private void SaveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
        }

        private void ExitToolsStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void CutToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void CopyToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void PasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
        }

        private void ToolBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            toolStrip.Visible = toolBarToolStripMenuItem.Checked;
        }

        private void StatusBarToolStripMenuItem_Click(object sender, EventArgs e)
        {
            statusStrip.Visible = statusBarToolStripMenuItem.Checked;
        }

        private void TileVerticalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileVertical);
        }

        private void TileHorizontalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LayoutMdi(MdiLayout.TileHorizontal);
        }


        private void CloseAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (Form childForm in MdiChildren)
            {
                childForm.Close();
            }
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            richTextBox1.Rtf = "{\\rtf1\\ansi\\ansicpg1252\\deff0\\nouicompat\\deflang2055{\\fonttbl{\\f0\\fnil\\fcharset0 Calibri;}{\\f1\\fnil\\fcharset2 Symbol;}}{\\colortbl;\\red0\\green0\\blue255;}{\\*\\generator Riched20 10.0.17134}\\viewkind4\\uc1\\pard\\sl240\\slmult1\\b\\f0\\fs24\\lang7 Welcome to the Pet Editor!\\b0\\fs20\\par This editor is still in beta, so if you get an error, please report it.\\par You can use the chat inside this app(it use a webpage) or put the question on GitHub:\\par {{\\field{\\*\\fldinst{HYPERLINK https://github.com/Adrianotiger/desktopPet }}{\\fldrslt{https://github.com/Adrianotiger/desktopPet\\ul0\\cf0}}}}\\f0\\fs20\\par\\b\\fs24 How to start?\\b0\\fs20\\par\\pard{\\pntext\\f1\\'B7\\tab}{\\*\\pn\\pnlvlblt\\pnf1\\pnindent0{\\pntxtb\\'B7}}\\fi-360\\li720\\sl240\\slmult1 Click on Open or New to begin.\\par{\\pntext\\f1\\'B7\\tab}Fill the fields on \\b Header\\b0\\par{\\pntext\\f1\\'B7\\tab}Set an image on \\b Image\\b0\\par{\\pntext\\f1\\'B7\\tab}Add an animation with some frames\\par{\\pntext\\f1\\'B7\\tab}Add a spawn\\par{\\pntext\\f1\\'B7\\tab}Add more animations if you want\\par{\\pntext\\f1\\'B7\\tab}Add some childs (optional)\\par{\\pntext\\f1\\'B7\\tab}Add some sounds (optional)\\par\\pard\\sa200\\sl276\\slmult1\\fs22\\par}";
            Text = Application.ProductName + " - ver." + Application.ProductVersion;
            Recent.AddRange(Properties.Settings.Default.recent.Split(';'));
            if (Recent.Count > 0)
            {
                for (var k = 0; k < Recent.Count; k++)
                {
                    if (Recent[k].Length > 5)
                    {
                        var menuText = Recent[k];
                        var pos = menuText.LastIndexOf("\\");
                        if (pos > 0)
                        {
                            pos = menuText.LastIndexOf("\\", pos - 1);
                            if (pos > 0)
                            {
                                menuText = menuText.Substring(pos);
                            }
                        }
                        ToolStripItem tsi = new ToolStripMenuItem(menuText, newToolStripButton.Image);
                        tsi.Tag = Recent[k];
                        tsi.Click += (s, ev) =>
                        {
                            OpenFile((s as ToolStripMenuItem).Tag.ToString());
                        };
                        openToolStripButton.DropDownItems.Add(tsi);
                    }
                }
            }

            formChat = new WebForm();
            formChat.MdiParent = this;
            formChat.Parent = splitContainer2.Panel1;
            formChat.Dock = DockStyle.Fill;
            formChat.Show();
        }

        private void splitContainer2_Panel1_Paint(object sender, PaintEventArgs e)
        {

        }

        private void saveToolStripButton_Click(object sender, EventArgs e)
        {
        }

        public void SelectSection(string section)
        {
            List<string> editWins = new List<string> { "header", "image", "spawns", "animations", "childs", "sounds" };
            if (editWins.Contains(section))
            {
                editWindows.SelectSection(section);
                if (!editWindows.Visible)
                {
                    xmlViewer.Hide();
                    editWindows.Show();
                }
            }
            else
            {
                xmlViewer.SelectSection(section);
                if (editWindows.Visible)
                {
                    xmlViewer.Show();
                    editWindows.Hide();
                }
            }

            if(section == "view xml")
            {
                xmlViewer.LoadXML();
            }
        }

        public bool SetSpawnEdit(int spawnId)
        {
            return editWindows.SetSpawnEdit(spawnId);
        }

        public void SetAnimationEdit(int animationId)
        {
            editWindows.SetAnimationEdit(animationId);
        }

        public bool SetChildEdit(int childId)
        {
            return editWindows.SetChildEdit(childId);
        }

        public bool SetSoundEdit(int soundIndex)
        {
            return editWindows.SetSoundEdit(soundIndex);
        }

        private void toolStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void nodeSaveStripButton_Click(object sender, EventArgs e)
        {
            
        }

        private void nodeRedoStripButton_Click(object sender, EventArgs e)
        {
        }

        private void toolStripLabel2_Click(object sender, EventArgs e)
        {
            
        }

        private void toolStripButton1_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            saveFileDialog.Filter = "Animations (animations.xml)|*.xml";
            if (saveFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                string FileName = saveFileDialog.FileName;

                string xmlString = XmlTools.GenerateXmlString();

                File.WriteAllText(FileName, xmlString);

                for(var k=0;k< mustSave.Count;k++)
                {
                    mustSave[k] = false;
                }
                XmlModified(true);
            }
        }

        private void toolStripButton2_Click(object sender, EventArgs e)
        {
            if (editWindows == null || xmlViewer == null) return;
            editWindows.Hide();
            xmlViewer.Show();
            formProject.XmlView();
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            //openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Personal);
            openFileDialog.RestoreDirectory = true;
            openFileDialog.Filter = "Animations (animations.xml)|animation?.xml|All XML files (*.xml)|*.xml";
            if (openFileDialog.ShowDialog(this) == DialogResult.OK)
            {
                OpenFile(openFileDialog.FileName);
            }
        }

        private void OpenFile(string FileName)
        {
            bool saveSettings = false;
            Enabled = false;

            if (Recent.Contains(FileName))
            {
                if (Recent.IndexOf(FileName) > 0)
                {
                    Recent.RemoveAt(Recent.IndexOf(FileName));
                    Recent.Insert(0, FileName);
                    saveSettings = true;
                }
            }
            else
            {
                Recent.Insert(0, FileName);
                saveSettings = true;
            }
            if (saveSettings)
            {
                Properties.Settings.Default.recent = String.Join(";", Recent);
                Properties.Settings.Default.Save();
            }

            ShowNewForm(null, null);

            XmlTools.LoadXML(FileName);

            XmlTools.FillMissingDataOnXML();

            UpdateData();
        }

        private void toolStripButton3_Click(object sender, EventArgs e)
        {
            if (editWindows != null) editWindows.Hide();
            if (xmlViewer != null) xmlViewer.Hide();

            if (!formChat.IsHandleCreated)
            {
                formChat = new WebForm();
                formChat.MdiParent = this;
                formChat.Parent = splitContainer2.Panel1;
                formChat.Dock = DockStyle.Fill;
            }
            
            formChat.Show();
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            bool messageshow = false;
            foreach(var k in mustSave)
            {
                if(k)
                {
                    messageshow = true;
                    if(MessageBox.Show("There is still a node that is not saved. Do you want close the editor without saving your project?", "save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                    {
                        e.Cancel = true;
                    }
                    break;
                }
            }
            if(!messageshow && xmlModified)
            {
                if (MessageBox.Show("Project is not saved, do you really want close the application without save?", "save", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question) != DialogResult.Yes)
                {
                    e.Cancel = true;
                }
            }
        }

        private void toolStripButton4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://github.com/Adrianotiger/desktopPet/wiki");
        }
    }
}
