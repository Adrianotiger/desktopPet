using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;

namespace DesktopPet
{
        /// <summary>
        /// Form2 is the main class (form) of the pet. <br />
        /// An ImageList will contains all images and a Timer will move the pet after a defined interval.<br />
        /// The animations of this form is loaded from an XML.<br />
        /// </summary>
    public partial class Form2 : Form
    {
            /// <summary>
            /// Current step in the animation-frames list.
            /// </summary>
            /// <remarks>
            /// Every animation has a defined quantity of steps. They are calculated from:<br />
            /// - Quantity of frames<br />
            /// - Repeat count and repeat from<br />
            /// If an animation has 10 different frames and frame 5 to 9 are repeated 8 times, the total of steps is 10 + 5 * 8 = 50 steps.<br />
            /// Once the last step was reached, the next animation will be started. If now animation is set, the SPAWN will be executed.<br />
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
            /// The original eSheep was a Japanese application. So it was normal to see something from right to left.<br />
            /// To leave the same characteristic, moveLeft is set to true. But it doesn't matter, because the sprite and movements gives the direction...<br />
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
            /// Offset Y - Sprite size is taken and not the single image. So, over the taskbar or over the windows, the pet could be 1-2 pixels over the border if you didn't drawn it on the bottom of the sprite frame.<br />
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
            /// Form constructor. This is never called. <br />
            /// Form2(Animations animations, Xml xml) -> Called when a new sheep is generated<br />
            /// Form2(Animations animations, Xml xml, Point parentPos, bool parentFlipped) -> Called when a Child is generated<br />
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
            /// With this overridden function, it is possible to remove the application from the ALT-TAB list.
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
            /// Once the form was created and resized, this is the next function to call.<br />
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
            /// Once the form was created, resized and all images was set, this is the next function to call.<br />
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
            /// Timer tick. The entire animation is droved through this timer. The interval is set in the XML animation file.
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

