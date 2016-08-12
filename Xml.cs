using System;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Data;
using System.Drawing;
using System.Xml.Schema;
using System.Globalization;
using System.Xml.Serialization;
using System.ComponentModel;

namespace DesktopPet
{
        /// <summary>
        /// This Node class is used to store the XML data using the serialize function.
        /// </summary>
        /// <remarks>Once the XML was loaded, it is possible to see the header info in the about box.</remarks>
    [XmlRoot("animations", Namespace= "http://esheep.petrucci.ch/", IsNullable=false)]
    public class RootNode
    {
            /// <summary>
            /// Main informations about the animation XML. 
            /// </summary>
        [XmlElement("header")]
        public HeaderNode Header;
            /// <summary>
            /// Information about the sprite image.
            /// </summary>
        [XmlElement("image")]
        public ImageNode Image;
            /// <summary>
            /// List of spawns. 
            /// </summary>
        [XmlElement("spawns")]
        public SpawnsNode Spawns;
            /// <summary>
            /// List of animations. 
            /// </summary>
        [XmlElement("animations")]
        public AnimationsNode Animations;
            /// <summary>
            /// List of child animations. 
            /// </summary>
        [XmlElement("childs")]
        public ChildsNode Childs;
            /// <summary>
            /// List of sounds. 
            /// </summary>
        [XmlElement("sounds")]
        public SoundsNode Sounds;
    }


        /// <summary>
        /// Main informations about the animation XML. This information is taken from the loaded xml file.
        /// </summary>
        /// <remarks>Once the XML was loaded, it is possible to see the header info in the about box.</remarks>
    public class HeaderNode
    {
            /// <summary>
            /// Author of the animation.
            /// </summary>
        [XmlElement("author")]
        public string Author;
            /// <summary>
            /// Title of the animation.
            /// </summary>
        [XmlElement("title")]
        public string Title;
            /// <summary>
            /// PetName. Similar to Title but shorter (max 16 chars). "eSheep" word will be replaced with this one in the context menu.
            /// </summary>
        [XmlElement("petname")]
        public string Petname;
            /// <summary>
            /// Version of the animation (is a string and developer can insert what he want).
            /// </summary> 
        [XmlElement("version")]
        public string Version;
            /// <summary>
            ///  Information (About and Copyright information) about the animation and the author.
            /// </summary>
        [XmlElement("info")]
        public string Info;
            /// <summary>
            ///  Application version. Used to parse different XML versions.
            /// </summary>
        [XmlElement("application")]
        public string Application;
            /// <summary>
            ///  Icon (base64) of the pet, used for the task bar.
            /// </summary>
        [XmlElement("icon")]
        public string Icon;
    }

        /// <summary>
        ///  Sprite image
        /// </summary>
    public class ImageNode
    {
            /// <summary>
            ///  Quantity of images on the X axis.
            /// </summary>
        [XmlElement("tilesx")]
        public int TilesX;
            /// <summary>
            ///  Quantity of images on the Y axis.
            /// </summary>
        [XmlElement("tilesy")]
        public int TilesY;
            /// <summary>
            ///  The sprite as base64 string.
            /// </summary>
        [XmlElement("png")]
        public string Png;
            /// <summary>
            ///  Color used for the transparency.
            /// </summary>
        [XmlElement("transparency")]
        public string Transparency;
    }

        /// <summary>
        ///  Node with a list of spawns.
        /// </summary>
    public class SpawnsNode
    {
            /// <summary>
            ///  List of spawn nodes.
            /// </summary>
        [XmlElement("spawn")]
        public SpawnNode[] Spawn;
    }

        /// <summary>
        ///  List of animations.
        /// </summary>
    public class AnimationsNode
    {
            /// <summary>
            ///  List of animation nodes.
            /// </summary>
        [XmlElement("animation")]
        public AnimationNode[] Animation;
    }

        /// <summary>
        ///  List of childs.
        /// </summary>
    public class ChildsNode
    {
            /// <summary>
            ///  List of child nodes.
            /// </summary>
        [XmlElement("child")]
        public ChildNode[] Child;
    }

        /// <summary>
        ///  List of sounds.
        /// </summary>
    public class SoundsNode
    {
        /// <summary>
        ///  List of sound nodes.
        /// </summary>
        [XmlElement("sound")]
        public SoundNode[] Sound;
    }

