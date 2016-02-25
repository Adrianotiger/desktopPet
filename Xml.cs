using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Xml.Schema;
using System.Globalization;

namespace DesktopPet
{
        /// <summary>
        /// Main informations about the animation XML. This information is taken from the loaded xml file.
        /// </summary>
        /// <remarks>Once the XML was loaded, it is possible to see the header info in the about box.</remarks>
    public struct Header
    {
            /// <summary>
            /// Author of the animation.
            /// </summary>
        public string Author;
            /// <summary>
            /// Title of the animation.
            /// </summary>
        public string Title;
            /// <summary>
            /// Version of the animation (is a string and developer can insert what he want).
            /// </summary> 
        public string Version;
            /// <summary>
            /// PetName. Similar to Title but shorter (max 16 chars). "eSheep" word will be replaced with this one in the context menu.
            /// </summary>
        public string PetName;
            /// <summary>
            ///  Information (About and Copyright information) about the animation and the author.
            /// </summary>
        public string Info;
    };

        /// <summary>
        /// Sprite sheet (PNG with all possible positions).
        /// </summary>
    public struct Images
    {
            /// <summary>
            /// Memory stream containing the PNG sprite sheet.
            /// </summary>
        public MemoryStream bitmapImages;
            /// <summary>
            /// Total images horizontally (position counts in the X axis).
            /// </summary>
        public int xImages;
            /// <summary>
            /// Total images vertically (position counts in the Y axis).
            /// </summary>
        public int yImages;
    };

        /// <summary>
        /// Xml class contains all functions to read the XML file and functions to parse it.
        /// </summary>
    public sealed class Xml : IDisposable
    {
            /// <summary>
            /// XML Document, containing the animations xml.
            /// </summary>
        XmlDocument xmlDoc;
            /// <summary>
            /// XML Namespace, to check if xml is valid.
            /// </summary>
        XmlNamespaceManager xmlNS;
            /// <summary>
            /// Informations about the animation, see <see cref="Header"/>.
            /// </summary>
        public Header headerInfo;
            /// <summary>
            /// Structure with the sprite sheet informations.
            /// </summary>
        public Images images;
            /// <summary>
            /// A memory stream containing the animation icon. This is visible in the taskbar and tray icon.
            /// </summary>
        public MemoryStream bitmapIcon;
            
            /// <summary>
            /// X position of the parent image. Used to set the child position.
            /// </summary>
        public int parentX;
            /// <summary>
            /// Y position of the parent image. Used to set the child position.
            /// </summary>
        public int parentY;
            /// <summary>
            /// If the parent is flipped. If so, the image will be flipped and screen-mirrored.
            /// </summary>
        public bool parentFlipped;
            /// <summary>
            /// The sprite sheet image, full image with all frames.
            /// </summary>
        private Bitmap FullImage;
            /// <summary>
            /// Random spawn, this value changes each time the XML is reloaded. Used in the animation xml.
            /// </summary>
        int iRandomSpawn = 10;

            /// <summary>
            /// Constructor. Initialize member variables.
            /// </summary>
        public Xml()
        {
            headerInfo = new Header();
            xmlDoc = new XmlDocument();
            images = new Images();

            parentX = -1;                   // -1 means it is not a child.
            parentY = -1;
            parentFlipped = false;
        }

            /// <summary>
            /// Dispose class and created objects.
            /// </summary>
        public void Dispose()
        {
            bitmapIcon.Dispose();
            FullImage.Dispose();
        }
        
            /// <summary>
            /// Event handler to check XML validity.
            /// </summary>
            /// <param name="sender">Caller as object.</param>
            /// <param name="e">Validation event values.</param>
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

