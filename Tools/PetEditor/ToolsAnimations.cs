using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;

namespace PetEditor
{
    /// <summary>
    /// In the XML you can write also strings, not only numbers.<br />
    /// So a movement or a number can also be dynamic.
    /// </summary>
    /// <remarks>
    /// Values are converted to integers in the application. But to allow flexibility, you can insert also some strings:<br />
    /// - screenW / screenH = width / height of screen<br />
    /// - areaW / areaH = width / height of area<br />
    /// - imageW / imageH = width / height of the image frame<br />
    /// - imageX / imageY = left / top position of the parent image<br />
    /// - random = a random number between 0 and 99 (inclusive)<br />
    /// - randS = a number between 0 and 99 (inclusive). This number doesn't change until next spawn<br />
    /// If you want discover more about what you can do, see <a href="https://msdn.microsoft.com/en-us/library/system.data.datacolumn.expression(v=vs.110).aspx">https://msdn.microsoft.com/en-us/library/system.data.datacolumn.expression(v=vs.110).aspx</a><br />
    /// </remarks>
    public struct TValue
    {
            /// <summary>
            /// If the parsed value contains a dynamic number
            /// </summary>
        public bool IsDynamic;
            /// <summary>
            /// If the parsed value contains a screen number (multiscreen have different sizes)
            /// </summary>
        public bool IsScreen;
            /// <summary>
            /// String with the expression to compute
            /// </summary>
        public string Compute;
            /// <summary>
            /// Computed value <see cref="Compute"/>
            /// </summary>
        public int Value;
        
            /// <summary>
            /// Get integer value from XML expression. IF expression is a string and contains the word "random",
            /// the returned value changes each time.
            /// </summary>
            /// <returns>The value parsed from xml file</returns>
        public int GetValue(int screenIndex = -1)
        {
            if (IsDynamic)
            {
                return Animations.Xml.ParseValue(Compute, "Animations.GetValue()");
            }
            else if(IsScreen && screenIndex >= 0)
            {
                return Animations.Xml.ParseValue(Compute, "Animations.GetValue()", screenIndex);
            }
            else
            {
                return Value;
            }
        }
    }

        /// <summary>
        /// Animation movement step (moves for each frame)
        /// </summary>
    public struct TMovement
    {
            /// <summary>
            /// Movement on the X axis
            /// </summary>
        public TValue X;
            /// <summary>
            /// Movement on the Y axis
            /// </summary>
        public TValue Y;
            /// <summary>
            /// Interval before the next step will be executed
            /// </summary>
        public TValue Interval;
            /// <summary>
            /// Move image from its position in the Y axis
            /// </summary>
        public int OffsetY;
            /// <summary>
            /// Opacity of the pet (0.0 = transparent, 1.0 = opaque)
            /// </summary>
        public double Opacity;
    }

