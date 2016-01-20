using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace desktopPet
{
    public partial class Form2 : Form
    {
            /// <summary>
            /// Current step in the animation-frames list.
            /// </summary>
            /// <remarks>
            /// Every animation has a defined quantity of steps. They are calculated from:
            /// - Quantity of frames
            /// - Repeat count and repeat from
            /// If an animation has 10 different frames and frame 5 to 9 are repeted 8 times, the total of steps is 10 + 5 * 8 = 50 steps.
            /// Once the last step was reached, the next animation will be started. If now animation is set, the SPAWN will be executed.
            /// </remarks>
        int iAnimationStep;
            /// <summary>
            /// Structure with all informations about the current animation.
            /// </summary>
        TAnimation CurrentAnimation;
            /// <summary>
            /// Handle to the current window. If this value is 0, the sheep is NOT walking on a window.
            /// </summary>
        IntPtr hwndWindow = (IntPtr)0;
            /// <summary>
            /// If sheep is walking to left  (default).
            /// </summary>
            /// <remarks>
            /// The original eSheep was a japanese application. So it was normal to see something from right to left.
            /// To leave the same characteristic, moveLeft is set to true. But it doesn't matter, because the sprite and movements gives the direction...
            /// </remarks>
        bool bMoveLeft = true;
            /// <summary>
            /// Animations class. The entire animation and its values are described here.
            /// </summary>
        Animations Animations;
            /// <summary>
            /// Xml class. Xml parser and functionality are stored here.
            /// </summary>
        Xml Xml;
            /// <summary>
            /// If the pet is in dragging mode (user is holding the pet with the mouse)
            /// </summary>
        bool bDragging = false;
            /// <summary>
            /// Offset Y - Sprite size is taken and not the single image. So, over the taskbar or over the windows, the pet could be 1-2 pixels over the border if you didn't drawn it on the bottom of the sprite frame.
            /// With this offset, you can re-place the pet or you can give them an offset so that it is positioned over the window (for example if you want to show a girl sitting over the taskbar, you need this function)
            /// </summary>
        int iOffsetY = 0;
            /// <summary>
            /// Current X position of the form. Because an offset can be used, this is the origin of the sprite (not like Form2.Left) before an offset was interpolated with the form position.
            /// </summary>
        int iPosX = 0;
            /// <summary>
            /// Current Y position of the form. Because an offset can be used, this is the origin of the sprite (not like Form2.Top) before an offset was interpolated with the form position.
            /// </summary>
        int iPosY = 0;

            /// <summary>
            /// Form constructor. This is never called. 
            /// Form2(Animations animations, Xml xml) -> Called when a new sheep is generated
            /// Form2(Animations animations, Xml xml, Point parentPos, bool parentFlipped) -> Called when a Child is generated
            /// </summary>
        public Form2()
        {
            InitializeComponent();
        }

            /// <summary>
            /// Form constructor.  Called when a new sheep is generated. 
            /// </summary>
            /// <param name="animations">Animation class, with all values.</param>
            /// <param name="xml">Xml class, with xml functions</param>
        public Form2(Animations animations, Xml xml)
        {
            Animations = animations;
            Xml = xml;
            InitializeComponent();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
        }

            /// <summary>
            /// Form constructor. Called when a Child is generated. 
            /// </summary>
            /// <param name="animations">Animation class, with all values.</param>
            /// <param name="xml">Xml class, with xml functions</param>
            /// <param name="parentPos">Position of the parent - used to detect where the child should be positioned</param>
            /// <param name="parentFlipped">If parent is flipped. If true, the child image will also be flipped</param>
        public Form2(Animations animations, Xml xml, Point parentPos, bool parentFlipped)
        {
            Animations = animations;
            Xml = xml;
            Xml.parentX = parentPos.X;
            Xml.parentY = parentPos.Y;
            Xml.parentFlipped = parentFlipped;
            bMoveLeft = !parentFlipped;
            InitializeComponent();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
        }

            /// <summary>
            /// Whith this overrided function, it is possible to remove the application from the ALT-TAB list.
            /// This, because it is not nice to see 10 times the same sheep when you press ALT-TAB (with 10 sheeps walking on your screen).
            /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                // Turn on WS_EX_TOOLWINDOW style bit
                CreateParams cp = base.CreateParams;
                cp.ExStyle |= 0x80;
                return cp;
            }
        }

            /// <summary>
            /// Once the form was created, this is the next function to call.
            /// It will set the size of the pet. Form will still be invisible because it has an opacity of 0.0.
            /// </summary>
            /// <param name="w">Single frame width</param>
            /// <param name="h">Single frame height</param>
        public void Show(int w, int h)
        {
            Width = w;
            Height = h;

            pictureBox1.Width = w;
            pictureBox1.Height = h;
            pictureBox1.Top = 0;
            pictureBox1.Left = 0;
            pictureBox1.Tag = 0;
            
            iAnimationStep = 0;

            Show();
        }

            /// <summary>
            /// Once the form was created and resized, this is the next function to call.
            /// This function is called for every single image in the sprite sheet and will be saved in the Image List component.
            /// </summary>
            /// <param name="im">Single frame image, beginning from top left to bottom right</param>
        public void addImage(Image im)
        {
            if(imageList1.Images.Count == 0)
            {
                imageList1.ImageSize = new Size(im.Width, im.Height);
            }
            imageList1.Images.Add(im);
        }

            /// <summary>
            /// Once the form was created, resized and all images was set, this is the next function to call.
            /// It will initialize all variables and start the first animation (SPAWN).
            /// </summary>
            /// <param name="first">If it is playing a spawn for the first time. Does not have any functionality for the moment.</param>
        public void Play(bool first)
        {
            timer1.Enabled = false;                     // Stop the timer

            iAnimationStep = 0;                         // First step
            hwndWindow = (IntPtr)0;                     // It is not over a window

            TSpawn spawn = Animations.GetRandomSpawn(); // Get a random SPAWN, to setting the form properties
            Top = spawn.Start.Y.GetValue();
            Left = spawn.Start.X.GetValue();
            iPosX = Left;
            iPosY = Top;
            iOffsetY = 0;
            Visible = true;                             // Now we can show the form
            Opacity = 1.0;
            SetNewAnimation(spawn.Next);                // Set next animation
            
            timer1.Enabled = true;                      // Enable the timer (interval is well known now)
        }

            /// <summary>
            /// If this form is a child, this function is called instead of Play().
            /// It will initialize all variables and start the first animation using CHILD, not SPAWN.
            /// </summary>
            /// <param name="aniID">Animation playing by the parent (child will synchronize to this animation).</param>
        public void PlayChild(int aniID)
        {
            TChild child = Animations.GetAnimationChild(aniID);

            timer1.Enabled = false;                     // Stop the timer
               
            iAnimationStep = 0;                         // First step
            hwndWindow = (IntPtr)0;                     // It is not over a window
            
            Top = child.Position.Y.GetValue();          // Set position. If parent is flipped, mirror the position
            if (bMoveLeft)
                Left = child.Position.X.GetValue();
            else
                Left = Screen.PrimaryScreen.Bounds.Width - child.Position.X.GetValue() - Width;
            iPosX = Left;
            iPosY = Top;
            iOffsetY = 0;
            Visible = true;                             // Now we can show this child
            Opacity = 1.0;
            SetNewAnimation(child.Next);                // Set next animation to play

            timer1.Enabled = true;                      // Enable timer (interval is known, now)
        }

            /// <summary>
            /// If application is closed, all forms have still 1 second to show something (change animation).
            /// </summary>
            /// <remarks>
            /// Kill, Sync, Drag and Fall are "Key-names" in the XML file. If you use one of them, this program will automatically run the animation linked to this names.
            /// </remarks>
        public void Kill()
        {
            if(Animations.AnimationKill > 1)
                SetNewAnimation(Animations.AnimationKill);
        }

            /// <summary>
            /// If user press the CANCEL button in the about box, all pets are synchronized executing the SYNC-animation.
            /// </summary>
            /// <remarks>
            /// Kill, Sync, Drag and Fall are "Key-names" in the XML file. If you use one of them, this program will automatically run the animation linked to this names.
            /// </remarks>
        public void Sync()
        {
            if (Animations.AnimationSync > 1)
                SetNewAnimation(Animations.AnimationSync);
        }

            /// <summary>
            /// Timer tick. The entire animation is drived through this timer. The interval is set in the XML animation file.
            /// </summary>
            /// <remarks>
            /// On each tick, the next step is called. If it fails an error message will be show and the animation will stop.
            /// </remarks>
        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (iAnimationStep < 0) iAnimationStep = 0;
            try
            {
                NextStep();
                iAnimationStep++;
                timer1.Enabled = true;
            }
            catch(Exception ex) // if form is closed timer could continue to tick (why?)
            {
                if(Name != "child")
                    MessageBox.Show("Fatal Error: " + ex.Message, "App error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
        }

        /*
        private int GetRandomNumber()
        {
            Random Rand = new Random();
            return Rand.Next(0, 100);
        }
        */ 

        private void SetNewAnimation(int id)
        {
            if (id < 0)  // no animation found, spawn!
            {
                Play(false);
            }
            else
            {
                iAnimationStep = -1;
                CurrentAnimation = Animations.GetAnimation(id);
                if(Animations.HasAnimationChild(id))
                {
                    if (Name != "child")
                    {
                        TChild childInfo = Animations.GetAnimationChild(id);
                        Form2 child = new Form2(Animations, Xml, new Point(Left, Top), !bMoveLeft);
                        for(int i=0;i<imageList1.Images.Count-1;i++)
                        {
                            child.addImage(imageList1.Images[i]);
                        }
                        child.Name = "child";
                        child.Show(Width, Height);
                        child.PlayChild(id);
                    }
                }
                timer1.Interval = CurrentAnimation.Start.Interval.GetValue();
            }
        }

        private void NextStep()
        {
            if (iAnimationStep < CurrentAnimation.Sequence.Frames.Count)
            {
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[iAnimationStep]];
            }
            else
            {
                int index = ((iAnimationStep - CurrentAnimation.Sequence.Frames.Count + CurrentAnimation.Sequence.RepeatFrom) % (CurrentAnimation.Sequence.Frames.Count - CurrentAnimation.Sequence.RepeatFrom)) + CurrentAnimation.Sequence.RepeatFrom;
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[index]]; 
            }

            timer1.Interval = CurrentAnimation.Start.Interval.Value + ((CurrentAnimation.End.Interval.Value - CurrentAnimation.Start.Interval.Value) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);
            Opacity = CurrentAnimation.Start.Opacity + ((CurrentAnimation.End.Opacity - CurrentAnimation.Start.Opacity) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);
            iOffsetY = CurrentAnimation.Start.OffsetY + ((CurrentAnimation.End.OffsetY - CurrentAnimation.Start.OffsetY) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);

            if (bDragging)
            {
                iPosX = Left = Cursor.Position.X - Width / 2;
                iPosY = Top = Cursor.Position.Y + 2;
                return;
            }

            int x = CurrentAnimation.Start.X.Value + ((CurrentAnimation.End.X.Value - CurrentAnimation.Start.X.Value) * iAnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1));
            int y = CurrentAnimation.Start.Y.Value + ((CurrentAnimation.End.Y.Value - CurrentAnimation.Start.Y.Value) * iAnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1));
            bool bNewAnimation = false;
            if (!bMoveLeft) x = -x;
            
            if(x < 0)   // moving left
            {
                if (hwndWindow == (IntPtr)0)
                {
                    if (iPosX + x < 0)    // left screen border!
                    {
                        x = -iPosX;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL));
                        bNewAnimation = true;
                    }
                }
                else
                {
                    NativeMethods.RECT rct;
                    if (NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (iPosX + x < rct.Left)    // left window border!
                        {
                            x = -iPosX + rct.Left;
                            SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                        }
                    }
                }
            }
            else if (x > 0)   // moving right
            {
                if (hwndWindow == (IntPtr)0)
                {
                    if (iPosX + x + Width > Screen.PrimaryScreen.WorkingArea.Width)    // right screen border!
                    {
                        x = Screen.PrimaryScreen.WorkingArea.Width - Width - iPosX;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL));
                        bNewAnimation = true;
                    }
                }
                else
                {
                    NativeMethods.RECT rct;
                    if (NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (iPosX + x + Width > rct.Right)    // right window border!
                        {
                            x = rct.Right - Width - iPosX;
                            SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                        }
                    }
                }
            }
            if(y > 0)   // moving down
            {
                if (CurrentAnimation.EndBorder.Count > 0)
                {
                    if (iPosY + y > Screen.PrimaryScreen.WorkingArea.Height - Height) // border detected!
                    {
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.TASKBAR));
                        bNewAnimation = true;
                        y = Screen.PrimaryScreen.WorkingArea.Height - iPosY - Height;
                    }
                    else
                    {
                        int iWindowTop = FallDetect(y);
                        if (iWindowTop > 0)
                        {
                            SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                            y = iWindowTop - iPosY - Height;
                        }
                    }
                }
            }

            if (iAnimationStep >= CurrentAnimation.Sequence.TotalSteps - 1) // animation over
            {
                int iNextAni = -1;
                if(CurrentAnimation.Sequence.Action == "flip")
                {
                    // flip all images
                    bMoveLeft = !bMoveLeft;
                    for (int i = 0; i < imageList1.Images.Count; i++)
                    {
                        Image im = imageList1.Images[i];
                        im.RotateFlip(RotateFlipType.RotateNoneFlipX);
                        imageList1.Images[i] = im;
                    }
                }
                if(hwndWindow != (IntPtr)0)
                {
                    iNextAni = Animations.SetNextSequenceAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                }
                else
                {
                    iNextAni = Animations.SetNextSequenceAnimation(CurrentAnimation.ID, iPosY + Height + y >= Screen.PrimaryScreen.WorkingArea.Height - 2 ? TNextAnimation.TOnly.TASKBAR : TNextAnimation.TOnly.NONE);
                }
                if (iNextAni >= 0)
                {
                    SetNewAnimation(iNextAni);
                    bNewAnimation = true;
                }
                else
                {
                    if(Name=="child")
                    {
                        timer1.Stop();
                        timer1.Enabled = false;
                        StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "removing child");
                        Hide();
                        Close();
                    }
                    else
                    {
                        Play(false);
                    }
                }
            }
            else if(CurrentAnimation.Gravity)
            {
                if(hwndWindow == (IntPtr)0)
                {
                    if(iPosY + y < Screen.PrimaryScreen.WorkingArea.Height - Height)
                    {
                        if(iPosY + y + 3 >= Screen.PrimaryScreen.WorkingArea.Height - Height) // allow 3 pixels to move without fall
                        {
                            y = Screen.PrimaryScreen.WorkingArea.Height - iPosY - Height;
                        }
                        else
                        {
                            SetNewAnimation(Animations.SetNextGravityAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.NONE));
                            bNewAnimation = true;
                        }
                    }
                }
                else
                {
                    if (iAnimationStep > 0 && CheckTopWindow(true))
                    {
                        hwndWindow = (IntPtr)0;
                        SetNewAnimation(Animations.SetNextGravityAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                        bNewAnimation = true;
                    }
                }
            }

            if(bNewAnimation)
            {
                timer1.Interval = CurrentAnimation.Start.Interval.Value;
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[0]];
            }

            iPosX += x;
            iPosY += y;

            Left = iPosX;
            Top = iPosY + iOffsetY;
        }

        private int FallDetect(int y)
        {
            NativeMethods.RECT rct;
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();
            NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

            NativeMethods.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == Handle) return true;

                if (NativeMethods.IsWindowVisible(hWnd))
                {
                    StringBuilder sTitle = new StringBuilder(128);
                    NativeMethods.GetWindowText(hWnd, sTitle, 128);

                    if (sTitle.ToString() == "Sheep") { }
                    else if (!NativeMethods.GetTitleBarInfo(hWnd, ref titleBarInfo)) return true;
                    else if ((titleBarInfo.rgstate[0] & 0x00008000) > 0) return true;    // invisible
                    
                    if (sTitle.Length > 0)
                    {
                        windows[hWnd] = sTitle.ToString();
                    }
                }
                return true;
            }, (IntPtr)0);

            foreach (KeyValuePair<IntPtr, string> window in windows)
            {
                if (NativeMethods.GetWindowRect(new HandleRef(this, window.Key), out rct))
                {
                    //Console.WriteLine("Window title: {0}", window.Value);

                    if (iPosY + Height < rct.Top && iPosY + Height + y >= rct.Top &&
                        iPosX >= rct.Left - Width / 2 && iPosX + Width <= rct.Right + Width / 2 &&
                        iPosY > 30)
                    {
                        hwndWindow = window.Key;
                        if (!CheckTopWindow(false))
                        {
                            NativeMethods.ShowWindow(window.Key, 0);
                            NativeMethods.ShowWindow(window.Key, 5);
                            return rct.Top;
                        }
                        else
                        {
                            hwndWindow = (IntPtr)0;
                        }
                    }
                }
            }
            return -1;
        }

        /*
        private void Exit1(int step)
        {
            if (step == 0)
            {
                pictureBox1.Image = imageList1.Images[12];
                this.Top += 5;
            }
            else if (step < 20)
            {
                if (step < 10) this.Top += 3;
                pictureBox1.Image = imageList1.Images[131 + (step % 2)];
            }
            else if (step < 50)
            {
                pictureBox1.Image = imageList1.Images[133];
                this.Top += 5;
            }
            else
            {
                this.Top = -40;
                this.Left = ((Screen.PrimaryScreen.WorkingArea.Width / 120) * GetRandomNumber()) + 100;
                this.TopMost = true;

                if (GetRandomNumber() < 2)
                {
                    SetNewAnimation(AnimationType.Water);
                }
                else
                {
                    SetNewAnimation(AnimationType.FallWinDown);
                }
            }
        }

        private void Water(int step)
        {
            if (step == 0)
            {
                this.TopMost = true;
                formX = new Form2();
                formX.addImage(imageList1.Images[146]);
                formX.addImage(imageList1.Images[147]);
                formX.addImage(imageList1.Images[148]);
                formX.Enabled = false;
                formX.Show();
                if (bMoveLeft)
                {
                    formX.Top = Screen.PrimaryScreen.WorkingArea.Height - 40;
                    formX.Left = (Screen.PrimaryScreen.WorkingArea.Width / 150) * GetRandomNumber() + 200;
                    this.Left = Screen.PrimaryScreen.WorkingArea.Width + 60;
                    this.Top = Screen.PrimaryScreen.WorkingArea.Height - (this.Left + 40 - formX.Left);
                }
                else
                {
                    formX.Top = Screen.PrimaryScreen.WorkingArea.Height - 40;
                    formX.Left = Screen.PrimaryScreen.WorkingArea.Width - (Screen.PrimaryScreen.WorkingArea.Width / 150) * GetRandomNumber() - 200;
                    this.Left = -60;
                    this.Top = Screen.PrimaryScreen.WorkingArea.Height - (-this.Left + 40 + formX.Left);
                }

                timer1.Interval = 40;

                formX.PlayWater((Screen.PrimaryScreen.WorkingArea.Height - this.Top) / 5 - 3);
                formX.SetTopLevel(true);
                formX.TopMost = true;
                this.SetTopLevel(true);
            }
            else if(step < 20)
            {
                pictureBox1.Image = imageList1.Images[134];
                if (bMoveLeft)
                    this.Left -= 5;
                else
                    this.Left += 5;
                this.Top += 5;
            }
            else if (134 + (step - 20) / 10 < 145)
            {
                pictureBox1.Image = imageList1.Images[134 + (step - 20) / 10];
                if (bMoveLeft)
                    this.Left -= 5;
                else
                    this.Left += 5;
                this.Top += 5;
            }
            else
            {
                if (this.Top >= Screen.PrimaryScreen.WorkingArea.Height - 45)
                    pictureBox1.Image = imageList1.Images[173];
                else
                    pictureBox1.Image = imageList1.Images[144 + ((step / 7) % 2)];

                if (bMoveLeft)
                    this.Left -= 5;
                else
                    this.Left += 5;
                this.Top += 5;

                if (this.Top >= Screen.PrimaryScreen.WorkingArea.Height)
                {
                    this.Top  = Screen.PrimaryScreen.WorkingArea.Height - 40;
                    if (bMoveLeft)
                        this.Left += 45;
                    else
                        this.Left -= 45;
                    SetNewAnimation(AnimationType.Walk);
                }
            }
            
        }

        private void WaterX(int step)
        {
            if (step < iWaterSteps - 2)
            {
                pictureBox1.Image = imageList1.Images[0];
            }
            else if (step == iWaterSteps - 2)
            {
                timer1.Interval = 150;
                pictureBox1.Image = imageList1.Images[1];
            }
            else if (step == iWaterSteps - 1)
            {
                pictureBox1.Image = imageList1.Images[2];
            }
            else if (step == iWaterSteps)
            {
                pictureBox1.Image = imageList1.Images[1];
            }
            else
            {
                this.Close();
            }
        }
        
        */
        
        private bool CheckTopWindow(bool bCheck)
        {
            if ((int)hwndWindow != 0)
            {
                NativeMethods.RECT rctO;
                NativeMethods.RECT rct;
                NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out rctO);

                if (bCheck)
                {
                    if (rctO.Top > iPosY + Height + 2) return true;
                    else if (rctO.Top < iPosY + Height - 2) return true;
                    else if (rctO.Left > iPosX + Width - 5) return true;
                    else if (rctO.Right < iPosX + 5) return true;
                }

                NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
                titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                IntPtr hwnd2 = NativeMethods.GetWindow(hwndWindow, 3);
                while (hwnd2 != (IntPtr)0)
                {
                    StringBuilder sTitle = new StringBuilder(128);
                    NativeMethods.GetWindowText(hwnd2, sTitle, 128);

                    if (NativeMethods.GetTitleBarInfo(hwnd2, ref titleBarInfo))
                    {
                        if (sTitle.Length > 0 && NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out rct) && (titleBarInfo.rcTitleBar.Bottom > 0 || sTitle.ToString() == "sheep"))
                        {
                            if (rct.Top < rctO.Top && rct.Bottom > rctO.Top)
                            {
                                if (rct.Left < iPosX && rct.Right > iPosX + 40 && iAnimationStep > 4) return true;
                            }
                        }
                    }
                    hwnd2 = NativeMethods.GetWindow(hwnd2, 3);
                }
            }
            return false;
        }
         
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            hwndWindow = (IntPtr)0;
            TopMost = false;
            TopMost = true;
            bDragging = true;
            SetNewAnimation(Animations.AnimationDrag);
        }

        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            SetNewAnimation(Animations.AnimationFall);
            bDragging = false;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

        }

        private void Form2_DragEnter(object sender, DragEventArgs e)
        {
            StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "dragging file...");
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;

        }

        private void Form2_DragDrop(object sender, DragEventArgs e)
        {
            StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "files dragged:");
            string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
            foreach (string file in files)
            {
                Program.Mainthread.LoadNewXMLFromString(File.ReadAllText(file));
                break;  // Currently only 1 file, in future maybe more animations at the same time
            }

        }
    }

    internal static class NativeMethods
    //public partial class Form2 : Form
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetTitleBarInfo(IntPtr hWnd, ref TITLEBARINFO pti);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int nCmdShow);

        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFO
        {
            public int cbSize;
            public RECT rcTitleBar;
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }

        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
        //delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
    }
}
