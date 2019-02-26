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
using System.Diagnostics;
using System.Collections.Generic;

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
            /// List of sprite images for animations.
            /// </summary>
        public IList<Bitmap> sprites;

            /// <summary>
            /// Width of sprite in pixels.
            /// </summary>
        public int spriteWidth;

            /// <summary>
            /// Height of sprite in pixels.
            /// </summary>
        public int spriteHeight;

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
            /// Random spawn, this value changes each time the XML is reloaded. Used in the animation xml.
            /// </summary>
        int iRandomSpawn = 10;

        private LocalData.LocalData myData = new LocalData.LocalData();

            /// <summary>
            /// Constructor. Initialize member variables.
            /// </summary>
        public Xml()
        {
            sprites = new List<Bitmap>();

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

            foreach (var sprite in sprites)
            {
                if (sprite != null) sprite.Dispose();
            }
            sprites.Clear();
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
                writer.Write(Program.MyData.GetXml());
                AnimationXMLString = Program.MyData.GetXml();
             
                //writer.Write(Properties.Resources.animations);
                writer.Flush();
                stream.Position = 0;
                // Call the Deserialize method and cast to the object type.
                AnimationXML = (RootNode)mySerializer.Deserialize(stream);

                stream.Close();

                Program.MyData.SetImages(AnimationXML.Image.Png);
                Program.MyData.SetIcon(AnimationXML.Header.Icon);
            }
            catch(Exception ex)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "User XML error: " + ex.ToString());
                if (Program.MyData.GetXml().Length > 100)
                {
                    MessageBox.Show("Error parsing animation XML:" + ex.ToString(), "XML error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                stream.Flush();
                stream.Position = 0;
                writer.Write(Properties.Resources.animations);
                writer.Flush();
                AnimationXMLString = Properties.Resources.animations;
                stream.Position = 0;
                // Call the Deserialize method and cast to the object type.
                AnimationXML = (RootNode)mySerializer.Deserialize(stream);

                Program.MyData.SetImages(AnimationXML.Image.Png);
                Program.MyData.SetIcon(AnimationXML.Header.Icon);
            }
            finally
            {
                // if the images were loaded from external make some memory available.
                // don't need it again as its in Properties.Settings.Default.Images
                AnimationXML.Image.Png = string.Empty;
                // don't need it again as its in Properties.Settings.Default.Icon
                AnimationXML.Header.Icon = string.Empty; 
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
            if(AnimationXML.Animations == null)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "No animations for this pet");
                return;
            }
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

                ani.Start.X = getXMLCompute(node.Start.X, "animation " + node.Id + ": node.start.X");
                ani.Start.Y = getXMLCompute(node.Start.Y, "animation " + node.Id + ": node.start.Y");
                ani.Start.Interval = getXMLCompute(node.Start.Interval, "animation " + node.Id + ": node.start.Interval");
                ani.Start.OffsetY = node.Start.OffsetY;
                ani.Start.Opacity = node.Start.Opacity;

                ani.End.X = getXMLCompute(node.End.X, "animation " + node.Id + ": node.end.X");
                ani.End.Y = getXMLCompute(node.End.Y, "animation " + node.Id + ": node.end.Y");
                ani.End.Interval = getXMLCompute(node.End.Interval, "animation " + node.Id + ": node.end.Interval");
                ani.End.OffsetY = node.End.OffsetY;
                ani.End.Opacity = node.End.Opacity;

                ani.Sequence.RepeatFrom = node.Sequence.RepeatFromFrame;
                ani.Sequence.Action = node.Sequence.Action;
                ani.Sequence.Repeat = getXMLCompute(node.Sequence.RepeatCount, "animation " + node.Id + ": node.sequence.Repeat");
                ani.Sequence.Frames.AddRange(node.Sequence.Frame);
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

                    ani.Start.X = getXMLCompute(node.X, "spawn " + node.Id + ": node.X");
                    ani.Start.Y = getXMLCompute(node.Y, "spawn " + node.Id + ": node.X");
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

                    aniChild.Position.X = getXMLCompute(node.X, "child " + node.Id + ": node.X");
                    aniChild.Position.Y = getXMLCompute(node.Y, "child " + node.Id + ": node.Y");
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
            /// <param name="debugInfo">Info text to show if this function fails.</param>
            /// <returns>A structure with the values.</returns>
        public TValue getXMLCompute(string text, string debugInfo)
        {
            TValue v;

            v.Compute = text;
            v.IsDynamic = (v.Compute.IndexOf("random") >= 0 || v.Compute.IndexOf("randS") >= 0 || v.Compute.IndexOf("imageX") >= 0 || v.Compute.IndexOf("imageY") >= 0);
            v.Value = parseValue(v.Compute, debugInfo);

            return v;
        }

        /// <summary>
        /// Parse a value, converting keys like screenW, imageH, random,... to integers.
        /// </summary>
        /// <remarks>
        /// See <a href="https://msdn.microsoft.com/en-us/library/9za5w1xw(v=vs.100).aspx">https://msdn.microsoft.com/en-us/library/9za5w1xw(v=vs.100).aspx</a>
        /// for more information of what you can write as sText (expression to compute).
        /// </remarks> 
        /// <param name="parsingText">The text to parse and convert.</param>
        /// <param name="debugInfo">Debug text to show if this function fails.</param>
        /// <returns>The integer value from the parsed text expression.</returns>
        public int parseValue(string parsingText, string debugInfo)
        {
            int iRet = 0;
            DataTable dt = new DataTable();
            Random rand = new Random();

                // When adding a child, it is important to place the child on the other side of the parent, if the parent is flipped.
            if(parentFlipped)
            {
                if (parsingText.IndexOf("-imageW") >= 0)
                {
                    parsingText = parsingText.Replace("-imageW", "+imageW");
                }
                else
                {
                    parsingText = parsingText.Replace("imageW", "(-imageW)");
                }
            }
            parsingText = parsingText.Replace("screenW", Screen.PrimaryScreen.Bounds.Width.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("screenH", Screen.PrimaryScreen.Bounds.Height.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("areaW", Screen.PrimaryScreen.WorkingArea.Width.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("areaH", (Screen.PrimaryScreen.WorkingArea.Height + Screen.PrimaryScreen.WorkingArea.Y).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageW", spriteWidth.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageH", spriteHeight.ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageX", (parentX).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("imageY", (parentY).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("random", rand.Next(0, 100).ToString(CultureInfo.InvariantCulture));
            parsingText = parsingText.Replace("randS", iRandomSpawn.ToString(CultureInfo.InvariantCulture));
            
            var v = dt.Compute(parsingText, "");
            double dv;
            if (double.TryParse(v.ToString(), out dv))
            {
                iRet = (int)dv;
            }
            else
            {
				StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "Unable to parse integer: " + parsingText + " - " + debugInfo);
			}
            
            return iRet;
        }

            /// <summary>
            /// Read images from XML file and store them in the application.
            /// </summary>
        private void readImages()
        {
            MemoryStream imageStream = null;

            try
            {
                if (myData.GetImages().Length < 2) throw new InvalidDataException();
                imageStream = new MemoryStream(Convert.FromBase64String(myData.GetImages()));
                // only decode once so dont need to keep the source string for image
                Program.MyData.SetImages(string.Empty); 
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "user images loaded");
            }
            catch (Exception)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "user images not found, loading defaults");
                try
                {
                    string pngStr = AnimationXML.Image.Png;
                    int mod4 = pngStr.Length % 4;
                    if (mod4 > 0)
                    {
                        pngStr += new string('=', 4 - mod4);
                    }
                    Program.MyData.SetImages(pngStr);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    imageStream = new MemoryStream(Convert.FromBase64String(myData.GetImages()));
                    // only decode once so dont need to keep the source string for image
                    Program.MyData.SetImages(string.Empty);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            
            try
            {
                if (myData.GetIcon().Length < 100) throw new InvalidDataException();
                bitmapIcon = new MemoryStream(Convert.FromBase64String(myData.GetIcon()));
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "user icon loaded");
            }
            catch (Exception)
            {
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "no user icon, loading default");
                try
                {
                    var strIco = AnimationXML.Header.Icon;
                    int mod4 = strIco.Length % 4;
                    if (mod4 > 0)
                    {
                        strIco += new string('=', 4 - mod4);
                    }
                    Program.MyData.SetIcon(strIco);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
                try
                {
                    bitmapIcon = new MemoryStream(Convert.FromBase64String(myData.GetIcon()));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            }

            var image = new Bitmap(imageStream);
            // no longer need stream
            imageStream.Close();
            spriteWidth = image.Width / AnimationXML.Image.TilesX;
            spriteHeight = image.Height / AnimationXML.Image.TilesY;
            sprites = BuildSprites(image);
            // have sprites no longer need source sheet
            image.Dispose();
        }

        /// <summary>
        /// Build sprites from animation image
        /// </summary>
        /// <param name="spriteSheet"></param>
        /// <returns></returns>
        private IList<Bitmap> BuildSprites(Bitmap spriteSheet)
        {
            var sprites = new List<Bitmap>();

            for (var yOffset = 0; yOffset < spriteSheet.Height; yOffset += spriteHeight)
            {
                for (var xOffset = 0; xOffset < spriteSheet.Width; xOffset += spriteWidth)
                {
                    var bmpImage = new Bitmap(spriteWidth, spriteHeight, spriteSheet.PixelFormat);
                    var destRectangle = new Rectangle(0, 0, spriteWidth, spriteHeight);
                    using (var graphics = Graphics.FromImage(bmpImage))
                    {
                        var sourceRectangle = new Rectangle(xOffset, yOffset, spriteWidth, spriteHeight);
                        graphics.DrawImage(spriteSheet, destRectangle, sourceRectangle, GraphicsUnit.Pixel);
                    }
                    sprites.Add(bmpImage);
                }
            }
            return sprites;
        }
    }
}