        /// <summary>
        ///  Information about the spawn. Used to start the pet on the screen.
        /// </summary>
    public class SpawnNode
    {
            /// <summary>
            ///  Unique ID of spawn.
            /// </summary>
        [XmlAttribute("id")]
        public int Id;
            /// <summary>
            ///  Probability to use this spawn.
            /// </summary>
        [XmlAttribute("probability")]
        public int Probability;
            /// <summary>
            ///  X start position of the pet.
            /// </summary>
        [XmlElement("x")]
        public string X;
            /// <summary>
            ///  Y start position of the pet.
            /// </summary>
        [XmlElement("y")]
        public string Y;
            /// <summary>
            ///  ID of the next animation.
            /// </summary>
        [XmlElement("next")]
        public NextNode Next;
    }

        /// <summary>
        ///  All information of each single animation is stored here.
        /// </summary>
    public class AnimationNode
    {
            /// <summary>
            ///  Unique ID, used to set the next animation.
            /// </summary>
        [XmlAttribute("id")]
        public int Id;
            /// <summary>
            ///  Name for this animation. With some key-names special actions can be defined. Otherwise it is used for debug purposes.
            /// </summary>
        [XmlElement("name")]
        public string Name;
            /// <summary>
            ///  Information about the start position (velocity, opacity, ...).
            /// </summary>
        [XmlElement("start")]
        public MovingNode Start;
            /// <summary>
            ///  Information about the end position (velocity, opacity, ...).
            /// </summary>
        [XmlElement("end")]
        public MovingNode End;
            /// <summary>
            ///  Sequence of frames to play and information about how to play.
            /// </summary>
        [XmlElement("sequence")]
        public SequenceNode Sequence;
            /// <summary>
            ///  What to do if a pet reach a border of the screen or window.
            /// </summary>
        [XmlElement("border")]
        public HitNode Border;
            /// <summary>
            ///  What to do if a pet doesn't have any gravity anymore.
            /// </summary>
        [XmlElement("gravity")]
        public HitNode Gravity;
    }

        /// <summary>
        ///  This is like a spawn but for second/child animation.
        /// </summary>
    public class ChildNode
    {
            /// <summary>
            ///  Id of animation. Once that animation is executed, this child is automatically played.
            /// </summary>
        [XmlAttribute("animationid")]
        public int Id;
            /// <summary>
            ///  X position when it is created.
            /// </summary>
        [XmlElement("x")]
        public string X;
            /// <summary>
            ///  Y position when it is created.
            /// </summary>
        [XmlElement("y")]
        public string Y;
            /// <summary>
            ///  The next animation used for this child.
            /// </summary>
        [XmlElement("next")]
        public int Next;
    }

        /// <summary>
        ///  To add sounds on some defined animations.
        /// </summary>
    public class SoundNode
    {
        /// <summary>
        ///  Id of animation. Once that animation is executed, this sound is automatically played.
        /// </summary>
        [XmlAttribute("animationid")]
        public int Id;
        /// <summary>
        ///  Probability (in %) that this sound will be played together with the animation.
        /// </summary>
        [XmlElement("probability")]
        public int Probability;
        /// <summary>
        ///  How many times the sound should loop (default: 0). 1 means that the sound will play 2 times.
        /// </summary>
        [DefaultValue(0), XmlElement("loop")]
        public int Loop;
        /// <summary>
        ///  Base64 string of the mp3 sound.
        /// </summary>
        [XmlElement("base64")]
        public string Base64;
    }

        /// <summary>
        ///  Information about the moves.
        /// </summary>
        /// <remarks>
        ///  There are 2 types: start and end. If a sequence has 10 frame sequences, the other 8 frames will interpolate
        ///  the values from start and end.
        /// </remarks>
    public class MovingNode
    {
            /// <summary>
            ///  How many pixels to move in the X axis.
            /// </summary>
        [XmlElement("x")]
        public string X;
            /// <summary>
            ///  How many pixels to move in the Y axis.
            /// </summary>
        [XmlElement("y")]
        public string Y;
            /// <summary>
            ///  Graphical offset, from the physical position calculated for the pet.
            /// </summary>
        [XmlElement("offsety")]
        public int OffsetY;
            /// <summary>
            ///  Opacity from 0.0 (invisible) to 1.0 (opaque).
            /// </summary>
        [XmlElement("opacity")]
        public double Opacity=1.0;
            /// <summary>
            ///  Interval until the next sequence is executed.
            /// </summary>
        [XmlElement("interval")]
        public string Interval;
    }

