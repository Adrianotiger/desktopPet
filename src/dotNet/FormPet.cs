using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;

namespace DesktopPet
{
        /// <summary>
        /// Form2 is the main class (form) of the pet. <br />
        /// An ImageList will contains all images and a Timer will move the pet after a defined interval.<br />
        /// The animations of this form is loaded from an XML.<br />
        /// </summary>
    public partial class FormPet : Form
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
        int AnimationStep;
            /// <summary>
            /// Structure with all informations about the current animation.
            /// </summary>
        TAnimation CurrentAnimation;
            /// <summary>
            /// Handle to the current window. If this value is 0, the sheep is NOT walking on a window.
            /// </summary>
        IntPtr hwndWindow = (IntPtr)0;
            /// <summary>
            /// Handle to the full screen window. If this value is 0, there is no full screen window.
            /// </summary>
        IntPtr hwndFullscreenWindow = (IntPtr)0;
        IntPtr hookTaskbarId = IntPtr.Zero; // not used
        NativeMethods.RECT currentWindowSize;

            /// <summary>
            /// If sheep is walking to left  (default).
            /// </summary>
            /// <remarks>
            /// The original eSheep was a Japanese application. So it was normal to see something from right to left.<br />
            /// To leave the same characteristic, moveLeft is set to true. But it doesn't matter, because the sprite and movements gives the direction...<br />
            /// </remarks>
        bool IsMovingLeft = true;
            /// <summary>
            /// Animations class. The entire animation and its values are described here.
            /// </summary>
        readonly Animations Animations;
            /// <summary>
            /// Xml class. Xml parser and functionality are stored here.
            /// </summary>
        readonly Xml Xml;
            /// <summary>
            /// If the pet is in dragging mode (user is holding the pet with the mouse)
            /// </summary>
        bool IsDragging = false;
            /// <summary>
            /// If the pet is leaving the screen
            /// </summary>
        bool IsLeaving = false;
            /// <summary>
            /// Offset Y - Sprite size is taken and not the single image. So, over the taskbar or over the windows, the pet could be 1-2 pixels over the border if you didn't drawn it on the bottom of the sprite frame.<br />
            /// With this offset, you can re-place the pet or you can give them an offset so that it is positioned over the window (for example if you want to show a girl sitting over the taskbar, you need this function)
            /// </summary>
        double OffsetY = 0.0;
            /// <summary>
            /// Current X position of the form. Because an offset can be used, this is the origin of the sprite (not like Form2.Left) before an offset was interpolated with the form position.
            /// </summary>
        double PositionX = 0.0;
            /// <summary>
            /// Current Y position of the form. Because an offset can be used, this is the origin of the sprite (not like Form2.Top) before an offset was interpolated with the form position.
            /// </summary>
        double PositionY = 0.0;

            /// <summary>
            /// If multi screens are available, the pet can be set on a defined screen
            /// </summary>
        int DisplayIndex = 0;

        private readonly List<FormPet> childs = new List<FormPet>(4);

        /// <summary>
        /// Form constructor. This is never called. <br />
        /// Form2(Animations animations, Xml xml) -> Called when a new sheep is generated<br />
        /// Form2(Animations animations, Xml xml, Point parentPos, bool parentFlipped) -> Called when a Child is generated<br />
        /// </summary>
        public FormPet()
        {
            InitializeComponent();
        }

            /// <summary>
            /// Form constructor.  Called when a new sheep is generated. 
            /// </summary>
            /// <param name="animations">Animation class, with all values.</param>
            /// <param name="xml">Xml class, with xml functions</param>
        public FormPet(Animations animations, Xml xml)
        {
            Animations = animations;
            Xml = xml;
            InitializeComponent();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
            for (var s = 0; s < Screen.AllScreens.Length; s++)
            {
                if (Screen.AllScreens[s].Primary)
                {
                    DisplayIndex = s;
                    break;
                }
            }
        }

            /// <summary>
            /// Form constructor. Called when a Child is generated. 
            /// </summary>
            /// <param name="animations">Animation class, with all values.</param>
            /// <param name="xml">Xml class, with xml functions</param>
            /// <param name="parentPos">Position of the parent - used to detect where the child should be positioned</param>
            /// <param name="parentFlipped">If parent is flipped. If true, the child image will also be flipped</param>
            /// <param name="parentDisplay">Display Index of the parent. Put the child on same screen</param>
        public FormPet(Animations animations, Xml xml, Point parentPos, bool parentFlipped, int parentDisplay)
        {
            Animations = animations;
            Xml = xml;
            Xml.parentX = parentPos.X;
            Xml.parentY = parentPos.Y;
            Xml.parentFlipped = parentFlipped;
            DisplayIndex = parentDisplay;
			IsMovingLeft = !parentFlipped;
            InitializeComponent();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
        }