    /// <summary>
    /// Information about next animation
    /// </summary>
    /// <remarks>
    /// On each animation sequence, there are 3 different NEXT:<br />
    /// 1- end of sequence<br />
    /// 2- gravity detected<br />
    /// 3- border detected<br />
    /// If all frames where played, Next-"end of sequence" will be executed.<br />
    /// If Next-"gravity" is set, pet will fall if no gravity is detected.<br />
    /// If a border is detected, Next-"border" will be executed.<br />
    /// <b>Note: if sequence is over or border was detected but you don't have a next statement for it, pet will re-spawn!</b><br />
    /// </remarks>
    public struct TNextAnimation
    {
            /// <summary>
            /// Enumeration about the Next structure.
            /// You can limit the next function to a state:
            /// </summary>
        public enum TOnly
        {
                /// <summary>
                /// No flag - is taken as next animation
                /// </summary>
            NONE        = 0x7F,
                /// <summary>
                /// Only taskbar - next animation will be executed only if pet is on the taskbar
                /// </summary>
            TASKBAR     = 0x01,
                /// <summary>
                /// Only window - next animation will be executed only if pet is on a window
                /// </summary>
            WINDOW      = 0x02,
                /// <summary>
                /// Only horizontal screen borders - next animation will be executed only if pet is on the top or bottom
                /// </summary>
            HORIZONTAL  = 0x04,
                /// <summary>
                /// Horizontal or Window borders - net animation will be executed only if pet detected an horizontal border
                /// </summary>
            HORIZONTAL_ = 0x06,
                /// <summary>
                /// Vertical screen borders - next animation will be executed only if pet is on the left or right screen border
                /// </summary>
            VERTICAL    = 0x08,
        }
            /// <summary>
            /// ID of the next animation to play
            /// </summary>
        public int ID;
            /// <summary>
            /// Probability the next animation will be executed:
            /// If there are 3 Next statements wit probability 5,12,3 probabilities are: 25%, 60% and 15%
            /// </summary>
        public int Probability;
            /// <summary>
            /// One of the values of TOnly. Default: NONE
            /// </summary>
        public TOnly only;
            /// <summary>
            /// Initialisation of the Next structure
            /// </summary>
            /// <param name="id">ID of the next animation</param>
            /// <param name="probability">Probability the next animation will be executed</param>
            /// <param name="where">Where the pet must be if you want this animation to be executed</param>
        public TNextAnimation(int id, int probability, TOnly where) 
        { 
            ID = id; 
            Probability = probability;
            only = where;
        }
    }

        /// <summary>
        /// Each sequence contains a defined quantity of image frames. An animation is based on this sequence.
        /// </summary>
    public struct TSequence
    {
            /// <summary>
            /// How many times the frames should be repeated until next animation is started
            /// </summary>
        public TValue Repeat;
            /// <summary>
            /// If <see cref="Repeat"/> is more than 1, you can set from which frame the sequence should be repeated.
            /// It is a 0 index based value.
            /// </summary>
        public int RepeatFrom;
            /// <summary>
            /// Frames index list. Contains all frames to play.
            /// </summary>
        public List<int> Frames;
            /// <summary>
            /// Total steps in the animation. Because Repeat and RepeatFrom can change the number of frames, this value will be calculated at beginning to increase the performance.
            /// </summary>
        public int TotalSteps { get; set; }
            /// <summary>
            /// A defined string. It can contains one of the fallowing values:
            /// 'flip': will flip all images and mirror the x-values in the animations
            /// </summary>
        public string Action;

            /// <summary>
            /// Calculate the steps present in this sequence. 
            /// This is used to calculate the movements, opacity and offset if they are different from START to END.
            /// </summary>
            /// <returns>Number of steps in the sequence.</returns>
        public int CalculateTotalSteps()
        {
            return Frames.Count + (Frames.Count - RepeatFrom) * Repeat.GetValue();
        }
    }

        /// <summary>
        /// Animation structure. This contains all information about an animation.
        /// </summary>
    public struct TAnimation
    {
            /// <summary>
            /// Movement values at beginning of the animation. Will be interpolated with the End structure.
            /// </summary>
        public TMovement Start;
            /// <summary>
            /// Movement values at the end of the animation. Will be interpolated with the Start structure.
            /// </summary>
        public TMovement End;
            /// <summary>
            /// Name of the animation. Used for debug purposes and to get the key animations
            /// </summary>
        public string Name;
            /// <summary>
            /// List of possible animations to execute, when this animation is over
            /// </summary>
        public List<TNextAnimation> EndAnimation;
            /// <summary>
            /// List of possible animations to execute, when the pet reach a border.
            /// </summary>
        public List<TNextAnimation> EndBorder;
            /// <summary>
            /// List of possible animations to execute, when the pet should fall.
            /// </summary>
        public List<TNextAnimation> EndGravity;
            /// <summary>
            /// Sequence of frames to play for this animation.
            /// </summary>
        public TSequence Sequence;
            /// <summary>
            /// If an animation for the gravity is set, the pet will fall if no window is detected.
            /// </summary>
        public bool Gravity;
            /// <summary>
            /// If an animation for the border is set, the pet will automatically jump to this animation if a border is detected.
            /// </summary>
        public bool Border;
            /// <summary>
            /// ID of the animation
            /// </summary>
        public int ID;