        /// <summary>
        ///  Sequence node.
        /// </summary>
    public class SequenceNode
    {
            /// <summary>
            ///  If repeat is > 0, repeat from indicate from which frame it should be repeated.
            /// </summary>
        [XmlAttribute("repeatfrom")]
        public int RepeatFromFrame;
            /// <summary>
            ///  How many times the sequence should be executed (value of 0 or 1 is NO-REPEAT).
            /// </summary>
        [XmlAttribute("repeat")]
        public string RepeatCount;
            /// <summary>
            ///  An array of images to show for this sequence.
            /// </summary>
        [XmlElement("frame")]
        public int[] Frame;
            /// <summary>
            ///  The next animation, once the sequence is over.
            /// </summary>
        [XmlElement("next")]
        public NextNode[] Next;
            /// <summary>
            ///  Action to execute if this animation is over.
            /// </summary>
        [XmlElement("action")]
        public string Action;
    }

        /// <summary>
        ///  Hit node indicate an array of next animations if the pet hit a border or has no gravity.
        /// </summary>
    public class HitNode
    {
            /// <summary>
            ///  List of next animations.
            /// </summary>
        [XmlElement("next")]
        public NextNode[] Next;
    }

        /// <summary>
        ///  Next animation to play. Animation, Border or Gravity have 0 or more Next-nodes.
        /// </summary>
    public class NextNode
    {
            /// <summary>
            ///  Probability this will be the next animation.
            /// </summary>
        [XmlAttribute("probability")]
        public int Probability;
            /// <summary>
            ///  Only flag, <see cref="TNextAnimation.TOnly"/>.
            /// </summary>
        [XmlAttribute("only")]
        public string OnlyFlag;
            /// <summary>
            ///  Next animation ID.
            /// </summary>
        [XmlText]
        public int Value;
    }

        /// <summary>
        /// Sprite sheet (PNG with all possible positions).
        /// </summary>
    public struct Images
    {
            /// <summary>
            /// Memory stream containing the PNG sprite sheet.
            /// </summary>
        public MemoryStream bitmapImages;
    };

        /// <summary>
        /// Xml class contains all functions to read the XML file and functions to parse it.
        /// </summary>
    public sealed class Xml : IDisposable
    {
            /// <summary>
            /// XML Document, containing the animations xml.
            /// </summary>
        public RootNode AnimationXML;

            /// <summary>
            /// XML String, used for the current running animation.
            /// </summary>
        public string AnimationXMLString;

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
            bool bError = false;
            // Construct an instance of the XmlSerializer with the type
            // of object that is being deserialized.
            XmlSerializer mySerializer = new XmlSerializer(typeof(RootNode));
            // To read the file, create a FileStream.
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            // Try to load local XML
            try
            {
                if(File.Exists(Application.StartupPath + "\\installpet.xml"))
                {
                    string sXML = System.Text.Encoding.Default.GetString(File.ReadAllBytes(Application.StartupPath + "\\installpet.xml"));
                    File.Delete(Application.StartupPath + "\\installpet.xml");
                    writer.Write(sXML);
                    AnimationXMLString = sXML;
                    Properties.Settings.Default.xml = sXML;
                    Properties.Settings.Default.Save();
                }
                else if (Program.ArgumentLocalXML != "")
                {
                    string sXML = System.Text.Encoding.Default.GetString(File.ReadAllBytes(Program.ArgumentLocalXML));
                    writer.Write(sXML);
                    AnimationXMLString = sXML;
                }
                else if (Program.ArgumentWebXML != "")
                {
                    System.Net.WebClient client = new System.Net.WebClient();
                    string sXML = client.DownloadString(Program.ArgumentWebXML);
                    writer.Write(sXML);
                    AnimationXMLString = sXML;
                }
                else
                {
                    writer.Write(Properties.Settings.Default.xml);
                    AnimationXMLString = Properties.Settings.Default.xml;
                }

                    // Don't load personal pets anymore
                Program.ArgumentLocalXML = "";  
                Program.ArgumentWebXML = "";

                //writer.Write(Properties.Resources.animations);
                writer.Flush();
                stream.Position = 0;
                // Call the Deserialize method and cast to the object type.
                AnimationXML = (RootNode)mySerializer.Deserialize(stream);

                Properties.Settings.Default.Images = AnimationXML.Image.Png;
                Properties.Settings.Default.Icon = AnimationXML.Header.Icon;
            }
            catch(Exception ex)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "User XML error: " + ex.ToString());
                if (Properties.Settings.Default.xml.Length > 100)
                {
                    MessageBox.Show("Error parsing animation XML:" + ex.ToString(), "XML error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                
                writer.Write(Properties.Resources.animations);
                writer.Flush();
                AnimationXMLString = Properties.Resources.animations;
                stream.Position = 0;
                // Call the Deserialize method and cast to the object type.
                AnimationXML = (RootNode)mySerializer.Deserialize(stream);

                Properties.Settings.Default.Images = AnimationXML.Image.Png;
                Properties.Settings.Default.Icon = AnimationXML.Header.Icon;
            }
            finally
            {
                try
                {
                    readImages();

                    if (AnimationXML.Header.Petname.Length > 16) AnimationXML.Header.Petname = AnimationXML.Header.Petname.Substring(0, 16);
                }
                catch (Exception ex)
                {
                    StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "Error reading XML: " + ex.Message);
                    bError = true;
                }
            }

