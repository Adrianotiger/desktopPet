using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Xml.Schema;

namespace desktopPet
{
    public struct Header
    {
        public string Author;
        public string Title;
        public string Version;
        public string Info;
    };

    public struct Images
    {
        public MemoryStream bitmapImages;
        public int xImages;
        public int yImages;
    };

    public class Xml
    {
        XmlDocument xmlDoc;
        XmlNamespaceManager xmlNS;
        public Header headerInfo;
        public Images images;
        public MemoryStream bitmapIcon;

        private Bitmap FullImage;
        
        public Xml()
        {
            headerInfo = new Header();
            xmlDoc = new XmlDocument();
            images = new Images();
        }

        static void ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            switch (e.Severity)
            {
                case XmlSeverityType.Error:
                    StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "XSD validation: " + e.Message);
                    break;
                case XmlSeverityType.Warning:
                    StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "XSD validation: " + e.Message);
                    break;
            }

        }

        public void readXML()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
            XmlSchema schema = XmlSchema.Read(new StringReader(DesktopPet.Properties.Resources.animations1), eventHandler);
            settings.Schemas.Add(schema);
            settings.ValidationType = ValidationType.Schema;

            xmlDoc = new XmlDocument();

            xmlNS = new XmlNamespaceManager(xmlDoc.NameTable);
            xmlNS.AddNamespace("pet", "http://esheep.petrucci.ch/");

            XmlReader reader = null;

            // try to load local xml
            try
            {
                reader = XmlReader.Create(new StringReader(DesktopPet.Properties.Settings.Default.xml), settings);
                xmlDoc.Load(reader);
                xmlDoc.Validate(eventHandler);
                DesktopPet.Properties.Settings.Default.Images = xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:png", xmlNS).InnerText;
                DesktopPet.Properties.Settings.Default.Icon = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:icon", xmlNS).InnerText;
            }
            catch (Exception ex)
            {
                xmlDoc.RemoveAll();
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "User XML error: " + ex.ToString());
            }
            //XmlReader reader = XmlReader.Create(new StringReader(DesktopPet.Properties.Resources.animations), settings);

            if (xmlDoc.ChildNodes.Count <= 0)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "Loading default XML animation");

                try
                {
                    reader = XmlReader.Create(new StringReader(DesktopPet.Properties.Resources.animations), settings);

                    xmlDoc.Load(reader);
                    xmlDoc.Validate(eventHandler);
                }
                catch (Exception ex)
                {
                    StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "XML error: " + ex.ToString());
                    MessageBox.Show("FATAL ERROR reading XML file: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            readImages();

            images.xImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesx", xmlNS).InnerText);
            images.yImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesy", xmlNS).InnerText);

            headerInfo.Author = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:author", xmlNS).InnerText;
            headerInfo.Title = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:title", xmlNS).InnerText;
            headerInfo.Info = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:info", xmlNS).InnerText;
            headerInfo.Version = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:version", xmlNS).InnerText;
        }

        public void loadAnimations(Animations animations)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes("//pet:animations/pet:animation", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText);
                TAnimation ani = animations.AddAnimation(id, id.ToString());
                ani.Border = false;
                ani.Gravity = false;
                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "name": 
                                    ani.Name = node2.InnerText;
                                    switch(ani.Name)
                                    {
                                        case "fall": animations.AnimationFall = id; break;
                                        case "drag": animations.AnimationDrag = id; break;
                                        case "kill": animations.AnimationKill = id; break;
                                        case "sync": animations.AnimationSync = id; break;
                                    }
                                    break;
                        case "start":
                                    ani.Start.X.Compute = node2.SelectSingleNode(".//pet:x", xmlNS).InnerText;
                                    ani.Start.X.Random = (ani.Start.X.Compute.IndexOf("random") >= 0);
                                    ani.Start.X.Value = parseValue(ani.Start.X.Compute);
                                    ani.Start.Y.Compute = node2.SelectSingleNode(".//pet:y", xmlNS).InnerText;
                                    ani.Start.Y.Random = (ani.Start.Y.Compute.IndexOf("random") >= 0);
                                    ani.Start.Y.Value = parseValue(ani.Start.Y.Compute);
                                    ani.Start.Interval.Compute = node2.SelectSingleNode(".//pet:interval", xmlNS).InnerText;
                                    ani.Start.Interval.Random = (ani.Start.Interval.Compute.IndexOf("random") >= 0);
                                    ani.Start.Interval.Value = parseValue(ani.Start.Interval.Compute);
                                    if (node2.SelectSingleNode(".//pet:offsety", xmlNS) != null)
                                        ani.Start.OffsetY = int.Parse(node2.SelectSingleNode(".//pet:offsety", xmlNS).InnerText);
                                    else
                                        ani.Start.OffsetY = 0;
                                    if (node2.SelectSingleNode(".//pet:opacity", xmlNS) != null)
                                        ani.Start.Opacity = double.Parse(node2.SelectSingleNode(".//pet:opacity", xmlNS).InnerText);
                                    else
                                        ani.Start.Opacity = 1.0;
                                    break;
                        case "end":
                                    ani.End.X.Compute = node2.SelectSingleNode(".//pet:x", xmlNS).InnerText;
                                    ani.End.X.Random = (ani.End.X.Compute.IndexOf("random") >= 0);
                                    ani.End.X.Value = parseValue(ani.End.X.Compute);
                                    ani.End.Y.Compute = node2.SelectSingleNode(".//pet:y", xmlNS).InnerText;
                                    ani.End.Y.Random = (ani.End.Y.Compute.IndexOf("random") >= 0);
                                    ani.End.Y.Value = parseValue(ani.End.Y.Compute);
                                    ani.End.Interval.Compute = node2.SelectSingleNode(".//pet:interval", xmlNS).InnerText;
                                    ani.End.Interval.Random = (ani.End.Interval.Compute.IndexOf("random") >= 0);
                                    ani.End.Interval.Value = parseValue(ani.End.Interval.Compute);
                                    if (node2.SelectSingleNode(".//pet:offsety", xmlNS) != null)
                                        ani.End.OffsetY = int.Parse(node2.SelectSingleNode(".//pet:offsety", xmlNS).InnerText);
                                    else
                                        ani.End.OffsetY = 0;
                                    if (node2.SelectSingleNode(".//pet:opacity", xmlNS) != null)
                                        ani.End.Opacity = double.Parse(node2.SelectSingleNode(".//pet:opacity", xmlNS).InnerText);
                                    else
                                        ani.End.Opacity = 1.0;
                                    break;
                        case "sequence":
                                    ani.Sequence.RepeatFrom = int.Parse(node2.Attributes["repeatfrom"].InnerText);
                                    ani.Sequence.Repeat.Compute = node2.Attributes["repeat"].InnerText;
                                    if(node2.SelectSingleNode(".//pet:action", xmlNS) != null)
                                        ani.Sequence.Action = node2.SelectSingleNode(".//pet:action", xmlNS).InnerText;
                                    ani.Sequence.Repeat.Random = (ani.Sequence.Repeat.Compute.IndexOf("random") >= 0);
                                    ani.Sequence.Repeat.Value = parseValue(ani.Sequence.Repeat.Compute);
                                    foreach (XmlNode node3 in node2.SelectNodes(".//pet:frame", xmlNS))
                                    {
                                        ani.Sequence.Frames.Add(int.Parse(node3.InnerText));
                                    }
                                    if(ani.Sequence.RepeatFrom > 0)
                                        ani.Sequence.TotalSteps = ani.Sequence.Frames.Count + (ani.Sequence.Frames.Count - ani.Sequence.RepeatFrom - 1) * ani.Sequence.Repeat.Value;
                                    else
                                        ani.Sequence.TotalSteps = ani.Sequence.Frames.Count + ani.Sequence.Frames.Count * ani.Sequence.Repeat.Value;
                                    foreach (XmlNode node3 in node2.SelectNodes(".//pet:next", xmlNS))
                                    {
                                        TNextAnimation.TOnly where = TNextAnimation.TOnly.NONE;
                                        if (node3.Attributes["only"] != null)
                                        {
                                            switch (node3.Attributes["only"].InnerText)
                                            {
                                                case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                                                case "window": where = TNextAnimation.TOnly.WINDOW; break;
                                                case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                                                case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                                                case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                                                default: where = TNextAnimation.TOnly.NONE; break;
                                            }
                                        }
                                        ani.EndAnimation.Add(
                                            new TNextAnimation(
                                                int.Parse(node3.InnerText),
                                                int.Parse(node3.Attributes["probability"].InnerText),
                                                where
                                            )
                                        );
                                    }
                                    break;
                        case "border":
                                    foreach (XmlNode node3 in node2.SelectNodes(".//pet:next", xmlNS))
                                    {
                                        TNextAnimation.TOnly where = TNextAnimation.TOnly.NONE;
                                        if (node3.Attributes["only"] != null)
                                        {
                                            switch (node3.Attributes["only"].InnerText)
                                            {
                                                case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                                                case "window": where = TNextAnimation.TOnly.WINDOW; break;
                                                case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                                                case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                                                case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                                            }
                                        }
                                        ani.Border = true;
                                        ani.EndBorder.Add(
                                            new TNextAnimation(
                                                int.Parse(node3.InnerText),
                                                int.Parse(node3.Attributes["probability"].InnerText),
                                                where
                                            )
                                        );
                                    }
                                    break;
                        case "gravity":
                                    foreach (XmlNode node3 in node2.SelectNodes(".//pet:next", xmlNS))
                                    {
                                        TNextAnimation.TOnly where = TNextAnimation.TOnly.NONE;
                                        if (node3.Attributes["only"] != null)
                                        {
                                            switch (node3.Attributes["only"].InnerText)
                                            {
                                                case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                                                case "window": where = TNextAnimation.TOnly.WINDOW; break;
                                                case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                                                case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                                                case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                                            }
                                        }
                                        ani.Gravity = true;
                                        ani.EndGravity.Add(
                                            new TNextAnimation(
                                                int.Parse(node3.InnerText),
                                                int.Parse(node3.Attributes["probability"].InnerText),
                                                where
                                            )
                                        );
                                    }
                                    break;
                    }
                }
                animations.SaveAnimation(ani, id);
            }

            nodes = xmlDoc.SelectNodes("//pet:animations/pet:spawn", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText);
                TSpawn ani = animations.AddSpawn(id, int.Parse(node.Attributes["probability"].InnerText), id.ToString());

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "x":
                                    ani.Start.X.Compute = node2.InnerText;
                                    ani.Start.X.Random = (ani.Start.X.Compute.IndexOf("random") >= 0);
                                    ani.Start.X.Value = parseValue(ani.Start.X.Compute);
                                    break;
                        case "y":
                                    ani.Start.Y.Compute = node2.InnerText;
                                    ani.Start.Y.Random = (ani.Start.Y.Compute.IndexOf("random") >= 0);
                                    ani.Start.Y.Value = parseValue(ani.Start.Y.Compute);
                                    break;
                        case "direction":
                                    string sDirection = node2.InnerText;
                                    ani.MoveLeft = (sDirection == "left");
                                    break;
                        case "next":
                                    ani.Next = int.Parse(node2.InnerText);
                                    break;
                    }
                }
                animations.SaveSpawn(ani, id);
            }
        }

        public int parseValue(string sText)
        {
            int iRet = 0;
            DataTable dt = new DataTable();
            Random rand = new Random();

            sText = sText.Replace("screenW", Screen.PrimaryScreen.Bounds.Width.ToString());
            sText = sText.Replace("screenH", Screen.PrimaryScreen.Bounds.Height.ToString());
            sText = sText.Replace("areaW", Screen.PrimaryScreen.WorkingArea.Width.ToString());
            sText = sText.Replace("areaH", Screen.PrimaryScreen.WorkingArea.Height.ToString());
            sText = sText.Replace("imageW", (FullImage.Width / images.xImages).ToString());
            sText = sText.Replace("imageH", (FullImage.Height / images.yImages).ToString());
            sText = sText.Replace("random", rand.Next(0, 100).ToString());

            var v = dt.Compute(sText, "");
            double dv;
            if (double.TryParse(v.ToString(), out dv))
            {
                iRet = (int)dv;
            }
            else
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "Unable to parse integer: " + sText);
            }
            
            return iRet;
        }

        private void readImages()
        {
            /*
            if (DesktopPet.Properties.Settings.Default.xml == null || DesktopPet.Properties.Settings.Default.xml.Length < 100)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "loading user animation");
                xmlDoc.LoadXml(DesktopPet.Properties.Resources.animations);
            }
            else
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "no user xml, loading default");
                xmlDoc.LoadXml(DesktopPet.Properties.Settings.Default.xml);
            }
            */
            
            try
            {
                if (DesktopPet.Properties.Settings.Default.Images.Length < 2) throw new InvalidDataException();
                images.bitmapImages = new MemoryStream(Convert.FromBase64String(DesktopPet.Properties.Settings.Default.Images));
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "user images loaded");
            }
            catch (Exception)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "user images not found, loading defaults");
                try
                {
                    XmlNode node = xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:png", xmlNS);
                    if (node != null)
                    {
                        DesktopPet.Properties.Settings.Default.Images = node.InnerText;
                        int mod4 = DesktopPet.Properties.Settings.Default.Images.Length % 4;
                        if (mod4 > 0)
                        {
                            DesktopPet.Properties.Settings.Default.Images += new string('=', 4 - mod4);
                        }
                        DesktopPet.Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    images.bitmapImages = new MemoryStream(Convert.FromBase64String(DesktopPet.Properties.Settings.Default.Images));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            
            try
            {
                if (DesktopPet.Properties.Settings.Default.Icon.Length < 100) throw new InvalidDataException();
                bitmapIcon = new MemoryStream(Convert.FromBase64String(DesktopPet.Properties.Settings.Default.Icon));
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "user icon loaded");
            }
            catch (Exception)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "no user icon, loading default");
                try
                {
                    XmlNode node = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:icon", xmlNS);
                    if (node != null)
                    {
                        DesktopPet.Properties.Settings.Default.Icon = node.InnerText;
                        int mod4 = DesktopPet.Properties.Settings.Default.Icon.Length % 4;
                        if (mod4 > 0)
                        {
                            DesktopPet.Properties.Settings.Default.Icon += new string('=', 4 - mod4);
                        }
                        DesktopPet.Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    bitmapIcon = new MemoryStream(Convert.FromBase64String(DesktopPet.Properties.Settings.Default.Icon));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }

            FullImage = new Bitmap(images.bitmapImages);
        }
    }
}