            /// <summary>
            /// With this overridden function, it is possible to remove the application from the ALT-TAB list.
            /// This, because it is not nice to see 10 times the same sheep when you press ALT-TAB (with 10 sheeps walking on your screen).
            /// If this form is a child, remove the possibility to interact with this form.
            /// See: https://msdn.microsoft.com/en-us/library/windows/desktop/ff700543(v=vs.85).aspx
            /// </summary>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;

                cp.ExStyle |= 0x00000080; // WS_EX_TOOLWINDOW             <- remove from ALT-TAB list
                cp.ExStyle |= 0x00000008; // WS_EX_TOPMOST                <- set on TopMost
                cp.ExStyle |= 0x00080000; // WS_EX_LAYERED                <- increase paint performance
                //cp.ExStyle |= 0x00000020; // WS_EX_TRANSPARENT            <- Do not draw window -> unclickable
                //cp.Style |= 0x80000000; // WS_POPUP

                if (Name.IndexOf("child") == 0)
                {
                    cp.ExStyle |= 0x08000000;   //WS_EX_NOACTIVATE  <- prevent focus when created
                }
                return cp;
            }
        }

        /// <summary>
        /// With this overridden function, it is possible to prevent the form to get the focus once created.
        /// </summary>
        protected override bool ShowWithoutActivation
        {
            get { return true; }
        }

        /// <summary>
        /// Once the form was created, this is the next function to call.
        /// It will set the size of the pet. Form will still be invisible because it has an opacity of 0.0.
        /// </summary>
        /// <param name="w">Single frame width</param>
        /// <param name="h">Single frame height</param>
        public void Show(int w, int h)
        {
            // 5 = WH_CBT
            if (hookTaskbarId == IntPtr.Zero)
            {
                StringBuilder s1 = new StringBuilder("TaskListThumbnailWnd");
                StringBuilder s2 = new StringBuilder("");
                hookTaskbarId = NativeMethods.FindWindowEx(IntPtr.Zero, IntPtr.Zero, s1, s2);
            }
            //NativeMethods.SetWindowLong(Handle, (int)-16 /*GWL_STYLE*/, (uint)0x80000000);<< does not work to force overlapping taskbar

            Width = w;
            Height = h;

            pictureBox1.Width = w;
            pictureBox1.Height = h;
            pictureBox1.Top = 0;
            pictureBox1.Left = 0;
            pictureBox1.Tag = 0;

			AnimationStep = 0;

            Show();
        }

            /// <summary>
            /// Once the form was created and resized, this is the next function to call.<br />
            /// This function is called for every single image in the sprite sheet and will be saved in the Image List component.
            /// </summary>
            /// <param name="im">Single frame image, beginning from top left to bottom right</param>
        public void AddImage(Image im)
        {
            if(imageList1.Images.Count == 0)
            {
                imageList1.ImageSize = new Size(im.Width, im.Height);
            }
            imageList1.Images.Add(im);
        }

        private Rectangle ScreenBounds { get { return Screen.AllScreens[DisplayIndex].Bounds; } }
        private Rectangle ScreenArea { get { return Screen.AllScreens[DisplayIndex].WorkingArea; } }

        /// <summary>
        /// Once the form was created, resized and all images was set, this is the next function to call.<br />
        /// It will initialize all variables and start the first animation (SPAWN).
        /// </summary>
        /// <param name="first">If it is playing a spawn for the first time. Does not have any functionality for the moment.</param>
        public void Play(bool first)
        {
            timer1.Enabled = false;                     // Stop the timer

			AnimationStep = 0;                         // First step
            hwndWindow = (IntPtr)0;                     // It is not over a window

            // Multiscreen
            if(Program.MyData.GetMultiscreen())
            {
                Random rand = new Random();
                int oldDisplayIndex = DisplayIndex;
                DisplayIndex = rand.Next(0, Screen.AllScreens.Length);
                if(oldDisplayIndex != DisplayIndex) // display changed, all computed values could be wrong
                {

                }
            }

            TSpawn spawn = Animations.GetRandomSpawn(); // Get a random SPAWN, to setting the form properties
            Top = ScreenBounds.Y + spawn.Start.Y.GetValue(DisplayIndex);
            Left = ScreenBounds.X + spawn.Start.X.GetValue(DisplayIndex);
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;
            Width = pictureBox1.Width;
            Height = pictureBox1.Height;
			PositionX = Left;
			PositionY = Top;
			OffsetY = 0.0;
            IsLeaving = false;
            SetNewAnimation(spawn.Next);                // Set next animation
            Visible = true;                             // Now we can show the form
            Opacity = 0.0;                              // do not show first frame (as it is undefined)
            timer1.Enabled = true;                      // Enable the timer (interval is well known now)
        }

			/// <summary>
			/// If this form is a child, this function is called instead of Play().
			/// It will initialize all variables and start the first animation using CHILD, not SPAWN.
			/// </summary>
			/// <param name="aniID">Animation playing by the parent (child will synchronize to this animation).</param>
			/// <param name="child">The child to play (more than 1 childs can be played at the same time).</param>
		public void PlayChild(int aniID, TChild child)
        {
            timer1.Enabled = false;                     // Stop the timer

			AnimationStep = 0;                          // First step
            hwndWindow = (IntPtr)0;                     // It is not over a window
            
            Top = ScreenBounds.Y + child.Position.Y.GetValue(DisplayIndex);          // Set position. If parent is flipped, mirror the position
            if (IsMovingLeft)
            {
                Left = ScreenBounds.X + child.Position.X.GetValue(DisplayIndex);
            }
            else
            {
                Left = ScreenBounds.X + child.Position.X.GetValue(DisplayIndex);
            }
			PositionX = Left;
			PositionY = Top;
			OffsetY = 0.0;
            Visible = true;                             // Now we can show this child
            Opacity = 1.0;
            IsLeaving = false;
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.MouseDown += (s, e) => { };     // Replace the "drag and drop" functionality

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
            foreach(var c in childs)
            {
                if(c != null && !c.IsDisposed)
                {
                    c.Close();
                    c.Dispose();
                }
            }
            if (Animations.AnimationKill > 1)
            {
                SetNewAnimation(Animations.AnimationKill);
            }
            else
            {
                Close();
                Dispose();
            }
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
        private void Timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (AnimationStep < 0) AnimationStep = 0;
            try
            {
                NextStep();
                if (IsDisposed)
                {
                    timer1.Enabled = false;
                }
                else
                {
					AnimationStep++;
                    timer1.Enabled = true;
                }
            }
            catch(Exception ex) // if form is closed timer could continue to tick (why?)
            {
                if(MessageBox.Show("Fatal Error: " + ex.Message + "\n----------\nPress Cancel for more info", "App error", MessageBoxButtons.OKCancel, MessageBoxIcon.Error) == DialogResult.Cancel)
                {
                    string seqIndex = "";
                    foreach (var item in CurrentAnimation.Sequence.Frames) seqIndex += item + ",";

                        MessageBox.Show(
                        "Current Animation ID: " + CurrentAnimation.ID + "\n" +
                        "Current Animation Name: " + CurrentAnimation.Name + "\n" +
                        "Current Animation Sequence: " + seqIndex + "\n"
                        );

                }
            }
        }

            /// <summary>
            /// After an animation is over and after a new animation was selected, this function will play the selected animation.
            /// </summary>
            /// <param name="id">Animation ID to play.</param>
        private void SetNewAnimation(int id)
        {
            if (CurrentAnimation.ID == Animations.AnimationKill) return;
            if (id < 0)  // no animation found, spawn!
            {
                Play(false);
            }
            else
            {
                TopMost = true; // bring to top again on each new animation
				AnimationStep = -1;
                CurrentAnimation = Animations.GetAnimation(id);
                CurrentAnimation.UpdateValues(DisplayIndex);
                // Check if animation ID has a child. If so, the child will be created.
                if (Animations.HasAnimationChild(id))
                {
                        // child creating childs... Maximum 5 sub-childs can be created
                    if (Name.IndexOf("child") < 0 || int.Parse(Name.Substring(5)) < 5)
                    {
						foreach (TChild childInfo in Animations.GetAnimationChild(id))
						{
							FormPet child = new FormPet(Animations, Xml, new Point(ScreenBounds.X + Left, ScreenBounds.Y + Top), !IsMovingLeft, DisplayIndex);
							for (int i = 0; i < imageList1.Images.Count; i++)
							{
								child.AddImage(imageList1.Images[i]);
							}
							// To detect if it is a child, the name of the form will be renamed.
							if (Name.IndexOf("child") < 0) // first child
							{
								child.Name = "child1";
							}
							else if (Name.IndexOf("child") == 0) // second, fifth child
							{
								child.Name = "child" + (int.Parse(Name.Substring(5)) + 1).ToString();
							}

							child.Show(Width, Height);
							child.PlayChild(id, childInfo);

                            childs.Add(child);
                        }
                    }
                }

                timer1.Interval = CurrentAnimation.Start.Interval.GetValue();
            }
        }

            /// <summary>
            /// The most important function. Each movement step is managed by this function:<br />
            /// Will calculate how much and where a pet should be positioned in the next step.<br />
            /// This function is called from <see cref="Timer1_Tick(object, EventArgs)"/>.
            /// </summary>
        private void NextStep()
        {
                // If there is no repeat, we don't need to calculate the frame index.
            if (AnimationStep < CurrentAnimation.Sequence.Frames.Count)
            {
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[AnimationStep]];
            }
            else
            {
                int index = ((AnimationStep - CurrentAnimation.Sequence.Frames.Count + CurrentAnimation.Sequence.RepeatFrom) % (CurrentAnimation.Sequence.Frames.Count - CurrentAnimation.Sequence.RepeatFrom)) + CurrentAnimation.Sequence.RepeatFrom;
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[index]]; 
            }

                // Get interval, opacity and offset interpolated from START and END values.
            timer1.Interval = CurrentAnimation.Start.Interval.Value + ((CurrentAnimation.End.Interval.Value - CurrentAnimation.Start.Interval.Value) * AnimationStep / CurrentAnimation.Sequence.TotalSteps);
            Opacity = CurrentAnimation.Start.Opacity + (CurrentAnimation.End.Opacity - CurrentAnimation.Start.Opacity) * AnimationStep / CurrentAnimation.Sequence.TotalSteps;
			OffsetY = CurrentAnimation.Start.OffsetY + (double)((CurrentAnimation.End.OffsetY - CurrentAnimation.Start.OffsetY) * AnimationStep / CurrentAnimation.Sequence.TotalSteps);

                // If dragging is enabled, move the pet to the mouse position.
            if (IsDragging)
            {
				PositionX = Left = Cursor.Position.X - Width / 2;
				PositionY = Top = Cursor.Position.Y - 2;
                return;
            }
            
            double x = CurrentAnimation.Start.X.Value;
            double y = CurrentAnimation.Start.Y.Value;
            // if TotalSteps is more than 1, we have to interpolate START and END values)
            if (CurrentAnimation.Sequence.TotalSteps > 1)
            {
                x += ((CurrentAnimation.End.X.Value - CurrentAnimation.Start.X.Value) * (double)AnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1.0));
                y += ((CurrentAnimation.End.Y.Value - CurrentAnimation.Start.Y.Value) * (double)AnimationStep / (CurrentAnimation.Sequence.TotalSteps - 1.0));
            }
                // If a new animation need to be started
            bool bNewAnimation = false;
                // If animation is leaving screen, cut the form so that it is not visibile on multiscreens
            bool bLeavingScreen = false;
                // If the pet is "flipped", mirror the movement
            if (!IsMovingLeft) x = -x;
            
            if(x < 0)   // moving left (detect left borders)
            {
                if (hwndWindow == (IntPtr)0)
                {
                    CheckFullScreen();  // used to check if another window is in full screen
                    if (PositionX + x < ScreenArea.X)    // left screen border!
                    {
                        int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL);
                        if (iBorderAnimation >= 0)
                        {
                            PositionX = ScreenArea.X;
                            x = 0;
                            SetNewAnimation(iBorderAnimation);
                            bNewAnimation = true;
                        }
                        else
                        {
                            bLeavingScreen = true;
                        }
                    }
                }
                else
                {
                    if (NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out NativeMethods.RECT rct))
                    {
                        if (PositionX + x < rct.Left)    // left window border!
                        {
                            int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                            if (iBorderAnimation >= 0)
                            {
                                PositionX = rct.Left;
                                x = 0;
                                SetNewAnimation(iBorderAnimation);
                                bNewAnimation = true;
                            }
                            else
                            {
                                // not anymore on the window
                                hwndWindow = (IntPtr)0;
                            }
                        }
                    }
                }
            }
            else if (x > 0)   // moving right (detect right borders)
            {
                if (hwndWindow == (IntPtr)0)
                {
                    CheckFullScreen();  // used to check if another window is in full screen
                    if (PositionX + x + Width > ScreenArea.X + ScreenArea.Width)    // right screen border!
                    {
                        
                        int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL);
                        if (iBorderAnimation >= 0)
                        {
                            PositionX = ScreenArea.X + ScreenArea.Width - Width;
                            x = 0;
                            SetNewAnimation(iBorderAnimation);
                            bNewAnimation = true;
                        }
                        else
                        {
                            bLeavingScreen = true;
                        }
                    }
                }
                else
                {
                    if (NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out NativeMethods.RECT rct))
                    {
                        if (PositionX + x + Width > rct.Right)    // right window border!
                        {
                            int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                            if (iBorderAnimation >= 0)
                            {
                                PositionX = rct.Right - Width;
                                x = 0;
                                SetNewAnimation(iBorderAnimation);
                                bNewAnimation = true;
                            }
                            else
                            {
                                // not anymore on the window
                                hwndWindow = (IntPtr)0;
                            }
                        }
                    }
                }
            }
            if(bNewAnimation || bLeavingScreen)
            {
                // don't check anymore for y movement
            }
            else if(y > 0)   // moving down (detect taskbar and windows)
            {
                int bottomY = ScreenArea.Y + ScreenArea.Height;

                if (PositionY + y > bottomY - Height) // border detected!
                {
                    int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.TASKBAR);
                    if (iBorderAnimation >= 0)
                    {
                        PositionY = bottomY - Height;
                        OffsetY = 0;
                        y = 0;
                        SetNewAnimation(iBorderAnimation);
                        bNewAnimation = true;
                    }
                }
                else
                {
                    int iWindowTop = FallDetect((int)y);
                    if (iWindowTop > 0)
                    {
                        int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                        if (iBorderAnimation >= 0)
                        {
                            PositionY = iWindowTop - Height;
                            OffsetY = 0;
                            y = 0;
                            SetNewAnimation(iBorderAnimation);
                            bNewAnimation = true;
                            if (CurrentAnimation.Start.Y.Value != 0)
                            {
                                hwndWindow = (IntPtr)0;
                            }
                        }
                    }
                }
            }
            else if(y < 0)  // moving up, detect upper screen border
            {
                if (PositionY + y < ScreenArea.Y) // border detected!
                {
                    int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.HORIZONTAL);
                    if (iBorderAnimation >= 0)
                    {
                        PositionY = ScreenArea.Y;
                        y = 0;
                        SetNewAnimation(iBorderAnimation);
                        bNewAnimation = true;
                    }
                    else
                    {
                        bLeavingScreen = true;
                    }
                }
            }

            if (AnimationStep >= CurrentAnimation.Sequence.TotalSteps) // animation over
            {
                int iNextAni = -1;
                if(CurrentAnimation.Sequence.Action == "flip")
                {
					// flip all images
					IsMovingLeft = !IsMovingLeft;
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
                        // If pet is outside the borders, spawn it again.
                    if (Left < ScreenBounds.X - Width || Left > ScreenBounds.X + ScreenBounds.Width)
                    {
                        iNextAni = -1;
                    }
                    else if (Top < ScreenBounds.Y - Height || Top > ScreenBounds.Y + ScreenBounds.Height)
                    {
                        iNextAni = -1;
                    }
                    else
                    {
                        iNextAni = Animations.SetNextSequenceAnimation(
                            CurrentAnimation.ID, 
                            PositionY + Height + y >= ScreenArea.Y + ScreenArea.Height - 2 ? TNextAnimation.TOnly.TASKBAR : TNextAnimation.TOnly.NONE
                        );
                    }
                }
                if(CurrentAnimation.ID == Animations.AnimationKill)
                {
                    if (timer1.Tag == null || !double.TryParse(timer1.Tag.ToString(), out double op)) timer1.Tag = 1.0;
                    op = double.Parse(timer1.Tag.ToString());
                    timer1.Tag = op - 0.1;
                    Opacity = op;
                    if (op <= 0.1)
                    {
                        Close();
                    }
                }
                else if (iNextAni >= 0)
                {
                    SetNewAnimation(iNextAni);
                    bNewAnimation = true;
                }
                else
                {
                        // Child doesn't have a spawn, they will be closed once the animation is over.
                    if(Name.IndexOf("child")==0)
                    {
                        StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "removing child");
                        Close();
                    }
                    else
                    {
                        Play(false);
                        return;
                    }
                }
            }
                // If there is a Gravity-Next animation, check if gravity is present.
            else if(CurrentAnimation.Gravity)
            {
                if(hwndWindow == (IntPtr)0)
                {
                    if(PositionY + y < ScreenArea.Y + ScreenArea.Height - Height)
                    {
                        if(PositionY + y + 3 >= ScreenArea.Y + ScreenArea.Height - Height) // allow 3 pixels to move without fall
                        {
                            y = ScreenArea.Y + ScreenArea.Height - (int)PositionY - Height;
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
                    if (AnimationStep > 0 && CheckTopWindow(true))
                    {
                        if (FollowWindow())
                        {
                            int iTimeout = 50;
                            do
                            {
                                if (!FollowWindow()) iTimeout--;
                                else iTimeout = 50;
                                Thread.Sleep(16);
                                Application.DoEvents();
                            }
                            while (iTimeout > 0);
                            PositionX = Left;
                            PositionY = Top - OffsetY;
                            return;
                        }
                        else
                        {
                            hwndWindow = (IntPtr)0;
                            SetNewAnimation(Animations.SetNextGravityAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                        }
                    }
                }
            }

                // If a new animation was started, set the interval and the first animation frame image.
            if(bNewAnimation)
            {
                timer1.Interval = 1;    // execute immediately the first step of the next animation.
                //x = 0;                  // don't move the pet, if a new animation must be started
                //y = 0;                  //  if falling, set the pet to the new position
                pictureBox1.Image = imageList1.Images[CurrentAnimation.Sequence.Frames[0]];
            }

			// Set the new pet position (and offset) in the screen.
			PositionX += x;
			PositionY += y;

            if (bLeavingScreen)
            {
                IsLeaving = true;
                if ((int)PositionX < ScreenArea.X) // leaving left
                {
                    int cut = ScreenArea.X - (int)PositionX;
                    if (Width > 2)
                    {
                        Width = pictureBox1.Width - cut;
                        Left = ScreenArea.X;
                        pictureBox1.Left -= cut;
                    }
                    else
                    {
                        Left -= Width;
                        pictureBox1.Left = Width + 1;
                        AnimationStep += CurrentAnimation.Sequence.Frames.Count / 3;
                    }   
                }
                else if ((int)(PositionX + x + Width) >= ScreenArea.X + ScreenArea.Width) // leaving right
                {
                    int cut = (int)(PositionX + x + Width) - (ScreenArea.X + ScreenArea.Width);
                    if (Width > 2)
                    {
                        Width -= cut;
                        Left = ScreenArea.X + ScreenArea.Width - Width;
                    }
                    else
                    {
                        Left += Width;
                        pictureBox1.Left = Width + 1;
                        AnimationStep += CurrentAnimation.Sequence.Frames.Count / 3;
                    }
                }
                if (PositionY - OffsetY < ScreenArea.Y) // leaving up
                {
                    int cut = (int)(ScreenArea.Y - PositionY - OffsetY);
                    if (Height > 2)
                    {
                        Height -= cut;
                        Top = ScreenArea.Y;
                        pictureBox1.Top -= cut;
                    }
                    else
                    {
                        Top -= Height;
                        pictureBox1.Top = Height + 1;
                        AnimationStep += CurrentAnimation.Sequence.Frames.Count / 3;
                    }
                }

            }
            else if (IsLeaving)  // was leaving screen just a moment ago. This could be an XML error. Reset cutting.
            {                    // it could also happens if during the leaving, a new animation was set.
                IsLeaving = false;
                if ((int)PositionX < ScreenArea.X) // leaving left
                {
                    Left = -pictureBox1.Width + Width;
                }
                pictureBox1.Top = 0;
                pictureBox1.Left = 0;
                Width = pictureBox1.Width;
                Height = pictureBox1.Height;
                Top = (int)(PositionY + OffsetY);
            }
            else
            {
                Left = (int)PositionX;
                Top = (int)(PositionY + OffsetY);
            }

            
        }

            /// <summary>
            /// Detect if pet is still falling or if taskbar/window was detected.
            /// </summary>
            /// <param name="y">Y moves in pixels for the next step (function will detect if window/taskbar is inside the movement).</param>
            /// <returns>Y position of the window or taskbar. -1 if pet is still falling.</returns>
        private int FallDetect(int y)
        {
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();
            NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

            CheckFullScreen();

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
                    else if ((titleBarInfo.rgstate[0] & 0x00008000) > 0) // invisible
                        return true;
                    
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
                if (NativeMethods.GetWindowRect(new HandleRef(this, window.Key), out NativeMethods.RECT rct))
                {
                        // If vertical position is in the falling range and pet is over window and window is at least 20 pixels under the screen border
                    if (PositionY + Height < rct.Top && PositionY + Height + y >= rct.Top &&
						PositionX >= rct.Left - Width / 2 && PositionX + Width <= rct.Right + Width / 2 &&
						PositionY > 20 + ScreenArea.Y)
                    {
                            // Pet need to walk over THIS window!
                        hwndWindow = window.Key;
                        currentWindowSize = rct;
						StringBuilder sTitle = new StringBuilder(128);
						NativeMethods.GetWindowText(hwndWindow, sTitle, 128);

						// If window is not covered by other windows, set this as current window for the pet.
						if (!CheckTopWindow(false))
                        {
								// Only if the option is set (this is an invasive functionality)
							if (Program.MyData.GetWindowForeground())
							{
								NativeMethods.ShowWindow(window.Key, 5);        // show window again
								NativeMethods.SetForegroundWindow(window.Key);  // set focus to window
							}
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
        /// Check if the window under the sheep is in full screen. If so, remove the top most.
        /// </summary>
        private void CheckFullScreen()
        {
            NativeMethods.RECT rct;
            IntPtr hwnd2 = NativeMethods.GetForegroundWindow();
            if (hwndFullscreenWindow == (IntPtr)0 && hwnd2 == Handle) return;

            if (NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out rct))
            {
                Point pWindowCenter = new Point(rct.Top + (rct.Bottom - rct.Top) / 2, rct.Left + (rct.Right - rct.Left) / 2);

                if (pWindowCenter.X > ScreenBounds.Left && pWindowCenter.X < ScreenBounds.Right &&
                    pWindowCenter.Y > ScreenBounds.Top && pWindowCenter.Y < ScreenBounds.Bottom &&
                    rct.Bottom - rct.Top >= ScreenBounds.Height && 
                    rct.Right - rct.Left >= ScreenBounds.Width) 
                {
                    if (TopMost)
                    {
                        hwndFullscreenWindow = hwnd2;
                        TopMost = false;
                        NativeMethods.SetForegroundWindow(hwnd2); // set the movie as foreground window and replace the sheep
                    }
                }
                else
                {
                    if (!TopMost)
                    {
                        hwndFullscreenWindow = (IntPtr)0;
                        TopMost = true;
                    }
                }
            }
        }

        private bool FollowWindow()
        {
            if ((int)hwndWindow != 0)
            {
                NativeMethods.RECT rctO;
                // Get window size and position of the current pet
                NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out rctO);

                if (currentWindowSize.Top != rctO.Top || currentWindowSize.Left != rctO.Left || currentWindowSize.Right != rctO.Right)
                {
                    // same width as before
                    if (rctO.Right - rctO.Left == currentWindowSize.Right - currentWindowSize.Left)
                    {
                        Top -= (currentWindowSize.Top - rctO.Top);
                        Left -= (currentWindowSize.Left - rctO.Left);
                    }
                    else // new width
                    {
                        Top -= (currentWindowSize.Top - rctO.Top);
                        Left = rctO.Left + (Left - currentWindowSize.Left) * (rctO.Right - rctO.Left) / (currentWindowSize.Right - currentWindowSize.Left);
                    }
                    currentWindowSize = rctO;
                    return true;
                }
            }
            return false;
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
                    if(currentWindowSize.Top != rctO.Top || currentWindowSize.Left != rctO.Left || currentWindowSize.Right != rctO.Right)
                    {
                        return true;
                    }
                }

                    // Get more informations about the current window title bar
                NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
                titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                //Debug.WriteLine("Window TREE");
                
                // Get the handle to the first window (from user visual, in Z-order)
                IntPtr hwnd2 = NativeMethods.GetTopWindow((IntPtr)0);
                    // Loop until there are windows over the current window (in Z-Order)
                while (hwnd2 != (IntPtr)0)
                {
						// All windows up to the current window was parsed, now window is overlapping the current window
					if (hwnd2 == hwndWindow)
					{
                        //Debug.WriteLine("--XX Parsed all windows");
						return false;
					}

                    if (NativeMethods.IsWindowVisible(hwnd2))
                    {
                        StringBuilder sTitle = new StringBuilder(128);
                        NativeMethods.GetWindowText(hwnd2, sTitle, 128);

                        //Debug.WriteLine("--> " + sTitle);

                        // If window has a title bar
                        if (sTitle.Length > 0 && NativeMethods.GetTitleBarInfo(hwnd2, ref titleBarInfo))
                        {
                            // If window has a title name and a valid size and is not fullscreen
                            if (NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out rct) &&
                                (titleBarInfo.rcTitleBar.Bottom >= 0 || sTitle.ToString() == "sheep"))
                            {
                                //Debug.WriteLine("   -->  Pos:" + rct.Top + "," + rct.Left + " - Size:" + (rct.Right - rct.Left).ToString() + "," + (rct.Bottom - rct.Top).ToString());
                                if (rct.Top < rctO.Top && rct.Bottom > rctO.Top)
                                {
                                    if (rct.Left < PositionX && rct.Right > PositionX + 40/* && iAnimationStep > 4*/)
                                    {
                                        //Debug.WriteLine("   --> Window found!");
                                        return true;
                                    }
                                }
                            }
                        }
                    }
                        // Get the handle to the next window (to user visual, in Z-order)
                    hwnd2 = NativeMethods.GetWindow(hwnd2, 2);
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
            if (e.Button == MouseButtons.Left && Name.IndexOf("child") < 0)
            {
                hwndWindow = (IntPtr)0;             // Remove window handles
                TopMost = false;
                TopMost = true;                     // Set again the topmost
				IsDragging = true;                   // Flag it as dragging pet
                SetNewAnimation(Animations.AnimationDrag);  // Set the dragging animation (if present)
            }
            else if(e.Button == MouseButtons.Right && StartUp.IsDebugActive())
            {
                ContextMenu cm = new ContextMenu();
                cm.MenuItems.Add("ID." + CurrentAnimation.ID + " - " + CurrentAnimation.Name).Enabled = false;
                cm.MenuItems.Add("-");
                MenuItem menuNext = cm.MenuItems.Add("Next");
                MenuItem menuBorder = cm.MenuItems.Add("Border");
                MenuItem menuGravity = cm.MenuItems.Add("Gravity");
                cm.MenuItems.Add("-");
                MenuItem menuSpawn = cm.MenuItems.Add("Spawns");

                List<TNextAnimation> list = Animations.GetNextAnimations(CurrentAnimation.ID, true, false, false);
                foreach (TNextAnimation ani in list)
                {
                    MenuItem menu = menuNext.MenuItems.Add("ID." + ani.ID + " - " + Animations.SheepAnimations[ani.ID].Name + "\t (Prob: " + ani.Probability + ") only:" + ani.only.ToString());
                    menu.Click += (ms, me) => { SetNewAnimation(ani.ID); };
                }
                if (list.Count == 0) menuNext.Enabled = false;

                list = Animations.GetNextAnimations(CurrentAnimation.ID, false, true, false);
                foreach (TNextAnimation ani in list)
                {
                    MenuItem menu = menuBorder.MenuItems.Add("ID." + ani.ID + " - " + Animations.SheepAnimations[ani.ID].Name + "\t (Prob: " + ani.Probability + ") only: " + ani.only.ToString());
                    menu.Click += (ms, me) => { SetNewAnimation(ani.ID); };
                }
                if (list.Count == 0) menuBorder.Enabled = false;

                list = Animations.GetNextAnimations(CurrentAnimation.ID, false, false, true);
                foreach (TNextAnimation ani in list)
                {
                    MenuItem menu = menuGravity.MenuItems.Add("ID." + ani.ID + " - " + Animations.SheepAnimations[ani.ID].Name + "\t (Prob: " + ani.Probability + ") only:" + ani.only.ToString());
                    menu.Click += (ms, me) =>{ SetNewAnimation(ani.ID); };
                }
                if (list.Count == 0) menuGravity.Enabled = false;

                List<TSpawn> listS = Animations.GetNextSpawns();
                foreach (TSpawn spa in listS)
                {
                    MenuItem menu = menuSpawn.MenuItems.Add("ID." + spa.Next + " - " + Animations.SheepAnimations[spa.Next].Name + "\t (Prob: " + spa.Probability + ")");
                    menu.Click += (ms, me) => 
                    {
                        Top = ScreenBounds.Y + spa.Start.Y.GetValue(DisplayIndex);
                        Left = ScreenBounds.X + spa.Start.X.GetValue(DisplayIndex);
						PositionX = Left;
						PositionY = Top;
						OffsetY = 0.0;
                        SetNewAnimation(spa.Next);
                    };
                }

                timer1.Enabled = false;

                cm.Collapse += (ms, me) =>
                {
                    timer1.Interval = 1;
                    timer1.Enabled = true;
                };

                pictureBox1.ContextMenu = cm;
                pictureBox1.ContextMenu.Show(pictureBox1, new Point(0,this.Top > 500 ? 0 : this.Height));
            }
        }

            /// <summary>
            /// Mouse released the pet.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Mouse event values.</param>
        private void pictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Name.IndexOf("child") < 0)
            {
                SetNewAnimation(Animations.AnimationFall);
            }
            if(IsDragging)
            {
                // if it was dragged, check if the screen is different
                // if(Program.MyData.GetMultiscreen()) <-- If manually moved to another screen, set the new screen as default screen.
                {
                    for(var k=0;k<Screen.AllScreens.Length;k++)
                    {
                        Rectangle bounds = Screen.AllScreens[k].Bounds;

                        if (Left + Width / 2 >= bounds.X && 
                            Left + Width / 2 <= bounds.X + bounds.Width)
                        {
                            if (Top + Height / 2 >= bounds.Y && 
                                Top + Height <= bounds.Y + bounds.Height)
                            {
                                DisplayIndex = k;
                                break;
                            }
                        }
                    }
                }
            }
			IsDragging = false;
        }
        
            /// <summary>
            /// Mouse double click on pet. From old eSheep, a double click with the right mouse will kill the sheep.
            /// </summary>
            /// <param name="sender">Caller object.</param>
            /// <param name="e">Mouse event values.</param>
        private void pictureBox1_DoubleClick(object sender, EventArgs e)
        {
            MouseEventArgs me = (MouseEventArgs)e;
            if (me.Button == MouseButtons.Right)
            {
                if(!Program.Mainthread.KillSheep(this))
                {
                    Close();
                }
            }
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
                if (file.Contains(".xml"))
                {
                    Program.Mainthread.KillSheep(this);
                    Program.MyData.SetXml(File.ReadAllText(file), "..");
                    Program.Mainthread.LoadNewXMLFromString(File.ReadAllText(file));
                    break;  // Currently only 1 file, in future maybe more animations at the same time
                }
            }

        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(!IsDisposed) Dispose();
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
        /// Get the window on the top, if hWnd is NULL, the top in Z-order will be returned
        /// </summary>
        /// <param name="hWnd">Handle to the current window.</param>
        /// <returns>Pointer to the next window.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetTopWindow(IntPtr hWnd);

        /// <summary>
        /// Get the desktop window.
        /// </summary>
        /// <returns>Pointer to the first window.</returns>
        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        internal static extern IntPtr FindWindowEx(IntPtr hWndParent, IntPtr hWndChildAfter, StringBuilder slpClass, StringBuilder slpWindow);

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