            /// <summary>
            /// Initialize the Animation structure
            /// </summary>
            /// <param name="name">name of the animation</param>
            /// <param name="id">ID of the animation</param>
        public TAnimation(string name, int id)
        {
            Start = new TMovement();
            End = new TMovement();
            Name = name;
            EndAnimation = new List<TNextAnimation>(8);
            EndBorder = new List<TNextAnimation>(8);
            EndGravity = new List<TNextAnimation>(8);
            Sequence = new TSequence();
            Sequence.Frames = new List<int>(16);
            Gravity = false;
            Border = false;
            ID = id;
        }
    }

        /// <summary>
        /// Spawn structure. Contains the info to start the first animation.
        /// </summary>
    public struct TSpawn
    {
            /// <summary>
            /// A start position for the pet on the screen
            /// </summary>
        public TMovement Start;
            /// <summary>
            /// Probability that this Spawn will be taken as start values.
            /// </summary>
        public int Probability;
            /// <summary>
            /// The next animation to play, once the position was set.
            /// </summary>
        public int Next;

            /// <summary>
            /// Initialisation of the Spawn structure
            /// </summary>
            /// <param name="probability">Probability that this will be the next spawn</param>
        public TSpawn(int probability)
        {
            Start = new TMovement();
            Probability = probability;
            Next = 1;
        }
    }

        /// <summary>
        /// Child structure. A second animation form can be started as child.
        /// </summary>
    public struct TChild
    {
            /// <summary>
            /// Position of the Child form.
            /// </summary>
        public TMovement Position;
            /// <summary>
            /// ID of the animation that should create this child.
            /// </summary>
        public int AnimationID;
            /// <summary>
            /// Next animation, once the child was created.
            /// </summary>
        public int Next;
    }

        /// <summary>
        /// Sound structure. A sound that can be played together with the animation.
        /// </summary>
    public struct TSound
    {
            /// <summary>
            /// ID of the animation that should create this child.
            /// </summary>
        public int AnimationID;
            /// <summary>
            /// Probability this sound will be played (in %).
            /// </summary>
        public int Probability;
            /// <summary>
            /// How many time the sound should be looped (1 = play 2 times).
            /// </summary>
        public int Loop;

        private static int LoopCount;

            /// <summary>
            /// Wave sound.
            /// </summary>
        private NAudio.Wave.WaveOut Audio;
        private NAudio.Wave.Mp3FileReader AudioReader;
        
        /// <summary>
        /// Load a sound for the animation from a byte array.
        /// </summary>
        /// <remarks>If this function fails, no sound will be played for this pet.</remarks>
        public void Load(byte[] buff)
        {
            try
            {
                MemoryStream ms = new MemoryStream(buff);
                AudioReader = new NAudio.Wave.Mp3FileReader(ms);
                Audio = new NAudio.Wave.WaveOut();
                Audio.Init(AudioReader);

                Audio.PlaybackStopped += Audio_PlaybackStopped;
            }
            catch(Exception e)
            {
                Program.AddLog("Unable to load MP3: " + e.Message, "Play pet", Program.LOG_TYPE.ERROR);
            }
        }

            /// <summary>
            /// Try to play the sound in the current animation.
            /// </summary>
            /// <param name="loopCount">How many times the sound should repeat. </param>
            /// <remarks>Sound is played only if the volume is greater than 0 and there are no sound problems.</remarks>
        public void Play(int loopCount)
        {
            try
            {
                // Set event handler only if looped
                if (loopCount > 0)
                {
                    LoopCount = loopCount;
                }
                AudioReader.Seek(0, SeekOrigin.Begin);
                Audio.Play();
            }
            catch(Exception)
            {
            }
        }