            /// <summary>
            /// After an animation is over and after a new animation was selected, this function will play the selected animation.
            /// </summary>
            /// <param name="id">Animation ID to play.</param>
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
                    // Check if animation ID has a child. If so, the child will be created.
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
                            // To detect if it is a child, the name of the form will be renamed.
                        child.Name = "child";
                        child.Show(Width, Height);
                        child.PlayChild(id);
                    }
                }
                timer1.Interval = CurrentAnimation.Start.Interval.GetValue();
            }
        }

            /// <summary>
            /// The most important function. Each movement step is managed by this function:<br />
            /// Will calculate how much and where a pet should be positioned in the next step.<br />
            /// This function is called from <see cref="timer1_Tick(object, EventArgs)"/>.
            /// </summary>
        private void NextStep()
        {
                // If there is no repeat, we don't need to calculate the frame index.
            if (iAnimationStep < CurrentAnimation.Sequence.Frames.Count)
            {
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[iAnimationStep]];
            }
            else
            {
                int index = ((iAnimationStep - CurrentAnimation.Sequence.Frames.Count + CurrentAnimation.Sequence.RepeatFrom) % (CurrentAnimation.Sequence.Frames.Count - CurrentAnimation.Sequence.RepeatFrom)) + CurrentAnimation.Sequence.RepeatFrom;
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[index]]; 
            }

                // Get interval, opacity and offset interpolated from START and END values.
            timer1.Interval = CurrentAnimation.Start.Interval.Value + ((CurrentAnimation.End.Interval.Value - CurrentAnimation.Start.Interval.Value) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);
            Opacity = CurrentAnimation.Start.Opacity + ((CurrentAnimation.End.Opacity - CurrentAnimation.Start.Opacity) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);
            iOffsetY = CurrentAnimation.Start.OffsetY + ((CurrentAnimation.End.OffsetY - CurrentAnimation.Start.OffsetY) * iAnimationStep / CurrentAnimation.Sequence.TotalSteps);

                // If dragging is enabled, move the pet to the mouse position.
            if (bDragging)
            {
                iPosX = Left = Cursor.Position.X - Width / 2;
                iPosY = Top = Cursor.Position.Y + 2;
                return;
            }

            int x = CurrentAnimation.Start.X.Value;
            int y = CurrentAnimation.Start.Y.Value;
            // if TotalSteps is more than 1, we have to interpolate START and END values)
            if (CurrentAnimation.Sequence.TotalSteps > 1)
            {
                x += ((CurrentAnimation.End.X.Value - CurrentAnimation.Start.X.Value) * iAnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1));
                y += ((CurrentAnimation.End.Y.Value - CurrentAnimation.Start.Y.Value) * iAnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1));
            }
                // If a new animation need to be started
            bool bNewAnimation = false;
                // If the pet is "flipped", mirror the movement
            if (!bMoveLeft) x = -x;
            
            if(x < 0)   // moving left (detect left borders)
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
            else if (x > 0)   // moving right (detect right borders)
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
            if(y > 0)   // moving down (detect taskbar and windows)
            {
                if (CurrentAnimation.EndBorder.Count > 0)
                {
                    if (iPosY + y > Screen.PrimaryScreen.WorkingArea.Height - Height) // border detected!
                    {
                        y = Screen.PrimaryScreen.WorkingArea.Height - iPosY - Height + iOffsetY;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.TASKBAR));
                        bNewAnimation = true;
                    }
                    else
                    {
                        int iWindowTop = FallDetect(y);
                        if (iWindowTop > 0)
                        {
                            y = iWindowTop - iPosY - Height + iOffsetY;
                            SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                            if(CurrentAnimation.Start.Y.Value != 0)
                            {
                                hwndWindow = (IntPtr)0;
                            }
                        }
                    }
                }
            }
            else if(y < 0)  // moving up, detect upper screen border
            {
                if (CurrentAnimation.EndBorder.Count > 0)
                {
                    if (iPosY < 0) // border detected!
                    {
                        y = 0;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.HORIZONTAL));
                        bNewAnimation = true;
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
                        // Child doesn't have a spawn, they will be closed once the animation is over.
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
                // If there is a Gravity-Next animation, check if gravity is present.
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

                // If a new animation was started, set the interval and the first animation frame image.
            if(bNewAnimation)
            {
                timer1.Interval = 1;    // execute immediately the first step of the next animation.
                x = 0;                  // don't move the pet, if a new animation must be started
                //y = 0;                  //  if falling, set the pet to the new position
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[0]];
            }

                // Set the new pet position (and offset) in the screen.
            iPosX += x;
            iPosY += y;

            Left = iPosX;
            Top = iPosY + iOffsetY;
        }

            /// <summary>
            /// Detect if pet is still falling or if taskbar/window was detected.
            /// </summary>
            /// <param name="y">Y moves in pixels for the next step (function will detect if window/taskbar is inside the movement).</param>
            /// <returns>Y position of the window or taskbar. -1 if pet is still falling.</returns>
        private int FallDetect(int y)
        {
            NativeMethods.RECT rct;
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();
            NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                // Enumerate all windows on the desktop.
            NativeMethods.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == Handle) return true;    // form itself, don't parse

                    // Enumerate only visible windows
                if (NativeMethods.IsWindowVisible(hWnd))
                {
                    StringBuilder sTitle = new StringBuilder(128);
                    NativeMethods.GetWindowText(hWnd, sTitle, 128);

                        // Sheep windows doesn't have a title bar, but we want detect if another pet is present
                    if (sTitle.ToString() == "Sheep") { }
                        // If there is no title bar, continue enumerating other windows
                    else if (!NativeMethods.GetTitleBarInfo(hWnd, ref titleBarInfo)) return true;
                        // If title bar is not visible, continue enumerating other windows
                    else if ((titleBarInfo.rgstate[0] & 0x00008000) > 0) return true;    // invisible
                    
                        // If window has a title, add this window to list
                    if (sTitle.Length > 0)
                    {
                        windows[hWnd] = sTitle.ToString();
                    }
                }
                return true;
            }, (IntPtr)0);

                // For each valid window found:
            foreach (KeyValuePair<IntPtr, string> window in windows)
            {
                    // Get size and position of window
                if (NativeMethods.GetWindowRect(new HandleRef(this, window.Key), out rct))
                {
                        // If vertical position is in the falling range and pet is over window and window is at least 20 pixels under the screen border
                    if (iPosY + Height < rct.Top && iPosY + Height + y >= rct.Top &&
                        iPosX >= rct.Left - Width / 2 && iPosX + Width <= rct.Right + Width / 2 &&
                        iPosY > 20)
                    {
                            // Pet need to walk over THIS window!
                        hwndWindow = window.Key;
                            // If window is not covered by other windows, set this as current window for the pet.
                        if (!CheckTopWindow(false))
                        {
                            //NativeMethods.ShowWindow(window.Key, 0);      // hide window
                            NativeMethods.ShowWindow(window.Key, 5);        // show window again
                            NativeMethods.SetForegroundWindow(window.Key);  // set focus to window
                            return rct.Top;                                 // return the position for the pet
                        }
                        else
                        {
                            hwndWindow = (IntPtr)0;                         // window is covered by other windows, reset handle
                        }
                    }
                }
            }
            return -1;      // no windows detected.
        }
        
            /// <summary>
            /// Check if current window handler is still valid (if another window cover the visual of this window, it must not be used as window)
            /// </summary>
            /// <param name="bCheck">Check if it is still valid. Set false if window is not proofed, true if pet is already walking on a window => check if window is still valid.</param>
            /// <returns>True if window is still valid and present. False if window is not anymore there.</returns>
            /// <seealso cref="NativeMethods.GetWindow(IntPtr, int)"/>
            /// <seealso cref="NativeMethods.GetTitleBarInfo(IntPtr, ref NativeMethods.TITLEBARINFO)"/>
        private bool CheckTopWindow(bool bCheck)
        {
                // Check only if we have a valid window handler
            if ((int)hwndWindow != 0)
            {
                NativeMethods.RECT rctO;
                NativeMethods.RECT rct;
                    // Get window size and position of the current pet
                NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out rctO);

                    // If pet was walking on a window, check if window is still in the same position
                if (bCheck)
                {
                    if (rctO.Top > iPosY + Height + 2) return true;
                    else if (rctO.Top < iPosY + Height - 2) return true;
                    else if (rctO.Left > iPosX + Width - 5) return true;
                    else if (rctO.Right < iPosX + 5) return true;
                }

                    // Get more informations about the current window title bar
                NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
                titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                    // Get the handle to the next window (to user visual, in Z-order)
                IntPtr hwnd2 = NativeMethods.GetWindow(hwndWindow, 3);
                    // Loop until there are windows over the current window (in Z-Order)
                while (hwnd2 != (IntPtr)0)
                {
                    StringBuilder sTitle = new StringBuilder(128);
                    NativeMethods.GetWindowText(hwnd2, sTitle, 128);

                        // If window has a title bar
                    if (NativeMethods.GetTitleBarInfo(hwnd2, ref titleBarInfo))
                    {
                            // If window has a title name and a valid size and is not fullscreen
                        if (sTitle.Length > 0 && 
                            NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out rct) && 
                            (titleBarInfo.rcTitleBar.Bottom > 0 || sTitle.ToString() == "sheep"))
                        {
                            if (rct.Top < rctO.Top && rct.Bottom > rctO.Top)
                            {
                                if (rct.Left < iPosX && rct.Right > iPosX + 40 && iAnimationStep > 4) return true;
                            }
                        }
                    }
                        // Get the handle to the next window (to user visual, in Z-order)
                    hwnd2 = NativeMethods.GetWindow(hwnd2, 3);
                }
            }
            return false;
        }
         
            /// <summary>
            /// Picture box fills the form, so mouse events are managed by this object: mouse pressed = pick pet. 
            /// </summary>
            /// <param name="sender">The caller object.</param>
            /// <param name="e">Mouse event values.</param>
        private void pictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            hwndWindow = (IntPtr)0;             // Remove window handles
            TopMost = false;
            TopMost = true;                     // Set again the topmost
            bDragging = true;                   // Flag it as dragging pet
            SetNewAnimation(Animations.AnimationDrag);  // Se the dragging animation (if present)
        }

            /// <summary>
            /// Mouse released the pet.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Mouse event values.</param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            SetNewAnimation(Animations.AnimationFall);
            bDragging = false;
        }

            /// <summary>
            /// Pet allows dropping other files. If you drop a XML animation file, the mouse icon will change.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Mouse event values.</param>
        private void Form2_DragEnter(object sender, DragEventArgs e)
        {
            StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "dragging file...");
            if (e.Data.GetDataPresent(DataFormats.FileDrop)) e.Effect = DragDropEffects.Copy;
        }

            /// <summary>
            /// Pet allows dropping other files. If a XML file was dropped, this one will be loaded.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Dragging event values.</param>
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

        /// <summary>
        /// Native methods for the windows detection functionality. User32.dll is used for this.
        /// </summary>
    internal static class NativeMethods
    {
            /// <summary>
            /// Get size of a window.
            /// </summary>
            /// <param name="hWnd">Handle to window.</param>
            /// <param name="lpRect">returns the size of the window.</param>
            /// <returns>True if successfully.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);

            /// <summary>
            /// Get a list of all windows present on the desktop.
            /// </summary>
            /// <param name="enumFunc">Enumeration function.</param>
            /// <param name="lParam">User defined value.</param>
            /// <returns>True if successfully.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool EnumWindows(EnumWindowsProc enumFunc, IntPtr lParam);

            /// <summary>
            /// If window is visible (is on the desktop).
            /// </summary>
            /// <param name="hWnd">Handle to the window.</param>
            /// <returns>True if successfully.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool IsWindowVisible(IntPtr hWnd);

            /// <summary>
            /// Get the text present in the window title bar.
            /// </summary>
            /// <param name="hWnd">Handle to the window.</param>
            /// <param name="lpString">Array, where the title should be copied.</param>
            /// <param name="nMaxCount">Array size.</param>
            /// <returns>Length of the title on the title bar.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        internal static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

            /// <summary>
            /// Get the values of the title bar from the window.
            /// </summary>
            /// <param name="hWnd">Handle to the window.</param>
            /// <param name="pti">Pointer to a valid structure. Will be filled with all information.</param>
            /// <returns>True if successfully.</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool GetTitleBarInfo(IntPtr hWnd, ref TITLEBARINFO pti);

            /// <summary>
            /// Change window modality (show, normal, hidden, maximize, ...) of a window.
            /// </summary>
            /// <param name="hWnd">Handle to the window.</param>
            /// <param name="nCmdShow">Command to change modality.</param>
            /// <returns>True if successfully</returns>
            /// <seealso cref="ShowWindow(IntPtr, int)"/>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

            /// <summary>
            /// Set the focus to the window and bring it to foreground. Used once the pet is felt over it.
            /// </summary>
            /// <param name="hWnd">Handle to the window.</param>
            /// <returns>True</returns>
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

            /// <summary>
            /// Get the next window on the desktop (next to the user in Z-order, child, first window, etc.)
            /// </summary>
            /// <param name="hWnd">Handle to the current window.</param>
            /// <param name="nCmdShow">Command of the next window to get, <see cref="GetWindow(IntPtr, int)"/></param>
            /// <returns>Pointer to the next window.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetWindow(IntPtr hWnd, int nCmdShow);

            /// <summary>
            /// Structure with the information about the title bar of the window.
            /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        internal struct TITLEBARINFO
        {
                /// <summary>
                /// Size (in bytes) of the current structure.
                /// </summary>
            public int cbSize;
                /// <summary>
                /// Dimension of the title bar.
                /// </summary>
            public RECT rcTitleBar;
                /// <summary>
                /// 6 bytes containing the states of the title bar.
                /// </summary>
            [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
            public int[] rgstate;
        }

            /// <summary>
            /// Dimension structure (used for the windows size).
            /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
                /// <summary>
                /// x position of upper-left corner
                /// </summary>
            public int Left;
                /// <summary>
                /// y position of upper-left corner
                /// </summary>
            public int Top;
                /// <summary>
                /// x position of lower-right corner
                /// </summary>
            public int Right;
                /// <summary>
                /// y position of lower-right corner
                /// </summary>
            public int Bottom; 
        }

            /// <summary>
            /// Procedure used to find all windows on the desktop.
            /// </summary>
            /// <param name="hWnd">Handle of the current found window.</param>
            /// <param name="lParam">User defined parameter.</param>
            /// <returns>True if successfully found another window.</returns>
        [return: MarshalAs(UnmanagedType.Bool)]
        internal delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);
    }
}
