using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Xml.Serialization;

namespace XmlData
{
    /// <summary>
    /// This Node class is used to store the XML data using the serialize function.
    /// </summary>
    /// <remarks>Once the XML was loaded, it is possible to see the header info in the about box.</remarks>
    [XmlRoot("animations", Namespace = "https://esheep.petrucci.ch/", IsNullable = false)]
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
        public double Opacity = 1.0;
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

    public class AnimationXML
    {
        public static RootNode ParseXML(string xml)
        {
            var aniXML = new RootNode();
            
            // Try to load local XML
            try
            {
                XmlSerializer mySerializer = new XmlSerializer(typeof(RootNode));

                using (MemoryStream stream = new MemoryStream())
                {
                    using (StreamWriter writer = new StreamWriter(stream))
                    {
                        writer.Write(xml);
                        writer.Flush();

                        stream.Position = 0;
                        aniXML = (RootNode)mySerializer.Deserialize(stream);
                    }
                    stream.Close();
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Error parsing XML: " + ex.Message);
                aniXML = null;
            }

            return aniXML;
        }
    }
}
