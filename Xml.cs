using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Xml.Schema;

namespace DesktopPet
{
        /// <summary>
        /// Main informations about the animation XML. This information is taken from the loaded xml file.
        /// </summary>
        /// <remarks>Once the XML was loaded, it is possible to see the header info in the about box.</remarks>
    public struct Header
    {
            /// <summary>
            /// Author of the animation
            /// </summary>
        public string Author;
            /// <summary>
            /// Title of the animation
            /// </summary>
        public string Title;
            /// <summary>
            /// Version of the animation (is a string and developer can insert what he want)
            /// </summary> 
        public string Version;
            /// <summary>
            /// PetName. Similar to Title but shorter (max 16 chars). "eSheep" word will be replaced with this one in the context menu.
            /// </summary>
        public string PetName;
            /// <summary>
            ///  Information (About and Copyright information) about the animation and the author
            /// </summary>
        public string Info;
    };

        /// <summary>
        /// Sprite sheet (PNG with all possible positions)
        /// </summary>
    public struct Images
    {
            /// <summary>
            /// Memory stream containing the PNG sprite sheet
            /// </summary>
        public MemoryStream bitmapImages;
            /// <summary>
            /// Total images horizontally (position counts in the X axis)
            /// </summary>
        public int xImages;
            /// <summary>
            /// Total images vertically (position counts in the Y axis)
            /// </summary>
        public int yImages;
    };

    public sealed class Xml : IDisposable
    {
            /// <summary>
            /// XML Document, containing the animations xml
            /// </summary>
        XmlDocument xmlDoc;
            /// <summary>
            /// XML Namespace, to check if xml is valid
            /// </summary>
        XmlNamespaceManager xmlNS;
            /// <summary>
            /// Informations about the animation, see <see cref="Header"/>
            /// </summary>
        public Header headerInfo;
            /// <summary>
            /// Structure with the sprite sheet informations
            /// </summary>
        public Images images;
            /// <summary>
            /// A memory stream containing the animation icon. This is visible in the taskbar and tray icon.
            /// </summary>
        public MemoryStream bitmapIcon;
            
        public int parentX;
        public int parentY;
        public bool parentFlipped;

        private Bitmap FullImage;

        int iRandomSpawn = 10;

        public Xml()
        {
            headerInfo = new Header();
            xmlDoc = new XmlDocument();
            images = new Images();

            parentX = -1;
            parentY = -1;
            parentFlipped = false;
        }

        public void Dispose()
        {
            bitmapIcon.Dispose();
            FullImage.Dispose();
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

        public bool readXML()
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
            bool bError = false;

            Random rand = new Random();
            iRandomSpawn = rand.Next(0, 100);

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
                if (xmlDoc.ChildNodes.Count > 0)
                {
                    MessageBox.Show("Error parsing animation XML:" + ex.ToString(), "XML error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
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
            
            try
            {
                readImages();

                images.xImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesx", xmlNS).InnerText);
                images.yImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesy", xmlNS).InnerText);

                headerInfo.Author = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:author", xmlNS).InnerText;
                headerInfo.Title = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:title", xmlNS).InnerText;
                headerInfo.Info = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:info", xmlNS).InnerText;
                headerInfo.Version = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:version", xmlNS).InnerText;
                headerInfo.PetName = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:petname", xmlNS).InnerText;
                if (headerInfo.PetName.Length > 16) headerInfo.PetName = headerInfo.PetName.Substring(0, 16);
            }
            catch(Exception ex)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "Error reading XML: " + ex.Message);
                bError = true;
            }

            if(bError)
            {
                MessageBox.Show("Error, can't load animations file. The original pet will be loaded", "XML error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }

            return !bError;
        }

        public void loadAnimations(Animations animations)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes("//pet:animations/pet:animations/pet:animation", xmlNS);
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
                                    ani.Start.X = getXMLCompute(node2.SelectSingleNode(".//pet:x", xmlNS).InnerText);
                                    ani.Start.Y = getXMLCompute(node2.SelectSingleNode(".//pet:y", xmlNS).InnerText);
                                    ani.Start.Interval = getXMLCompute(node2.SelectSingleNode(".//pet:interval", xmlNS).InnerText);
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
                                    ani.End.X = getXMLCompute(node2.SelectSingleNode(".//pet:x", xmlNS).InnerText);
                                    ani.End.Y = getXMLCompute(node2.SelectSingleNode(".//pet:y", xmlNS).InnerText);
                                    ani.End.Interval = getXMLCompute(node2.SelectSingleNode(".//pet:interval", xmlNS).InnerText);
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
                                    if(node2.SelectSingleNode(".//pet:action", xmlNS) != null)
                                        ani.Sequence.Action = node2.SelectSingleNode(".//pet:action", xmlNS).InnerText;
                                    ani.Sequence.Repeat = getXMLCompute(node2.Attributes["repeat"].InnerText);
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

            nodes = xmlDoc.SelectNodes("//pet:animations/pet:spawns/pet:spawn", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText);
                TSpawn ani = animations.AddSpawn(id, int.Parse(node.Attributes["probability"].InnerText));

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "x":
                                    ani.Start.X = getXMLCompute(node2.InnerText);
                                    break;
                        case "y":
                                    ani.Start.Y = getXMLCompute(node2.InnerText);
                                    break;
                        case "next":
                                    ani.Next = int.Parse(node2.InnerText);
                                    break;
                    }
                }
                animations.SaveSpawn(ani, id);
            }

            nodes = xmlDoc.SelectNodes("//pet:animations/pet:childs/pet:child", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["animationid"].InnerText);
                TChild aniChild = animations.AddChild(id);
                aniChild.AnimationID = id;

                foreach (XmlNode node2 in node.ChildNodes)
                {
                    switch (node2.Name)
                    {
                        case "x":
                            aniChild.Position.X = getXMLCompute(node2.InnerText);
                            break;
                        case "y":
                            aniChild.Position.Y = getXMLCompute(node2.InnerText);
                            break;
                        case "next":
                            aniChild.Next = int.Parse(node2.InnerText);
                            break;
                    }
                }
                animations.SaveChild(aniChild, id);
            }
        }

        public TValue getXMLCompute(string text)
        {
            TValue v;

            v.Compute = text;
            v.Random = (v.Compute.IndexOf("random") >= 0 || v.Compute.IndexOf("randS") >= 0);
            v.Value = parseValue(v.Compute);

            return v;
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
            sText = sText.Replace("imageX", (parentX).ToString());
            sText = sText.Replace("imageY", (parentY).ToString());
            sText = sText.Replace("random", rand.Next(0, 100).ToString());
            sText = sText.Replace("randS", iRandomSpawn.ToString());
            
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