        private void Audio_PlaybackStopped(object sender, NAudio.Wave.StoppedEventArgs e)
        {
            if (e.Exception == null)
            {
                if (LoopCount-- > 0)
                    Audio.Play();
            }
        }
    }

        /// <summary>
        /// Animations class. Contains all information about the animations of the pet.
        /// </summary>
    public class Animations
    {   
            /// <summary>
            /// Each animation has a unique ID.
            /// </summary>
        public Dictionary<int, TAnimation> SheepAnimations;
            /// <summary>
            /// Each Spawn has a unique ID.
            /// </summary>
        public Dictionary<int, TSpawn> SheepSpawn;
            /// <summary>
            /// Each Child has a unique animation ID.
            /// </summary>
        public Dictionary<int, List<TChild>> SheepChild;
            /// <summary>
            /// Each Sound must have a unique animation ID.
            /// </summary>
        public Dictionary<int, TSound> SheepSound;

            /// <summary>
            /// Random used for the "random" key value in the xml.
            /// </summary>
        Random rand;
            /// <summary>
            /// A copy of the xml document
            /// </summary>
        public static Xml Xml;
        
            /// <summary>
            /// Animation ID once the pet is being dragged (default: 1)
            /// </summary>
        public int AnimationDrag = 1;
            /// <summary>
            /// Animation ID for the falling animation, after the dragged pet was released (default: 1)
            /// </summary>
        public int AnimationFall = 1;
            /// <summary>
            /// Animation ID once the pet should be closed (default: -1) 
            /// </summary>
        public int AnimationKill = -1;
            /// <summary>
            /// Animation ID once the cancel button on the about box was pressed (default: 1)
            /// </summary>
        public int AnimationSync = 1;

            /// <summary>
            /// Constructor, initialize member variables
            /// </summary>
            /// <param name="xml">Xml document</param>
        public Animations(Xml xml)
        {
            SheepAnimations = new Dictionary<int, TAnimation>(64);  // Reserve space for 64 animations, more are added automatically
            SheepSpawn = new Dictionary<int, TSpawn>(8);            // Reserve space for 8 spawns
            SheepChild = new Dictionary<int, List<TChild>>(8);      // Reserve space for 8 child
            SheepSound = new Dictionary<int, TSound>(8);            // Reserve space for 8 sounds
            rand = new Random();
            Xml = xml;
        }

        /// <summary>
        /// Add another animation to the animations dictionary. Animations are defined in the XML.
        /// <seealso cref="AddSpawn(int, int)"/>
        /// <seealso cref="AddChild(int)"/>
        /// </summary>
        /// <param name="ID">Animation unique ID</param>
        /// <param name="name">Animation name</param>
        /// <returns>Structure item (so it is possible to fill all values)</returns>
        public TAnimation AddAnimation(int ID, string name)
        {
            try
            {
                SheepAnimations.Add(ID, new TAnimation(name, ID));
            }
            catch(Exception)
            {
            }
            return SheepAnimations[ID];
        }

            /// <summary>
            /// After adding the animation and filling data, this function must be called to save values.
            /// </summary>
            /// <param name="animation">Structure of an animation.</param>
            /// <param name="ID">ID of the animation to save in.</param>
        public void SaveAnimation(TAnimation animation, int ID)
        {
            SheepAnimations[ID] = animation;
        }

            /// <summary>
            /// Add another spawn to the spawn dictionary. Spawns are defined in the XML.
            /// <seealso cref="AddAnimation(int, string)"/>
            /// <seealso cref="AddChild(int)"/>
            /// </summary>
            /// <param name="ID">Spawn unique ID.</param>
            /// <param name="probability">Probability this spawn will be taken.</param>
            /// <returns></returns>
        public TSpawn AddSpawn(int ID, int probability)
        {
            SheepSpawn.Add(ID, new TSpawn(probability));
            return SheepSpawn[ID];
        }

