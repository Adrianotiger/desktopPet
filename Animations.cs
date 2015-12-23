using System;
using System.Collections.Generic;
using System.Linq;

namespace desktopPet
{
    public struct TValue
    {
        public bool Random;
        public string Compute;
        public int Value;
        
        public int GetValue()
        {
            if (Random)
            {
                return Animations.Xml.parseValue(Compute);
            }
            else
            {
                return Value;
            }
        }
    }

    public struct TMovement
    {
        public TValue X;
        public TValue Y;
        public TValue Interval;
    }

    public struct TNextAnimation
    {
        public enum TOnly
        {
            NONE        = 0x7F, // no flag
            TASKBAR     = 0x01, // only taskbar
            WINDOW      = 0x02, // only window
            HORIZONTAL  = 0x04, // only horizontal screen borders
            HORIZONTAL_ = 0x06, // horizontal screen borders and window
            VERTICAL    = 0x08, // only vertical screen borders 
        }
        public int ID;
        public int Probability;
        public TOnly only;
        public TNextAnimation(int id, int probability, TOnly where) 
        { 
            ID = id; 
            Probability = probability;
            only = where;
        }
    }

    public struct TSequence
    {
        public TValue Repeat;
        public int RepeatFrom;
        public List<int> Frames;
        public int TotalSteps { get; set; }
        public string Action;

        public int CalculateTotalSteps()
        {
            return Frames.Count + (Frames.Count - RepeatFrom) * Repeat.GetValue();
        }
    }

    public struct TAnimation
    {
        public TMovement Start;
        public TMovement End;
        public string Name;
        public List<TNextAnimation> EndAnimation;
        public List<TNextAnimation> EndBorder;
        public List<TNextAnimation> EndGravity;
        public List<TNextAnimation> EndWindow;
        public TSequence Sequence;
        public bool Gravity;
        public bool Border;
        public int ID;

        public TAnimation(string name, int id)
        {
            Start = new TMovement();
            End = new TMovement();
            Name = name;
            EndAnimation = new List<TNextAnimation>(8);
            EndBorder = new List<TNextAnimation>(8);
            EndGravity = new List<TNextAnimation>(8);
            EndWindow = new List<TNextAnimation>(8);
            Sequence = new TSequence();
            Sequence.Frames = new List<int>(16);
            Gravity = false;
            Border = false;
            ID = id;
        }
    }

    public struct TSpawn
    {
        public TMovement Start;
        public string Name;
        public int Probability;
        public bool MoveLeft;
        public int Next;

        public TSpawn(string name, int probability)
        {
            Start = new TMovement();
            Name = name;
            Probability = probability;
            MoveLeft = true;
            Next = 1;
        }
    }

    public class Animations
    {
        public Dictionary<int, TAnimation> SheepAnimations;
        public Dictionary<int, TSpawn> SheepSpawn;

        Random rand;
        public static Xml Xml;
        
        public int AnimationDrag = 1;
        public int AnimationFall = 1;
        public int AnimationKill = 1;
        public int AnimationSync = 1;

        public Animations(Xml xml)
        {
            SheepAnimations = new Dictionary<int, TAnimation>(64);   // Reserve space for 64 animations, more are added automatically
            SheepSpawn = new Dictionary<int, TSpawn>(8);
            rand = new Random();
            Xml = xml;
        }

        public TAnimation AddAnimation(int ID, string name)
        {
            try
            {
                SheepAnimations.Add(ID, new TAnimation(name, ID));
                Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "adding animation: " + name);
            }
            catch(Exception ex)
            {
                Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "unable to add animation: " + ex.Message);
            }
            return SheepAnimations[ID];
        }

        public void SaveAnimation(TAnimation animation, int ID)
        {
            SheepAnimations[ID] = animation;
        }

        public TSpawn AddSpawn(int ID, int probability, string name)
        {
            Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "adding spawn: " + name);
            SheepSpawn.Add(ID, new TSpawn(name, probability));
            return SheepSpawn[ID];
        }

        public void SaveSpawn(TSpawn spawn, int ID)
        {
            SheepSpawn[ID] = spawn;
        }

        public TSpawn GetRandomSpawn()
        {
            int percent = 0;
            int randValue;
            foreach (TSpawn spawn in SheepSpawn.Values)
            {
                percent += spawn.Probability;
            }
            randValue = rand.Next(0, percent);

            percent = 0;
            foreach (TSpawn spawn in SheepSpawn.Values)
            {
                percent += spawn.Probability;
                if (percent >= randValue)
                {
                    return spawn;
                }
            }
            return SheepSpawn.First().Value;
        }

        public TAnimation GetAnimation(int id)
        {
            return SheepAnimations[id];
        }

        public int SetNextBorderAnimation(int animationID, TNextAnimation.TOnly where)
        {
            Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "border detected");
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndBorder, where);
        }

        public int SetNextSequenceAnimation(int animationID, TNextAnimation.TOnly where)
        {
            Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "animation is over");
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndAnimation, where);
        }

        public int SetNextGravityAnimation(int animationID, TNextAnimation.TOnly where)
        {
            Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "gravity detected");
            return SetNextGeneralAnimation(SheepAnimations[animationID].EndGravity, where);
        }

        private int SetNextGeneralAnimation(List<TNextAnimation> list, TNextAnimation.TOnly where)
        {
            int iDefaultID = -1;
            if (list.Count > 0)
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
                if (iDefaultID > 0)
                {
                    UpdateAnimationValues(iDefaultID);
                }
                return iDefaultID;
            }
            else
            {
                Form1.AddDebugInfo(Form1.DEBUG_TYPE.error, "no next animation found");
                UpdateAnimationValues(list[0].ID);
                return list[0].ID;
            }
        }

        private void UpdateAnimationValues(int id)
        {
            bool bUpdated = false;
            TAnimation ani = SheepAnimations[id];
            if (ani.Sequence.Repeat.Random)
            {
                ani.Sequence.TotalSteps = ani.Sequence.CalculateTotalSteps();
                bUpdated = true;
            }
            if(ani.Start.Interval.Random || ani.Start.X.Random || ani.Start.Y.Random)
            {
                ani.Start.Interval.Value = ani.Start.Interval.GetValue();
                ani.Start.X.Value = ani.Start.X.GetValue();
                ani.Start.Y.Value = ani.Start.Y.GetValue();
                bUpdated = true;
            }
            if (ani.End.Interval.Random || ani.End.X.Random || ani.End.Y.Random)
            {
                ani.End.Interval.Value = ani.End.Interval.GetValue();
                ani.End.X.Value = ani.End.X.GetValue();
                ani.End.Y.Value = ani.End.Y.GetValue();
                bUpdated = true;
            }

            if (bUpdated)
            {
                SheepAnimations[id] = ani;
            }

            Form1.AddDebugInfo(Form1.DEBUG_TYPE.info, "new animation: " + ani.Name);
        }
    }
}
