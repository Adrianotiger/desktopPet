using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
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

        public Tools()
        {
            InitializeComponent();
        }

        private void XmlViewer_Load(object sender, EventArgs e)
        {

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
                        richTextBox1.Text = xmlString;

                        lineIndexes = Regex.Matches(richTextBox1.Text, "\n").Cast<Match>().Select(m => m.Index);

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
    }
}