            /// <summary>
            /// After adding the spawn and filling data, this function must be called to save values.
            /// </summary>
            /// <param name="spawn">Filled structure.</param>
            /// <param name="ID">ID of the structure.</param>
        public void SaveSpawn(TSpawn spawn, int ID)
        {
            SheepSpawn[ID] = spawn;
        }

            /// <summary>
            /// Add another Child to the Child dictionary. Childs are defined in the XML.
            /// <seealso cref="AddAnimation(int, string)"/>
            /// <seealso cref="AddSpawn(int, int)"/>
            /// </summary>
            /// <param name="ID">Child unique ID.</param>
            /// <returns></returns>
        public TChild AddChild(int ID)
        {
			if (!SheepChild.ContainsKey(ID))  // does not contains childs
			{
				SheepChild.Add(ID, new List<TChild>(1));	
			}
			SheepChild[ID].Add(new TChild());
			return SheepChild[ID].Last();
		}

            /// <summary>
            /// After adding the Child and filling data, this function must be called to save values of the last child.
            /// </summary>
            /// <param name="child">Filled structure.</param>
            /// <param name="ID">ID of the structure.</param>
        public void SaveChild(TChild child, int ID)
        {
            SheepChild[ID][SheepChild[ID].Count-1] = child;
        }

        /// <summary>
        /// Add a sound to the sound dictionary. Sounds are defined in the XML.
        /// </summary>
        /// <param name="ID">Animation ID.</param>
        /// <param name="Probability">Probability this sound will be played with the animation sequence.</param>
        /// <param name="Loop">How many times the sound should be looped.</param>
        /// <param name="Base64">Base 64 string with the encoded mp3 file.</param>
        /// <returns></returns>
        public void AddSound(int ID, int Probability, int Loop, string Base64)
        {
            try
            {
                if (Base64.IndexOf(";base64,") > 0)
                    Base64 = Base64.Substring(Base64.IndexOf(";base64,") + 8);

                TSound sound = new TSound();
                sound.Load(Convert.FromBase64String(Base64));
                sound.AnimationID = ID;
                sound.Probability = Probability;
                sound.Loop = Loop;
                SheepSound.Add(ID, sound);
            }
            catch(Exception)
            {
                
            }
        }

        /// <summary>
        /// Calling this method, the next Spawn is returned.
        /// If more Spawns are defined, a random Spawn will be taken (based on the probability)
        /// </summary>
        /// <returns>Structure with the next Spawn values</returns>
        public TSpawn GetRandomSpawn()
        {
            int percent = 0;
            int randValue;
                // Calculate total probability
            foreach (TSpawn spawn in SheepSpawn.Values)
            {
                percent += spawn.Probability;
            }
                // Get random number
            randValue = rand.Next(0, percent);

                // Get the spawn, based on the random number
            percent = 0;
            foreach (TSpawn spawn in SheepSpawn.Values)
            {
                percent += spawn.Probability;
                if (percent >= randValue)
                {
                    return spawn;
                }
            }

                // If no spawn was returned, return the first spawn in the dictionary
            if (SheepSpawn.Count > 0)
            {
                return SheepSpawn.First().Value;
            }
            else
            {
                TSpawn retSpawn;
                if (SheepAnimations.Count > 0)
                    retSpawn.Next = SheepAnimations.First().Key;
                else
                    retSpawn.Next = 1;
                retSpawn.Probability = 100;
                retSpawn.Start.X.Compute = "0";
                retSpawn.Start.X.IsDynamic = false;
                retSpawn.Start.X.IsScreen = false;
                retSpawn.Start.X.Value = 0;
                retSpawn.Start.Y.Compute = "0";
                retSpawn.Start.Y.IsDynamic = false;
                retSpawn.Start.Y.IsScreen = false;
                retSpawn.Start.Y.Value = 0;
                retSpawn.Start.Opacity = 1.0;
                retSpawn.Start.Interval.Compute = "1000";
                retSpawn.Start.Interval.IsDynamic = false;
                retSpawn.Start.Interval.IsScreen = false;
                retSpawn.Start.Interval.Value = 1000;
                retSpawn.Start.OffsetY = 0;
                return retSpawn;
            }
        }

