using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace PetEditor
{
    class XmlTools
    {
        public static ImageList AnimationImages = new ImageList();
        public static ImageList AnimationIcons = new ImageList();

        public static bool LoadXML(string fileName)
        {
            if (fileName == "")
            {
                Program.AnimationXML = new XmlData.RootNode();
                return true;
            }
            bool bError = false;
            // Construct an instance of the XmlSerializer with the type
            // of object that is being deserialized.
            XmlSerializer mySerializer = new XmlSerializer(typeof(XmlData.RootNode));
            // To read the file, create a FileStream.
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            // Try to load local XML
            try
            {
                var AnimationXMLString = File.ReadAllText(fileName);
                if (AnimationXMLString.IndexOf("http://esheep.petrucci.ch") > 0)
                {
                    Program.AddLog("Old format (http) found as namespace, replacing it with https.", "LOAD FILE", false, true);
                    AnimationXMLString = AnimationXMLString.Replace("http://esheep.petrucci.ch", "https://esheep.petrucci.ch");
                }
                writer.Write(AnimationXMLString);
                writer.Flush();
                stream.Position = 0;
                // Call the Deserialize method and cast to the object type.
                Program.AnimationXML = (XmlData.RootNode)mySerializer.Deserialize(stream);
                
                stream.Close();
                Program.AddLog("XML file loaded: " + fileName, "LOAD FILE");
            }
            catch (Exception ex)
            {
                Program.AddLog("Unable to load XML file " + fileName, "LOAD FILE", true);
                MessageBox.Show("User XML error: " + ex.ToString(), "XML Animation error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                if (Program.AnimationXML == null)
                {
                    Program.AnimationXML = new XmlData.RootNode();
                }
            }

            if (bError)
            {
                MessageBox.Show("Error, can't load animations file. The original pet will be loaded", "XML error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return !bError;
        }

        public static void GenerateImageIcons(Image img, int TilesX, int TilesY)
        {
            AnimationImages.Images.Clear();
            AnimationIcons.Images.Clear();
            int w = Math.Min(256, (int)(img.PhysicalDimension.Width / TilesX));
            int h = Math.Min(256, (int)(img.PhysicalDimension.Height / TilesY));
            AnimationImages.ImageSize = new Size(w, h);
            AnimationIcons.ImageSize = new Size(32, 32);

            var stream = new MemoryStream();
            img.Save(stream, img.RawFormat);
            
            for (var y = 0; y < TilesY; y++)
            {
                for (var x = 0; x < TilesX; x++)
                {
                    Bitmap bmp = new Bitmap(w, h);
                    Graphics graph = Graphics.FromImage(bmp);
                    graph.DrawImage(img, new Rectangle(0, 0, w, h), x * w, y * h, w, h, GraphicsUnit.Pixel);
                    AnimationImages.Images.Add(bmp);

                    Bitmap bmp2 = new Bitmap(32, 32);
                    Graphics graph2 = Graphics.FromImage(bmp2);
                    graph2.DrawImage(img, new Rectangle(0, 0, 32, 32), x * w, y * h, w, h, GraphicsUnit.Pixel);
                    AnimationIcons.Images.Add(bmp2);
                }
            }
        }

        public static String GenerateXmlString()
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);

            XmlSerializer mySerializer = new XmlSerializer(typeof(XmlData.RootNode));

            // set some default values:
            var ns = new XmlSerializerNamespaces();
            ns.Add("", "https://esheep.petrucci.ch/");
            ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
            ns.Add("xsd", "https://esheep.petrucci.ch/ https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Resources/animations.xsd");
            ns.Add("schemaLocation", "https://esheep.petrucci.ch/ https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Resources/animations.xsd");

            mySerializer.Serialize(writer, Program.AnimationXML, ns);

            stream.Seek(0, SeekOrigin.Begin);

            StreamReader reader = new StreamReader(stream);
            string sXml = reader.ReadToEnd();

            sXml = sXml.Replace("<icon>", "<icon><![CDATA[");
            sXml = sXml.Replace("</icon>", "]]></icon>");
            sXml = sXml.Replace("<png>", "<png><![CDATA[");
            sXml = sXml.Replace("</png>", "]]></png>");

            return sXml;
            /*
            var x = Program.AnimationXML;
            string r = "<?xml version='1.0'?><animations xmlns='https://esheep.petrucci.ch/' xmlns:xsi='http://www.w3.org/2001/XMLSchema-instance' xsi:schemaLocation='https://esheep.petrucci.ch/ https://raw.githubusercontent.com/Adrianotiger/desktopPet/master/Resources/animations.xsd'>";
            r += "<header>";
            r += "<author>" + x.Header.Author + "</author>";
            r += "</header>";
            r += "</animations>";
            return r;
            */
        }

        static public XmlNodeCombo[] GetAnimationList()
        {
            List<XmlNodeCombo> animations = new List<XmlNodeCombo>();
            if (Program.AnimationXML.Animations != null && Program.AnimationXML.Animations.Animation != null)
            {
                foreach (var a in Program.AnimationXML.Animations.Animation)
                {
                    animations.Add(new XmlNodeCombo { Id = a.Id, Name = a.Name });
                }
            }
            return animations.ToArray();
        }

        static public XmlData.SpawnNode GetXmlSpawnNode(int id)
        {
            XmlData.SpawnNode node = null;
            if (Program.AnimationXML.Spawns != null && Program.AnimationXML.Spawns.Spawn != null)
            {
                foreach (var n in Program.AnimationXML.Spawns.Spawn)
                {
                    if (n.Id == id) return n;
                }
            }

            return node;
        }

        static public void UpdateXmlSpawnNode(XmlData.SpawnNode oldNode, XmlData.SpawnNode newNode)
        {
            if (Program.AnimationXML.Spawns == null || Program.AnimationXML.Spawns.Spawn == null || oldNode == null)
            {
                var newNodes = new XmlData.SpawnNode[1];
                newNodes[0] = newNode;
                if (Program.AnimationXML.Spawns == null) Program.AnimationXML.Spawns = new XmlData.SpawnsNode();
                if (Program.AnimationXML.Spawns.Spawn == null) Program.AnimationXML.Spawns.Spawn = new XmlData.SpawnNode[0];
                Program.AnimationXML.Spawns.Spawn = Program.AnimationXML.Spawns.Spawn.Concat(newNodes).ToArray();
            }
            else
            {
                foreach (var n in Program.AnimationXML.Spawns.Spawn)
                {
                    if (n.Id == oldNode.Id)
                    {
                        n.Id = newNode.Id;
                        n.Next = newNode.Next;
                        n.Probability = newNode.Probability;
                        n.X = newNode.X;
                        n.Y = newNode.Y;
                        break;
                    }
                }
            }
        }

        static public void FillMissingDataOnXML()
        {
            if(Program.AnimationXML.Animations == null)
                Program.AnimationXML.Animations = new XmlData.AnimationsNode();
            if (Program.AnimationXML.Animations.Animation == null)
                Program.AnimationXML.Animations.Animation = new XmlData.AnimationNode[0];
            if (Program.AnimationXML.Childs == null)
                Program.AnimationXML.Childs = new XmlData.ChildsNode();
            if (Program.AnimationXML.Childs.Child == null)
                Program.AnimationXML.Childs.Child = new XmlData.ChildNode[0];
            if (Program.AnimationXML.Header == null)
                Program.AnimationXML.Header = new XmlData.HeaderNode();
            if (Program.AnimationXML.Image == null)
                Program.AnimationXML.Image = new XmlData.ImageNode();
            if (Program.AnimationXML.Spawns == null)
                Program.AnimationXML.Spawns = new XmlData.SpawnsNode();
            if (Program.AnimationXML.Spawns.Spawn == null)
                Program.AnimationXML.Spawns.Spawn = new XmlData.SpawnNode[0];

            if (!int.TryParse(Program.AnimationXML.Header.Application, out int appVer)) Program.AnimationXML.Header.Application = "1";
            if (Program.AnimationXML.Header.Icon == null || Program.AnimationXML.Header.Icon.Length < 20) Program.AnimationXML.Header.Icon = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+P+/HgAFhAJ/wlseKgAAAABJRU5ErkJggg==";
            if (Program.AnimationXML.Header.Author == null || Program.AnimationXML.Header.Author == "") Program.AnimationXML.Header.Author = "Author name";
            if (Program.AnimationXML.Header.Info == null || Program.AnimationXML.Header.Info == "") Program.AnimationXML.Header.Info = "Info about pet";
            if (Program.AnimationXML.Header.Petname == null || Program.AnimationXML.Header.Petname == "") Program.AnimationXML.Header.Petname = "Pet name";
            if (Program.AnimationXML.Header.Title == null || Program.AnimationXML.Header.Title == "") Program.AnimationXML.Header.Title = "Pet title";
            if (Program.AnimationXML.Header.Version == null || Program.AnimationXML.Header.Version == "") Program.AnimationXML.Header.Version = "0.1";

            if(Program.AnimationXML.Image.Png == null || Program.AnimationXML.Image.Png.Length < 20) Program.AnimationXML.Image.Png = "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNk+P+/HgAFhAJ/wlseKgAAAABJRU5ErkJggg==";
            if (Program.AnimationXML.Image.TilesX <= 0) Program.AnimationXML.Image.TilesX = 1;
            if (Program.AnimationXML.Image.TilesY <= 0) Program.AnimationXML.Image.TilesY = 1;
            if (Program.AnimationXML.Image.Transparency == null || Program.AnimationXML.Image.Transparency == "") Program.AnimationXML.Image.Transparency = "Magenta";

            for(var k=0;k<Program.AnimationXML.Animations.Animation.Length;k++)
            {
                if(Program.AnimationXML.Animations.Animation[k].Gravity != null && (Program.AnimationXML.Animations.Animation[k].Gravity.Next == null || Program.AnimationXML.Animations.Animation[k].Gravity.Next.Length == 0))
                {
                    Program.AnimationXML.Animations.Animation[k].Gravity = null;
                }
                if (Program.AnimationXML.Animations.Animation[k].Border != null && (Program.AnimationXML.Animations.Animation[k].Border.Next == null || Program.AnimationXML.Animations.Animation[k].Border.Next.Length == 0))
                {
                    Program.AnimationXML.Animations.Animation[k].Border = null;
                }
                if (Program.AnimationXML.Animations.Animation[k].Sequence != null && (Program.AnimationXML.Animations.Animation[k].Sequence.Next == null || Program.AnimationXML.Animations.Animation[k].Sequence.Next.Length == 0))
                {
                    Program.AnimationXML.Animations.Animation[k].Sequence.Next = null;
                }
            }
            if (Program.AnimationXML.Sounds != null && Program.AnimationXML.Sounds.Sound != null)
            {
                for (var k = 0; k < Program.AnimationXML.Sounds.Sound.Length; k++)
                {
                    if(Program.AnimationXML.Sounds.Sound[k].Base64.IndexOf("64,") > 0)
                    {
                        Program.AnimationXML.Sounds.Sound[k].Base64 =
                            Program.AnimationXML.Sounds.Sound[k].Base64.Remove(0, Program.AnimationXML.Sounds.Sound[k].Base64.IndexOf("64,") + 3);
                    }
                }
            }
        }

        static public void UpdateXmlAnimationNode(XmlData.AnimationNode oldNode, XmlData.AnimationNode newNode)
        {
            if (Program.AnimationXML.Animations == null || Program.AnimationXML.Animations.Animation == null || oldNode == null)
            {
                var newNodes = new XmlData.AnimationNode[1];
                if (newNode.Start == null)
                {
                    newNode.Start = new XmlData.MovingNode();
                    newNode.Start.X = "0";
                    newNode.Start.Y = "0";
                    newNode.Start.Interval = "100";
                    newNode.Start.OffsetY = 0;
                    newNode.Start.Opacity = 1.0;
                }
                if (newNode.End == null)
                {
                    newNode.End = new XmlData.MovingNode();
                    newNode.End.X = "0";
                    newNode.End.Y = "0";
                    newNode.End.Interval = "100";
                    newNode.End.OffsetY = 0;
                    newNode.End.Opacity = 1.0;
                }
                if (newNode.Sequence == null)
                {
                    newNode.Sequence = new XmlData.SequenceNode();
                    newNode.Sequence.RepeatCount = "0";
                    newNode.Sequence.RepeatFromFrame = 0;
                }
                if (newNode.Sequence.Frame == null) newNode.Sequence.Frame = new int[0];
                newNodes[0] = newNode;
                if (Program.AnimationXML.Animations == null) Program.AnimationXML.Animations = new XmlData.AnimationsNode();
                if (Program.AnimationXML.Animations.Animation == null) Program.AnimationXML.Animations.Animation = new XmlData.AnimationNode[0];
                Program.AnimationXML.Animations.Animation = Program.AnimationXML.Animations.Animation.Concat(newNodes).ToArray();
            }
            else
            {
                for(var k=0;k<Program.AnimationXML.Animations.Animation.Length;k++)
                {
                    if (Program.AnimationXML.Animations.Animation[k].Id == oldNode.Id)
                    {
                        Program.AnimationXML.Animations.Animation[k] = newNode;
                        break;
                    }
                }
            }
        }

        static public XmlData.AnimationNode GetXmlAnimationNode(int id)
        {
            XmlData.AnimationNode node = null;
            if (Program.AnimationXML.Animations != null && Program.AnimationXML.Animations.Animation != null)
            {
                foreach (var n in Program.AnimationXML.Animations.Animation)
                {
                    if (n.Id == id) return n;
                }
            }

            return node;
        }

        static public List<int> GetAnimationsId()
        {
            List<int> ret = new List<int>();
            if (Program.AnimationXML.Animations != null && Program.AnimationXML.Animations.Animation != null)
            {
                foreach (var n in Program.AnimationXML.Animations.Animation)
                {
                    ret.Add(n.Id);
                }
                //ret.Sort();
            }
            return ret;
        }

        static public AnimationStatistics GetAnimationStatistics(XmlData.AnimationNode node, int spriteWidth, int spriteHeight, int forceRandomTo = -1)
        {
            var stat = new AnimationStatistics();
            bool variableRand = false;
            bool variableScreen = false;

            stat.Frames = node.Sequence.Frame.Length;
            if (node.Sequence.RepeatCount.IndexOf("screen") >= 0 || node.Sequence.RepeatCount.IndexOf("area") >= 0) variableScreen = true;
            if (node.Sequence.RepeatCount.IndexOf("random") >= 0) variableRand = true;
            stat.Repeats = EvalValue(node.Sequence.RepeatCount, spriteWidth, spriteHeight, 0, 0, 50);
            stat.RepeatFrom = node.Sequence.RepeatFromFrame;
            stat.TotalFrames = stat.Frames + (stat.Frames - stat.RepeatFrom) * stat.Repeats;

            stat.Start = new AnimationStatistics.StepValues();
            if (node.Start.X.IndexOf("screen") >= 0 || node.Start.X.IndexOf("area") >= 0) variableScreen = true;
            if (node.Start.X.IndexOf("random") >= 0) variableRand = true;
            stat.Start.X = EvalValue(node.Start.X, spriteWidth, spriteHeight, 0, 0, 50);
            if (node.Start.Y.IndexOf("screen") >= 0 || node.Start.Y.IndexOf("area") >= 0) variableScreen = true;
            if (node.Start.Y.IndexOf("random") >= 0) variableRand = true;
            stat.Start.Y = EvalValue(node.Start.Y, spriteWidth, spriteHeight, 0, 0, 50);
            if (node.Start.Interval.IndexOf("screen") >= 0 || node.Start.Interval.IndexOf("area") >= 0) variableScreen = true;
            if (node.Start.Interval.IndexOf("random") >= 0) variableRand = true;
            stat.Start.Interval = EvalValue(node.Start.Interval, spriteWidth, spriteHeight, 0, 0, 50);
            stat.Start.Offset = node.Start.OffsetY;
            stat.Start.Opacity = node.Start.Opacity;

            stat.End = new AnimationStatistics.StepValues();
            if (node.End.X.IndexOf("screen") >= 0 || node.End.X.IndexOf("area") >= 0) variableScreen = true;
            if (node.End.X.IndexOf("random") >= 0) variableRand = true;
            stat.End.X = EvalValue(node.End.X, spriteWidth, spriteHeight, 0, 0, 50);
            if (node.End.Y.IndexOf("screen") >= 0 || node.End.Y.IndexOf("area") >= 0) variableScreen = true;
            if (node.End.Y.IndexOf("random") >= 0) variableRand = true;
            stat.End.Y = EvalValue(node.End.Y, spriteWidth, spriteHeight, 0, 0, 50);
            if (node.End.Interval.IndexOf("screen") >= 0 || node.End.Interval.IndexOf("area") >= 0) variableScreen = true;
            if (node.End.Interval.IndexOf("random") >= 0) variableRand = true;
            stat.End.Interval = EvalValue(node.End.Interval, spriteWidth, spriteHeight, 0, 0, 50);
            stat.End.Offset = node.End.OffsetY;
            stat.End.Opacity = node.End.Opacity;

            stat.SubSteps = new List<AnimationStatistics.StepValues>();
            for(var k=0;k<stat.TotalFrames;k++)
            {
                var step = new AnimationStatistics.StepValues();
                step.X = (int)(stat.Start.X + (stat.End.X - stat.Start.X) * ((double)k / stat.TotalFrames));
                step.Y = (int)(stat.Start.Y + (stat.End.Y - stat.Start.Y) * ((double)k / stat.TotalFrames));
                step.Interval = (int)(stat.Start.Interval + (stat.End.Interval - stat.Start.Interval) * ((double)k / stat.TotalFrames));
                step.Opacity = (int)(stat.Start.Opacity + (stat.End.Opacity - stat.Start.Opacity) * ((double)k / stat.TotalFrames));
                step.Offset = (int)(stat.Start.Offset + (stat.End.Offset - stat.Start.Offset) * ((double)k / stat.TotalFrames));
                stat.SubSteps.Add(step);
            }

            stat.RandomVariable = variableRand;
            stat.ScreenVariable = variableScreen;
            stat.TotalTime = 0;
            stat.TotalX = 0;
            stat.TotalY = 0;
            foreach(var s in stat.SubSteps)
            {
                stat.TotalTime += s.Interval;
                stat.TotalX += s.X;
                stat.TotalY += s.Y;
            }

            return stat;
        }

        static public XmlData.ChildNode GetXmlChildNode(int id)
        {
            XmlData.ChildNode node = null;
            if (Program.AnimationXML.Childs != null && Program.AnimationXML.Childs.Child != null)
            {
                foreach (var n in Program.AnimationXML.Childs.Child)
                {
                    if (n.Id == id) return n;
                }
            }

            return node;
        }

        static public void UpdateXmlChildNode(XmlData.ChildNode oldNode, XmlData.ChildNode newNode)
        {
            if (Program.AnimationXML.Childs == null || Program.AnimationXML.Childs.Child == null || oldNode == null)
            {
                var newNodes = new XmlData.ChildNode[1];
                newNodes[0] = newNode;
                if (Program.AnimationXML.Childs == null) Program.AnimationXML.Childs = new XmlData.ChildsNode();
                if (Program.AnimationXML.Childs.Child == null) Program.AnimationXML.Childs.Child = new XmlData.ChildNode[0];
                Program.AnimationXML.Childs.Child = Program.AnimationXML.Childs.Child.Concat(newNodes).ToArray();
            }
            else
            {
                foreach (var n in Program.AnimationXML.Childs.Child)
                {
                    if (n.Id == oldNode.Id)
                    {
                        n.Next = newNode.Next;
                        n.Id = newNode.Id;
                        n.X = newNode.X;
                        n.Y = newNode.Y;
                        break;
                    }
                }
            }
        }

        static public XmlData.SoundNode GetXmlSoundNode(int id)
        {
            XmlData.SoundNode node = null;
            if (Program.AnimationXML.Sounds != null && Program.AnimationXML.Sounds.Sound != null)
            {
                foreach (var n in Program.AnimationXML.Sounds.Sound)
                {
                    if (n.Id == id) return n;
                }
            }

            return node;
        }

        static public void UpdateXmlSoundNode(XmlData.SoundNode oldNode, XmlData.SoundNode newNode)
        {
            if (Program.AnimationXML.Sounds == null || Program.AnimationXML.Sounds.Sound == null || oldNode == null)
            {
                var newNodes = new XmlData.SoundNode[1];
                newNodes[0] = newNode;
                if (Program.AnimationXML.Sounds == null) Program.AnimationXML.Sounds = new XmlData.SoundsNode();
                if (Program.AnimationXML.Sounds.Sound == null) Program.AnimationXML.Sounds.Sound = new XmlData.SoundNode[0];
                Program.AnimationXML.Sounds.Sound = Program.AnimationXML.Sounds.Sound.Concat(newNodes).ToArray();
            }
            else
            {
                foreach (var n in Program.AnimationXML.Sounds.Sound)
                {
                    if (n.Id == oldNode.Id)
                    {
                        n.Id = newNode.Id;
                        n.Base64 = newNode.Base64;
                        n.Loop = newNode.Loop;
                        n.Probability = newNode.Probability;
                        break;
                    }
                }
            }
        }

        static public int EvalValue(String parsingText, int spriteWidth, int spriteHeight, int parentX, int parentY, int iRandomSpawn)
        {
            int iRet = 0;
            Random rand = new Random();

            // When adding a child, it is important to place the child on the other side of the parent, if the parent is flipped.
            var dt = new DataTable();
            var screen = Screen.PrimaryScreen;

            parsingText = parsingText.Replace("screenW", screen.Bounds.Width.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("screenH", screen.Bounds.Height.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("areaW", screen.WorkingArea.Width.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("areaH", (screen.WorkingArea.Height + screen.WorkingArea.Y).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageW", spriteWidth.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageH", spriteHeight.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageX", (parentX).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageY", (parentY).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("random", rand.Next(0, 100).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("randS", iRandomSpawn.ToString(CultureInfo.InvariantCulture));

            try
            {
                var v = dt.Compute(parsingText, "");
                double dv;
                if (double.TryParse(v.ToString(), out dv))
                {
                    iRet = (int)dv;
                }
            }
            catch(Exception ex)
            {
                Program.AddLog("Unable to parse string " + parsingText + " - " + ex.Message, "Parse value", true);
            }

            return iRet;
        }
    }

    class XmlNodeCombo
    {
        public int Id { get; set; }
        public string Name { get; set; }

        override public string ToString()
        {
            return " - " + Id + " - " + Name;
        }
    }

    class AnimationStatistics
    {
        public class StepValues
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Interval { get; set; }
            public int Offset { get; set; }
            public double Opacity { get; set; }
        }
        public int Frames { get; set; }
        public int Repeats { get; set; }
        public int RepeatFrom { get; set; }

        public StepValues Start { get; set; }
        public StepValues End { get; set; }
        public List<StepValues> SubSteps { get; set; }

        public int TotalFrames { get; set; }
        public int TotalTime { get; set; }
        public int TotalX { get; set; }
        public int TotalY { get; set; }
        public bool ScreenVariable { get; set; }
        public bool RandomVariable { get; set; }
    }

    class CompareByIndex : IComparer
    {
        private readonly ListView _listView;

        public CompareByIndex(ListView listView)
        {
            this._listView = listView;
        }
        public int Compare(object x, object y)
        {
            int i = this._listView.Items.IndexOf((ListViewItem)x);
            int j = this._listView.Items.IndexOf((ListViewItem)y);
            return i - j;
        }
    }
}