            if (bError)
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
                // for each animation
            foreach (AnimationNode node in AnimationXML.Animations.Animation)
            {
                TAnimation ani = animations.AddAnimation(node.Id, node.Id.ToString());
                ani.Border = node.Border != null;
                ani.Gravity = node.Gravity != null;

                ani.Name = node.Name;
                switch (ani.Name)
                {
                    case "fall": animations.AnimationFall = node.Id; break;
                    case "drag": animations.AnimationDrag = node.Id; break;
                    case "kill": animations.AnimationKill = node.Id; break;
                    case "sync": animations.AnimationSync = node.Id; break;
                }

                ani.Start.X = getXMLCompute(node.Start.X);
                ani.Start.Y = getXMLCompute(node.Start.Y);
                ani.Start.Interval = getXMLCompute(node.Start.Interval);
                ani.Start.OffsetY = node.Start.OffsetY;
                ani.Start.Opacity = node.Start.Opacity;

                ani.End.X = getXMLCompute(node.End.X);
                ani.End.Y = getXMLCompute(node.End.Y);
                ani.End.Interval = getXMLCompute(node.End.Interval);
                ani.End.OffsetY = node.End.OffsetY;
                ani.End.Opacity = node.End.Opacity;

                ani.Sequence.RepeatFrom = node.Sequence.RepeatFromFrame;
                ani.Sequence.Action = node.Sequence.Action;
                ani.Sequence.Repeat = getXMLCompute(node.Sequence.RepeatCount);
                foreach (int frameid in node.Sequence.Frame)
                {
                    ani.Sequence.Frames.Add(frameid);
                }
                if (ani.Sequence.RepeatFrom > 0)
                    ani.Sequence.TotalSteps = ani.Sequence.Frames.Count + (ani.Sequence.Frames.Count - ani.Sequence.RepeatFrom - 1) * ani.Sequence.Repeat.Value;
                else
                    ani.Sequence.TotalSteps = ani.Sequence.Frames.Count + ani.Sequence.Frames.Count * ani.Sequence.Repeat.Value;
                if (node.Sequence.Next != null)
                {
                    foreach (NextNode nextNode in node.Sequence.Next)
                    {
                        TNextAnimation.TOnly where;
                        switch (nextNode.OnlyFlag)
                        {
                            case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                            case "window": where = TNextAnimation.TOnly.WINDOW; break;
                            case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                            case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                            case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                            default: where = TNextAnimation.TOnly.NONE; break;
                        }

                        ani.EndAnimation.Add(
                            new TNextAnimation(
                                nextNode.Value,
                                nextNode.Probability,
                                where
                            )
                        );
                    }
                }

                if (ani.Border)
                {
                    foreach (NextNode nextNode in node.Border.Next)
                    {
                        TNextAnimation.TOnly where;
                        switch (nextNode.OnlyFlag)
                        {
                            case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                            case "window": where = TNextAnimation.TOnly.WINDOW; break;
                            case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                            case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                            case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                            default: where = TNextAnimation.TOnly.NONE; break;
                        }
                        ani.Border = true;
                        ani.EndBorder.Add(
                            new TNextAnimation(
                                nextNode.Value,
                                nextNode.Probability,
                                where
                            )
                        );
                    }
                }

                if (ani.Gravity)
                {
                    foreach (NextNode nextNode in node.Gravity.Next)
                    {
                        TNextAnimation.TOnly where;
                        switch (nextNode.OnlyFlag)
                        {
                            case "taskbar": where = TNextAnimation.TOnly.TASKBAR; break;
                            case "window": where = TNextAnimation.TOnly.WINDOW; break;
                            case "horizontal": where = TNextAnimation.TOnly.HORIZONTAL; break;
                            case "horizontal+": where = TNextAnimation.TOnly.HORIZONTAL_; break;
                            case "vertical": where = TNextAnimation.TOnly.VERTICAL; break;
                            default: where = TNextAnimation.TOnly.NONE; break;
                        }
                        ani.Gravity = true;
                        ani.EndGravity.Add(
                            new TNextAnimation(
                                nextNode.Value,
                                nextNode.Probability,
                                where
                            )
                        );
                    }
                }
                
                animations.SaveAnimation(ani, node.Id);
            }