            /// <summary>
            /// Get the structure of the animation.
            /// </summary>
            /// <param name="id">ID of the wanted animation.</param>
            /// <returns>Structure with all information about this animation.</returns>
        public TAnimation GetAnimation(int id)
        {
			if(!SheepAnimations.ContainsKey(id))
            {
				TAnimation tempAnimation = new TAnimation("NULL", 0);
                tempAnimation.Start.Interval.Value = 1000;
                tempAnimation.End.Interval.Value = 1000;
            }
            return SheepAnimations[id];
        }

            /// <summary>
            /// Get the Childs connected to the Animation ID.
            /// </summary>
            /// <param name="id">ID of the Animation.</param>
            /// <returns>A list of childs structure of the current Animation.</returns>
        public List<TChild> GetAnimationChild(int id)
        {
            return SheepChild[id];
        }

            /// <summary>
            /// If the animation has a Child to play.
            /// </summary>
            /// <param name="id">ID of the Animation.</param>
            /// <returns>true if there is a Child to play. <see cref="GetAnimationChild(int)"/></returns>
        public bool HasAnimationChild(int id)
        {
            return SheepChild.ContainsKey(id);
        }

            /// <summary>
            /// Start the next animation once a border was detected.
            /// </summary>
            /// <param name="animationID">ID of the Animation.</param>
            /// <param name="where">Where the pet is "walking".</param>
            /// <returns>ID of the next animation to play. -1 if there is no animation.</returns>
        public int SetNextBorderAnimation(int animationID, TNextAnimation.TOnly where)
        {
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndBorder, where);
        }