            /// <summary>
            /// This function will load the XML. If something can't be loaded as expected, the default XML will be loaded.
            /// </summary>
            /// <returns>true, if the XML was loaded successfully.</returns>
        public bool readXML()
        {
            XmlReaderSettings settings = new XmlReaderSettings();
            ValidationEventHandler eventHandler = new ValidationEventHandler(ValidationEventHandler);
            XmlSchema schema = XmlSchema.Read(new StringReader(Properties.Resources.animations1), eventHandler);
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
                reader = XmlReader.Create(new StringReader(Properties.Settings.Default.xml), settings);
                xmlDoc.Load(reader);
                xmlDoc.Validate(eventHandler);
                Properties.Settings.Default.Images = xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:png", xmlNS).InnerText;
                Properties.Settings.Default.Icon = xmlDoc.SelectSingleNode("//pet:animations/pet:header/pet:icon", xmlNS).InnerText;
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
            
                // If it was not possible to load the new XML, load the default XML.
            if (xmlDoc.ChildNodes.Count <= 0)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "Loading default XML animation");

                try
                {
                    reader = XmlReader.Create(new StringReader(Properties.Resources.animations), settings);

                    xmlDoc.Load(reader);
                    xmlDoc.Validate(eventHandler);
                }
                catch (Exception ex)
                {
                    StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "XML error: " + ex.ToString());
                    MessageBox.Show("FATAL ERROR reading XML file: " + ex.Message, "ERROR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
                // Load images and header info
            try
            {
                readImages();

                images.xImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesx", xmlNS).InnerText, CultureInfo.InvariantCulture);
                images.yImages = int.Parse(xmlDoc.SelectSingleNode("//pet:animations/pet:image/pet:tilesy", xmlNS).InnerText, CultureInfo.InvariantCulture);

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

            /// <summary>
            /// Load the animations (read them from XML file)
            /// </summary>
            /// <param name="animations">Animation class where the animations should be saved</param>
        public void loadAnimations(Animations animations)
        {
            XmlNodeList nodes = xmlDoc.SelectNodes("//pet:animations/pet:animations/pet:animation", xmlNS);
                // for each animation
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText, CultureInfo.InvariantCulture);
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
                                        ani.Start.OffsetY = int.Parse(node2.SelectSingleNode(".//pet:offsety", xmlNS).InnerText, CultureInfo.InvariantCulture);
                                    else
                                        ani.Start.OffsetY = 0;
                                    if (node2.SelectSingleNode(".//pet:opacity", xmlNS) != null)
                                        ani.Start.Opacity = double.Parse(node2.SelectSingleNode(".//pet:opacity", xmlNS).InnerText, CultureInfo.InvariantCulture);
                                    else
                                        ani.Start.Opacity = 1.0;
                                    break;
                        case "end":
                                    ani.End.X = getXMLCompute(node2.SelectSingleNode(".//pet:x", xmlNS).InnerText);
                                    ani.End.Y = getXMLCompute(node2.SelectSingleNode(".//pet:y", xmlNS).InnerText);
                                    ani.End.Interval = getXMLCompute(node2.SelectSingleNode(".//pet:interval", xmlNS).InnerText);
                                    if (node2.SelectSingleNode(".//pet:offsety", xmlNS) != null)
                                        ani.End.OffsetY = int.Parse(node2.SelectSingleNode(".//pet:offsety", xmlNS).InnerText, CultureInfo.InvariantCulture);
                                    else
                                        ani.End.OffsetY = 0;
                                    if (node2.SelectSingleNode(".//pet:opacity", xmlNS) != null)
                                        ani.End.Opacity = double.Parse(node2.SelectSingleNode(".//pet:opacity", xmlNS).InnerText, CultureInfo.InvariantCulture);
                                    else
                                        ani.End.Opacity = 1.0;
                                    break;
                        case "sequence":
                                    ani.Sequence.RepeatFrom = int.Parse(node2.Attributes["repeatfrom"].InnerText, CultureInfo.InvariantCulture);
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
                                                int.Parse(node3.InnerText, CultureInfo.InvariantCulture),
                                                int.Parse(node3.Attributes["probability"].InnerText, CultureInfo.InvariantCulture),
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
                                                int.Parse(node3.InnerText, CultureInfo.InvariantCulture),
                                                int.Parse(node3.Attributes["probability"].InnerText, CultureInfo.InvariantCulture),
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
                                                int.Parse(node3.InnerText, CultureInfo.InvariantCulture),
                                                int.Parse(node3.Attributes["probability"].InnerText, CultureInfo.InvariantCulture),
                                                where
                                            )
                                        );
                                    }
                                    break;
                    }
                }
                animations.SaveAnimation(ani, id);
            }

                // for each spawn
            nodes = xmlDoc.SelectNodes("//pet:animations/pet:spawns/pet:spawn", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["id"].InnerText, CultureInfo.InvariantCulture);
                TSpawn ani = animations.AddSpawn(id, int.Parse(node.Attributes["probability"].InnerText, CultureInfo.InvariantCulture));

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
                                    ani.Next = int.Parse(node2.InnerText, CultureInfo.InvariantCulture);
                                    break;
                    }
                }
                animations.SaveSpawn(ani, id);
            }

                // for each child
            nodes = xmlDoc.SelectNodes("//pet:animations/pet:childs/pet:child", xmlNS);
            foreach (XmlNode node in nodes)
            {
                int id = int.Parse(node.Attributes["animationid"].InnerText, CultureInfo.InvariantCulture);
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
                            aniChild.Next = int.Parse(node2.InnerText, CultureInfo.InvariantCulture);
                            break;
                    }
                }
                animations.SaveChild(aniChild, id);
            }
        }

            /// <summary>
            /// Get the value from XML file. If special keys are used (like screenW) it will be converted.
            /// </summary>
            /// <param name="text">XML text value.</param>
            /// <returns>A structure with the values.</returns>
        public TValue getXMLCompute(string text)
        {
            TValue v;

            v.Compute = text;
            v.Random = (v.Compute.IndexOf("random") >= 0 || v.Compute.IndexOf("randS") >= 0);
            v.Value = parseValue(v.Compute);

            return v;
        }

            /// <summary>
            /// Parse a value, converting keys like screenW, imageH, random,... to integers.
            /// </summary>
            /// <remarks>
            /// See <a href="https://msdn.microsoft.com/en-us/library/9za5w1xw(v=vs.100).aspx">https://msdn.microsoft.com/en-us/library/9za5w1xw(v=vs.100).aspx</a>
            /// for more information of what you can write as sText (expression to compute).
            /// </remarks> 
            /// <param name="sText">The text to parse and convert.</param>
            /// <returns>The integer value from the parsed text expression.</returns>
        public int parseValue(string sText)
        {
            int iRet = 0;
            DataTable dt = new DataTable();
            Random rand = new Random();

            sText = sText.Replace("screenW", Screen.PrimaryScreen.Bounds.Width.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("screenH", Screen.PrimaryScreen.Bounds.Height.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("areaW", Screen.PrimaryScreen.WorkingArea.Width.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("areaH", Screen.PrimaryScreen.WorkingArea.Height.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageW", (FullImage.Width / images.xImages).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageH", (FullImage.Height / images.yImages).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageX", (parentX).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageY", (parentY).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("random", rand.Next(0, 100).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("randS", iRandomSpawn.ToString(CultureInfo.InvariantCulture));
            
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

            /// <summary>
            /// Read images from XML file and store them in the application.
            /// </summary>
        private void readImages()
        {
            try
            {
                if (Properties.Settings.Default.Images.Length < 2) throw new InvalidDataException();
                images.bitmapImages = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Images));
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
                        Properties.Settings.Default.Images = node.InnerText;
                        int mod4 = Properties.Settings.Default.Images.Length % 4;
                        if (mod4 > 0)
                        {
                            Properties.Settings.Default.Images += new string('=', 4 - mod4);
                        }
                        Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    images.bitmapImages = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Images));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            
            try
            {
                if (Properties.Settings.Default.Icon.Length < 100) throw new InvalidDataException();
                bitmapIcon = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Icon));
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
                        Properties.Settings.Default.Icon = node.InnerText;
                        int mod4 = Properties.Settings.Default.Icon.Length % 4;
                        if (mod4 > 0)
                        {
                            Properties.Settings.Default.Icon += new string('=', 4 - mod4);
                        }
                        Properties.Settings.Default.Save();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    bitmapIcon = new MemoryStream(Convert.FromBase64String(Properties.Settings.Default.Icon));
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