            // for each spawn
            if (AnimationXML.Spawns.Spawn != null)
            {
                foreach (SpawnNode node in AnimationXML.Spawns.Spawn)
                {
                    TSpawn ani = animations.AddSpawn(
                        node.Id,
                        node.Probability);

                    ani.Start.X = getXMLCompute(node.X);
                    ani.Start.Y = getXMLCompute(node.Y);
                    ani.Next = node.Next.Value;

                    animations.SaveSpawn(ani, node.Id);
                }
            }

            // for each child
            if (AnimationXML.Childs.Child != null)
            {
                foreach (ChildNode node in AnimationXML.Childs.Child)
                {
                    TChild aniChild = animations.AddChild(node.Id);
                    aniChild.AnimationID = node.Id;

                    aniChild.Position.X = getXMLCompute(node.X);
                    aniChild.Position.Y = getXMLCompute(node.Y);
                    aniChild.Next = node.Next;

                    animations.SaveChild(aniChild, node.Id);
                }
            }

            // for each sound
            if (AnimationXML.Sounds != null && AnimationXML.Sounds.Sound != null)
            {
                foreach (SoundNode node in AnimationXML.Sounds.Sound)
                {
                    animations.AddSound(node.Id, node.Probability, node.Loop, node.Base64);
                }
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
            v.IsDynamic = (v.Compute.IndexOf("random") >= 0 || v.Compute.IndexOf("randS") >= 0 || v.Compute.IndexOf("imageX") >= 0 || v.Compute.IndexOf("imageY") >= 0);
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

                // When adding a child, it is important to place the child on the other side of the parent, if the parent is flipped.
            if(parentFlipped)
            {
                if (sText.IndexOf("-imageW") >= 0)
                {
                    sText = sText.Replace("-imageW", "+imageW");
                }
                else
                {
                    sText = sText.Replace("imageW", "(-imageW)");
                }
            }
            sText = sText.Replace("screenW", Screen.PrimaryScreen.Bounds.Width.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("screenH", Screen.PrimaryScreen.Bounds.Height.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("areaW", Screen.PrimaryScreen.WorkingArea.Width.ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("areaH", (Screen.PrimaryScreen.WorkingArea.Height + Screen.PrimaryScreen.WorkingArea.Y).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageW", (FullImage.Width / AnimationXML.Image.TilesX).ToString(CultureInfo.InvariantCulture));
            sText = sText.Replace("imageH", (FullImage.Height / AnimationXML.Image.TilesY).ToString(CultureInfo.InvariantCulture));
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
                    Properties.Settings.Default.Images = AnimationXML.Image.Png;
                    int mod4 = Properties.Settings.Default.Images.Length % 4;
                    if (mod4 > 0)
                    {
                        Properties.Settings.Default.Images += new string('=', 4 - mod4);
                    }
                    Properties.Settings.Default.Save();
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
                    Properties.Settings.Default.Icon = AnimationXML.Header.Icon;
                    int mod4 = Properties.Settings.Default.Icon.Length % 4;
                    if (mod4 > 0)
                    {
                        Properties.Settings.Default.Icon += new string('=', 4 - mod4);
                    }
                    Properties.Settings.Default.Save();
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