            /// <summary>
            /// Start the next animation once the sequence was over.
            /// </summary>
            /// <param name="animationID">ID of the animation.</param>
            /// <param name="where">Where the pet is "walking"</param>
            /// <returns>ID of the next animation to play. -1 if there is no animation.</returns>
        public int SetNextSequenceAnimation(int animationID, TNextAnimation.TOnly where)
        {
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndAnimation, where);
        }

            /// <summary>
            /// Start the next animation once the gravity was detected.
            /// </summary>
            /// <param name="animationID">ID of the animation.</param>
            /// <param name="where">Where the pet is "walking"</param>
            /// <returns>ID of the next animation to play. -1 if there is no animation.</returns>
        public int SetNextGravityAnimation(int animationID, TNextAnimation.TOnly where)
        {
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndGravity, where);
        }

            /// <summary>
            /// Set the next animation, once the last one was finished.
            /// </summary>
            /// <param name="list">List of animations that can be executed.</param>
            /// <param name="where">Where the pet is "walking"</param>
            /// <returns>ID of the next animation to play. -1 if there is no animation.</returns>
        private int SetNextGeneralAnimation(List<TNextAnimation> list, TNextAnimation.TOnly where)
        {
            int iDefaultID = -1;
            if (list.Count > 0)     // Find the next animation only if there is at least 1 animation in the list
            {
                int iVal;
                int iSum = 0;
                int iRandMax = 0;
                foreach (TNextAnimation anim in list)
                {
                    if (anim.only != TNextAnimation.TOnly.NONE && (anim.only & where) == 0) continue;

                    iRandMax += anim.Probability;
                }
                iVal = rand.Next(0, iRandMax);
                foreach (TNextAnimation anim in list)
                {
                    if (anim.only != TNextAnimation.TOnly.NONE && (anim.only & where) == 0) continue;

                    iSum += anim.Probability;
                    if (iSum >= iVal)
                    {
                        iDefaultID = anim.ID;
                        break;
                    }
                }
                    // If an animation was found, re-calculate the values (if there are some Random values, they must be evaluated again)
                if (iDefaultID > 0)
                {
                    UpdateAnimationValues(iDefaultID);
                    if(SheepSound.ContainsKey(iDefaultID))
                    {
                        if (rand.Next(0, 100) < SheepSound[iDefaultID].Probability)
                        {
                            SheepSound[iDefaultID].Play(SheepSound[iDefaultID].Loop);
                        }
                    }
                }
                return iDefaultID;
            }
            else
            {
                return -1;  // a new spawn is requested
            }
        }

            /// <summary>
            /// Update the values of the animation.<br />
            /// If "random" was used, on each start of a new animation this will change so the expression must be evaluated again.<br />
            /// Total steps are also calculated, so it has a better performance by playing it.
            /// </summary>
            /// <param name="id">ID of the Animation.</param>
        private void UpdateAnimationValues(int id)
        {
            bool bUpdated = false;
            TAnimation ani = SheepAnimations[id];
            if (ani.Sequence.Repeat.IsDynamic)
            {
                    // Calculate the total steps, based on the repeat values.
                ani.Sequence.TotalSteps = ani.Sequence.CalculateTotalSteps();
                bUpdated = true;
            }
            if(ani.Start.Interval.IsDynamic || ani.Start.X.IsDynamic || ani.Start.Y.IsDynamic)
            {
                ani.Start.Interval.Value = ani.Start.Interval.GetValue();
                ani.Start.X.Value = ani.Start.X.GetValue();
                ani.Start.Y.Value = ani.Start.Y.GetValue();
                bUpdated = true;
            }
            if (ani.End.Interval.IsDynamic || ani.End.X.IsDynamic || ani.End.Y.IsDynamic)
            {
                ani.End.Interval.Value = ani.End.Interval.GetValue();
                ani.End.X.Value = ani.End.X.GetValue();
                ani.End.Y.Value = ani.End.Y.GetValue();
                bUpdated = true;
            }

                // If a value was changed, overwrite the old structure with the new one.
            if (bUpdated)
            {
                SheepAnimations[id] = ani;
            }
        }

            /// <summary>
            /// Get a list of animations, based on the flags forwarded by the parameters.
            /// </summary>
            /// <param name="currentID">The base animation to find the next animations.</param>
            /// <param name="includeNext">Include all animations after the sequence is over.</param>
            /// <param name="includeBorder">Include all animations if the pet detected a border.</param>
            /// <param name="includeGravity">Include all animations if the pet detected a gravity.</param>
            /// <returns></returns>
        public List<TNextAnimation> GetNextAnimations(int currentID, bool includeNext, bool includeBorder, bool includeGravity)
        {
            List<TNextAnimation> list = new List<TNextAnimation>();

            if (includeNext)
                list.AddRange(SheepAnimations[currentID].EndAnimation);
            if (includeBorder)
                list.AddRange(SheepAnimations[currentID].EndBorder);
            if (includeGravity)
                list.AddRange(SheepAnimations[currentID].EndGravity);

            return list;
        }

            /// <summary>
            /// Get a list of <see cref="TSpawn"/> structures. Defines the start position of the pet.
            /// </summary>
            /// <returns>List of TSpawn structures.</returns>
            /// <remarks>Once the animation is over or at begins, one of the spawns will be used to place the pet.</remarks>
        public List<TSpawn> GetNextSpawns()
        {
            List<TSpawn> list = new List<TSpawn>();

            for(int i = 0; i < SheepSpawn.Keys.Count; i++)
            {
                list.Add(SheepSpawn[SheepSpawn.Keys.ElementAt(i)]);
            }
            return list;
        }
    }
}
