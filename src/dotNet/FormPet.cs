using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using System.Numerics;

using DesktopPet.Properties;
using System.Xml;

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
        private bool pendingTransition;
        private int pendingAnimation;
        private bool pendingWindowLanding;
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
            /// Random object
            /// </summary>
        Random rand = new Random();
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
            /// The force in which the user tossed the sheep with. Uses Vector2 for easy magnitude calculation.
            /// </summary>
        Vector2 TossForce = Vector2.Zero;
            /// <summary>
            /// If the pet is in tossing mode (it was tossed and is now falling).
            /// </summary>
        bool IsTossing = false;
            /// <summary>
            /// The current vertical velocity while in tossing mode.
            /// </summary>
        double tossVertVel = 0.0;

            /// <summary>
            /// Read the live option for both parent and child windows.
            /// </summary>
        private bool UseSmoothMovements { get { return Program.MyData.GetSmoothMovements(); } }

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
            InitializeMotion();
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
            InitializeMotion();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
            Screen[] screens = Screen.AllScreens;
            for (var s = 0; s < screens.Length; s++)
            {
                if (screens[s].Primary)
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
            InitializeMotion();
            Visible = false;            // Is invisible at beginning (we don't know where this sprite should be positioned)
            Opacity = 0.0;
            rand = new Random();
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

        private Screen CurrentScreen
        {
            get
            {
                // Display indices can become stale after unplugging a monitor or an RDP change.
                // Resolve against one snapshot so enumeration cannot change between check and use.
                Screen[] screens = Screen.AllScreens;
                if (DisplayIndex < 0 || DisplayIndex >= screens.Length)
                    DisplayIndex = Math.Max(0, Array.FindIndex(screens, screen => screen.Primary));
                return screens[DisplayIndex];
            }
        }
        private Rectangle ScreenBounds { get { return CurrentScreen.Bounds; } }
        private Rectangle ScreenArea { get { return CurrentScreen.WorkingArea; } }

		/// <summary>
		/// Once the form was created, resized and all images was set, this is the next function to call.<br />
		/// It will initialize all variables and start the first animation (SPAWN).
		/// </summary>
		/// <param name="first">If it is playing a spawn for the first time. Does not have any functionality for the moment.</param>
		/// <param name="forceSpawn">Set a spawn animation, or leave empty if a random spawn can be used.</param>
		public void Play(bool first, int forceSpawn = -1)
        {
            timer1.Enabled = false;                     // Stop the timer

			AnimationStep = 0;                         // First step
            hwndWindow = (IntPtr)0;                     // It is not over a window

            // Multiscreen
            if(Program.MyData.GetMultiscreen())
            {
                int oldDisplayIndex = DisplayIndex;
                DisplayIndex = rand.Next(0, Screen.AllScreens.Length);
                if(oldDisplayIndex != DisplayIndex) // display changed, all computed values could be wrong
                {

                }
            }

            TSpawn spawn;
            if (forceSpawn < 0) spawn = Animations.GetRandomSpawn(); // Get a random SPAWN, to setting the form properties
            else
            {
                var k = Animations.SheepSpawn.Keys.ToList();
                spawn = Animations.SheepSpawn[k[forceSpawn]];
            }
            Top = ScreenBounds.Y + spawn.Start.Y.GetValue(DisplayIndex);
            Left = ScreenBounds.X + spawn.Start.X.GetValue(DisplayIndex);
            if(!IsMovingLeft)
            {
                Left = ScreenBounds.X - (spawn.Start.X.GetValue(DisplayIndex) - ScreenBounds.Width) - pictureBox1.Width;
            }
            pictureBox1.Left = 0;
            pictureBox1.Top = 0;
            Width = pictureBox1.Width;
            Height = pictureBox1.Height;
			PositionX = Left;
			PositionY = Top;
			OffsetY = 0.0;
            IsLeaving = false;
            ResetMotion();
            SetNewAnimation(spawn.Next);                // Set next animation
            Visible = true;                             // Now we can show the form
            Opacity = 0.0;                              // do not show first frame (as it is undefined)
            timer1.Enabled = true;                      // Enable the timer (interval is well known now)
            TopMost = true;     // new in 1.2.6
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
            Left = ScreenBounds.X + child.Position.X.GetValue(DisplayIndex);
			PositionX = Left;
			PositionY = Top;
			OffsetY = 0.0;
            Visible = true;                             // Now we can show this child
            Opacity = 1.0;
            IsLeaving = false;
            ResetMotion();
            pictureBox1.Cursor = Cursors.Default;
            pictureBox1.MouseDown += (s, e) => { };     // Replace the "drag and drop" functionality

            SetNewAnimation(child.Next);                // Set next animation to play

            timer1.Enabled = true;                      // Enable timer (interval is known, now)
        }

            /// <summary>
            /// If application is closed, all forms have still 1 second to show something (change animation).
            /// </summary>
            /// <remarks>
            /// Kill, Sync, Drag, Fall, Toss, "Fall Soft" and "Fall Hard" are "Key-names" in the XML file. If you use one of them, this program will automatically run the animation linked to this names.
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
            /// Kill, Sync, Drag, Fall, Toss, "Fall Soft" and "Fall Hard" are "Key-names" in the XML file. If you use one of them, this program will automatically run the animation linked to this names.
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
            /// Replace the animation, settle its position and offset, then publish sound and children.
            /// Callers retain control of first-frame presentation and sprite scheduling.
            /// </summary>
            /// <param name="id">Animation ID to play.</param>
        private void SetNewAnimation(int id)
        {
			// Explicit actions (drag, sync, kill or respawn) replace a queued transition.
			pendingTransition = false;
            if (CurrentAnimation.ID == Animations.AnimationKill) return;
            // Settle the outgoing endpoint while its animation is still current.
            // This also owns endpoint commitment for a completed pending transition.
            if (!IsDragging && !IsTossing && !IsLeaving && motion.Initialized)
                CommitLogicalPosition();
            if (id < 0)  // no animation found, spawn!
            {
                Play(false);
            }
            else
            {
				AnimationStep = -1;
                CurrentAnimation = Animations.GetAnimation(id);
                CurrentAnimation.UpdateValues(DisplayIndex);

                // OffsetY belongs to the displayed sprite, not to the preceding
                // movement segment. Apply the new frame's offset before its sound
                // and children become observable; do not interpolate the old offset.
                if (!IsDragging && !IsTossing && !IsLeaving)
                {
                    OffsetY = CurrentAnimation.Start.OffsetY;
                    CommitLogicalPosition();
                }

                // Play sound if there is one
                Animations.StartSound(id);

                // v.1.2.6: this will steal taskbar focus and the tray menu will disappear. So this should not be used too often.
                if (Program.MyData.GetStealTaskbarFocus() && CurrentAnimation.Start.OffsetY != 0 && CurrentAnimation.Start.X.Value != 0)
                {
                    TopMost = true; // bring to top again on each new animation
                }

                // Check if animation ID has a child. If so, the child will be created.
                if (Animations.HasAnimationChild(id))
                {
                        // child creating childs... Maximum 5 sub-childs can be created
                    if (Name.IndexOf("child") < 0 || int.Parse(Name.Substring(5)) < 5)
                    {
						foreach (TChild childInfo in Animations.GetAnimationChild(id))
						{
							FormPet child = new FormPet(Animations, Xml, new Point(Left - ScreenBounds.X, Top - ScreenBounds.Y), !IsMovingLeft, DisplayIndex);
							for (int i = 0; i < imageList1.Images.Count; i++)
							{
                                child.AddImage(GetSprite(i));
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
            if (pendingTransition)
            {
                // The previous tick reserved a complete interval for its final
                // displacement. Publish the successor only after that interval.
                CompleteNaturalTransition(pendingAnimation, pendingWindowLanding);
                return;
            }
            // Flush the elapsed render sample before publishing the next sprite.
            // Otherwise the sprite timer can overtake the last motion timer tick.
            if (!IsDragging && !IsTossing && !IsLeaving)
            {
                if (UseSmoothMovements && motion.Initialized)
                    RenderMotion(motionClock.Elapsed.TotalMilliseconds);
                else if (!UseSmoothMovements)
                    CommitLogicalPosition();
            }
            // A completed sequence can still be moving through its terminal
            // interval. Hold its last valid frame instead of wrapping to the first.
            // Drag/toss and kill keep their existing independent frame loops.
            int spriteStep = AnimationStep;
            if (!IsDragging && !IsTossing && CurrentAnimation.ID != Animations.AnimationKill)
                spriteStep = Math.Min(spriteStep, CurrentAnimation.Sequence.TotalSteps - 1);
                // If there is no repeat, we don't need to calculate the frame index.
            if (spriteStep < CurrentAnimation.Sequence.Frames.Count)
            {
                pictureBox1.Image = GetSprite(CurrentAnimation.Sequence.Frames[spriteStep]);
            }
            else
            {
                int index = ((spriteStep - CurrentAnimation.Sequence.Frames.Count + CurrentAnimation.Sequence.RepeatFrom) % (CurrentAnimation.Sequence.Frames.Count - CurrentAnimation.Sequence.RepeatFrom)) + CurrentAnimation.Sequence.RepeatFrom;
                pictureBox1.Image = GetSprite(CurrentAnimation.Sequence.Frames[index]);
            }

                // Get interval (if not in tossing mode), opacity and offset interpolated from START and END values.
            if(!IsTossing)
                timer1.Interval = CurrentAnimation.Start.Interval.Value + ((CurrentAnimation.End.Interval.Value - CurrentAnimation.Start.Interval.Value) * AnimationStep / CurrentAnimation.Sequence.TotalSteps);
            int movementDuration = timer1.Interval;
            Opacity = CurrentAnimation.Start.Opacity + (CurrentAnimation.End.Opacity - CurrentAnimation.Start.Opacity) * AnimationStep / CurrentAnimation.Sequence.TotalSteps;
			OffsetY = CurrentAnimation.Start.OffsetY + (double)((CurrentAnimation.End.OffsetY - CurrentAnimation.Start.OffsetY) * AnimationStep / CurrentAnimation.Sequence.TotalSteps);

                // If dragging is enabled, move the pet to the mouse position.
            if (IsDragging)
            {
                // Position follows mouse messages and the independent render timer.
                return;
            }

            // Physics runs on the independent motion clock, not sprite frames.
            if (IsTossing) return;

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
            int nextAnimation = -1;
            bool landedOnWindow = false;
                // If animation is leaving screen, cut the form so that it is not visibile on multiscreens
            bool bLeavingScreen = false;
                // If the pet is "flipped", mirror the movement
            if (!IsMovingLeft) x = -x;
            
            if(x < 0)   // moving left (detect left borders)
            {
                CheckFullScreen();  // used to check if another window is in full screen
                if (PositionX + x < ScreenArea.X)    // left screen border!
                {
                    int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL);
                    if (iBorderAnimation >= 0)
                    {
                        PositionX = ScreenArea.X;
                        x = 0;
                        nextAnimation = iBorderAnimation;
                        bNewAnimation = true;
                    }
                    else
                    {
                        bLeavingScreen = true;
                    }
                }
                else    // run window border detection setup and then check right border of all valid windows!
                {
                    // Retrieve valid windows
                    Dictionary<IntPtr, string> windows = EnumerateWindows();

                    // For each valid window found:
                    foreach (KeyValuePair<IntPtr, string> window in windows)
                    {
                        // Get size and position of window
                        if (NativeMethods.GetWindowRect(new HandleRef(this, window.Key), out NativeMethods.RECT rct))
                        {
                            // window right border!
                            if (PositionX + x <= rct.Right
                                && PositionX + x >= rct.Right + x // A little bit of wiggle-room so the sheep doesn't phase through the window
                                && PositionY + y < rct.Bottom // Ignore if below window. REMINDER: Y is inverted
                                && PositionY + y + Height > rct.Top // Ignore if above window
                                && !CheckTopWindow(false, window.Key) // Ignore windows that aren't visible on top
                                && rct.Right < ScreenArea.Right - Width // Ignore collision if the sheep doesn't have enough space to make it past the right screen border (ex: if an application is covering the entire screen)
                                )
                            {
                                // Set border animation with correct "only" value
                                int iBorderAnimation = window.Value == "Sheep" ? SetPetInteractionAnimations(rct) : Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOWSIDE);

                                // If a "none", "vertical+" or "petSide"/"windowSide" border animation exists, play it. Otherwise, pet ignores this collision and continues as normal.
                                if (iBorderAnimation >= 0)
                                {
                                    PositionX = rct.Right;
                                    x = 0;
                                    nextAnimation = iBorderAnimation;
                                    bNewAnimation = true;
                                }

                                break;
                            }
                        }
                    }
                }

                // If pet is on top of a window, check for left window corner
                if (hwndWindow != (IntPtr)0)
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
                                nextAnimation = iBorderAnimation;
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
                CheckFullScreen();  // used to check if another window is in full screen
                if (PositionX + x + Width > ScreenArea.X + ScreenArea.Width)    // right screen border!
                {
                    int iBorderAnimation = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL);
                    if (iBorderAnimation >= 0)
                    {
                        PositionX = ScreenArea.X + ScreenArea.Width - Width;
                        x = 0;
                        nextAnimation = iBorderAnimation;
                        bNewAnimation = true;
                    }
                    else
                    {
                        bLeavingScreen = true;
                    }
                }
                else    // run window border detection setup and then check left border of all valid windows!
                {
                    // Retrieve valid windows
                    Dictionary<IntPtr, string> windows = EnumerateWindows();

                    // For each valid window found:
                    foreach (KeyValuePair<IntPtr, string> window in windows)
                    {
                        // Get size and position of window
                        if (NativeMethods.GetWindowRect(new HandleRef(this, window.Key), out NativeMethods.RECT rct))
                        {
                            // window left border!
                            if (PositionX + x + Width >= rct.Left
                                && PositionX + x + Width <= rct.Left + x // A little bit of wiggle-room so the sheep doesn't phase through the window
                                && PositionY + y < rct.Bottom // Ignore if below window. REMINDER: Y is inverted
                                && PositionY + y + Height > rct.Top // Ignore if above window
                                && !CheckTopWindow(false, window.Key) // Ignore windows that aren't visible on top
                                && rct.Left > ScreenArea.Left + Width // Ignore collision if the sheep doesn't have enough space to make it past the left screen border (ex: if an application is covering the entire screen)
                                )
                            {
                                // Set border animation with correct "only" value
                                int iBorderAnimation = window.Value == "Sheep" ? SetPetInteractionAnimations(rct) : Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOWSIDE);

                                // If a "none", "vertical+" or "petSide"/"windowSide" border animation exists, play it. Otherwise, pet ignores this collision and continues as normal.
                                if (iBorderAnimation >= 0)
                                {
                                    PositionX = rct.Left - Width;
                                    x = 0;
                                    nextAnimation = iBorderAnimation;
                                    bNewAnimation = true;
                                }

                                break;
                            }
                        }
                    }
                }

                // If pet is on top of a window, check for right window corner
                if (hwndWindow != (IntPtr)0)
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
                                nextAnimation = iBorderAnimation;
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
                        nextAnimation = iBorderAnimation;
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
                            nextAnimation = iBorderAnimation;
                            bNewAnimation = true;
                            landedOnWindow = true;
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
                        nextAnimation = iBorderAnimation;
                        bNewAnimation = true;
                    }
                    else
                    {
                        bLeavingScreen = true;
                    }
                }
            }

            if (!bNewAnimation && AnimationStep >= CurrentAnimation.Sequence.TotalSteps) // animation over
            {
                int iNextAni;
                if(CurrentAnimation.Sequence.Action == "flip")
                {
                    IsMovingLeft = !IsMovingLeft;
                    spritesFlipped = !spritesFlipped;
                }
                if(hwndWindow != (IntPtr)0)
                {
                    iNextAni = Animations.SetNextSequenceAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                }
                else
                {
                        // If pet is outside the borders, spawn it again.
                    if (PositionX < ScreenBounds.X - Width || PositionX > ScreenBounds.X + ScreenBounds.Width)
                    {
                        iNextAni = -1;
                    }
                    else if (PositionY < ScreenBounds.Y - Height || PositionY > ScreenBounds.Y + ScreenBounds.Height)
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
                    if (timer1.Tag == null) timer1.Tag = 1.0;

                    double op = double.Parse(timer1.Tag.ToString());
                    timer1.Tag = op - 0.1;
                    Opacity = op;
                    if (op <= 0.1)
                    {
                        Close();
                        return;
                    }
                }
                else if (iNextAni >= 0)
                {
                    nextAnimation = iNextAni;
                    bNewAnimation = true;
                }
                else
                {
                        // Child doesn't have a spawn, they will be closed once the animation is over.
                    if(Name.IndexOf("child")==0)
                    {
                        StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "removing child");
                        Close();
                        return;
                    }
                    else
                    {
                        Play(false);
                        return;
                    }
                }
            }
                // If there is a Gravity-Next animation, check if gravity is present.
            else if(!bNewAnimation && CurrentAnimation.Gravity)
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
                            nextAnimation = Animations.SetNextGravityAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.NONE);
                            bNewAnimation = true;
                        }
                    }
                }
                else
                {
                    if (AnimationStep > 0 && CheckTopWindow(true))
                    {
                        // Window following is non-blocking and also runs at render cadence.
                        if (!FollowWindow())
                        {
                            hwndWindow = (IntPtr)0;
                            nextAnimation = Animations.SetNextGravityAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW);
                            bNewAnimation = true;
                        }
                    }
                }
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
                CommitLogicalPosition();
            }
            else
            {
                if (UseSmoothMovements)
                {
                    QueueMotion(movementDuration);
                }
                else
                {
					CommitLogicalPosition();
				}
            }
            if (bLeavingScreen) ResetMotion();
            if (bNewAnimation)
            {
                if (UseSmoothMovements && !IsLeaving && motion.IsMoving(motionClock.Elapsed.TotalMilliseconds))
                {
                    pendingAnimation = nextAnimation;
                    pendingWindowLanding = landedOnWindow;
					pendingTransition = true;
                    timer1.Interval = movementDuration;
                }
                else
                    CompleteNaturalTransition(nextAnimation, landedOnWindow);
            }
        }

        // Natural sequence/border/gravity transitions share first-frame publication
        // and the next sprite tick. Explicit drag/toss/respawn callers own their timing.
        private void CompleteNaturalTransition(int animation, bool landedOnWindow)
        {
            SetNewAnimation(animation);
            if (animation < 0 || IsDisposed) return; // respawn/disposal owns its timing
            // SetNewAnimation evaluates dynamic XML values for the selected display.
            if (landedOnWindow && CurrentAnimation.Start.Y.Value != 0)
                hwndWindow = (IntPtr)0;
            timer1.Interval = 1;
            pictureBox1.Image = GetSprite(CurrentAnimation.Sequence.Frames[0]);
        }

            /// <summary>
            /// When pets collide, this function decides what both of the pets should do
            /// <para>Type 1: Both pets play the same random animation</para>
            /// <para>Type 2: Otherwise, this pet has a 50% chance to mimic the animation that the other pet is already playing</para>
            /// <para>Type 3: As a last resort, both pets play different random animations, but Pet2's animation stays the same if they aren't facing each other</para>
            /// </summary>
            /// <remarks>
            /// Pro tip: to make pets mimic each other, add some "petSide" border animations with their probabilities at 0
            /// </remarks>
            /// <param name="rct">Rect of other pet window (the pet that this pet collided with)</param>
            /// <returns>ID of the next animation for this sheep to play. -1 if there is no animation.</returns>
        private int SetPetInteractionAnimations(NativeMethods.RECT rct)
        {
            // NOTE: For readability, this pet is referred to as sheep1, and the other pet it collides with is called sheep2

            FormPet[] sheeps = Program.Mainthread.sheeps; // List of sheep objects
            FormPet sheep2 = null; // The sheep/child object that is being collided with

            // Find sheep2 object

            // Run through each sheep and its children to find out which one is being collided with
            for (int i = 0; i < sheeps.Length; i++)
            {
                // Break the loop if there are no more sheeps to run through
                if (sheeps[i] == null) break;

                // If position matches the rect's position, it is sheep2
                if(sheeps[i].Left == rct.Left && sheeps[i].Bottom == rct.Bottom)
                {
                    sheep2 = sheeps[i];
                    break;
                }

                // Run through children incase they were collided with
                foreach (FormPet child in sheeps[i].childs)
                {
                    // Skip if null
                    if (child == null) continue;

                    // Check if child's position matches the position of the rect. Set it to sheep2 if so
                    if (child.Left == rct.Left && child.Bottom == rct.Bottom)
                    {
                        sheep2 = child;
                        break;
                    }
                }

                // Break out of loop if sheep2 was found
                if (sheep2 != null)
                    break;
            }

            // If sheep2 was not found (which should never happen), make sheep1 play one of its None/PetSide/Vertical+ border animations
            if (sheep2 == null)
                return Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.PETSIDE);

            // Get data for deciding which animation type will happen

            TAnimation sheep2CurrentAnim = sheep2.CurrentAnimation; // Initialized here to prevent runtime errors

            List<TNextAnimation> commonBorderAnims = new List<TNextAnimation>(); // List of all next end-border animations that both sheep have in common
            int mimicAnimID = -1; // If sheep2 is currently playing an animation that is also in sheep1's end-border animations, its ID is stored here

            // Determine if sheeps are both facing/moving in the same direction
            bool sameDirection = IsMovingLeft == sheep2.IsMovingLeft // check if they're facing the same direction
                    || (IsMovingLeft && sheep2.PositionX > PositionX) // or, check that sheep2 is infront of sheep1 in the correct direction (fixes infinite loops if sheep1 is moving backwards)
                    || (!IsMovingLeft && sheep2.PositionX < PositionX);

            // Run through sheep1's end-border animations to see if sheep2 has them
            foreach (TNextAnimation borderAnim in CurrentAnimation.EndBorder)
            {
                // Skip this animation if it isn't set to petSide, none or vertical+
                if (borderAnim.only != TNextAnimation.TOnly.PETSIDE && borderAnim.only != TNextAnimation.TOnly.NONE && borderAnim.only != TNextAnimation.TOnly.VERTICAL_)
                    continue;

                // If sheep2's current animation matches this sheep1 border animation, update mimicAnimID
                if (sheep2CurrentAnim.ID == borderAnim.ID)
                    mimicAnimID = borderAnim.ID;

                // Ignore border animation if sheeps are moving in same direction
                if (sameDirection)
                {
                    // If a mimic anim was already found, it's pointless to continue the loop
                    if (mimicAnimID != -1)
                        break;
                    else
                        continue;
                }

                // Ignore border animation if the probability is 0
                if (borderAnim.Probability == 0)
                    continue;

                // Run through each of sheep2's end-border animations to find any in common
                foreach (TNextAnimation sheep2BorderAnim in sheep2CurrentAnim.EndBorder)
                {
                    // Ignore border animation if the probability is 0
                    if (sheep2BorderAnim.Probability == 0)
                        continue;

                    // If the IDs match and the "only" values are correct, add this animation to the commonBorderAnims list
                    if (sheep2BorderAnim.ID == borderAnim.ID && (sheep2BorderAnim.only == TNextAnimation.TOnly.PETSIDE || sheep2BorderAnim.only == TNextAnimation.TOnly.NONE || sheep2BorderAnim.only == TNextAnimation.TOnly.VERTICAL_))
                    {
                        commonBorderAnims.Add(borderAnim);
                        break;
                    }
                }
            }

            // Play correct animation type

            int animationID = -1;

            // Type 1: if both sheep have some of the same end-border animations, randomly pick one of them and apply it to both sheep
            if (commonBorderAnims.Count > 0)
            {
                //Console.WriteLine("Type-1 interaction: both pets do same thing");
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "Type-1 interaction");

                // Apply to sheep1
                animationID = Animations.SetNextInteractAnimation(commonBorderAnims, TNextAnimation.TOnly.PETSIDE);
                // Apply to sheep2 if not -1 (which should never be the case)
                if(animationID >= 0)
                    sheep2.SetNewAnimation(animationID);
            }
            // Type 2: otherwise, if sheep2 is currently playing an animation that sheep1 has as an end-border animation (ex: sleeping), there's a 50% chance for sheep1 to "mimic" sheep2
            else if(mimicAnimID > 0 && rand.Next(0, 99) < 50)
            {
                //Console.WriteLine("Type-2 interaction: pet1 mimics pet2");
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "Type-2 interaction");

                animationID = mimicAnimID;
                SetNewAnimation(animationID);
            }
            // Type 3: if all else fails, set both sheep to random end-border petSide animations
            else
            {
                //Console.WriteLine("Type-3 interaction: Both sheeps play different random animations, but sheep2's animation is ignored if they aren't facing each other");
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.warning, "Type-3 interaction");

                // Sheep1 animation
                animationID = Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.PETSIDE);

                // Sheep2 animation (only if it is facing sheep1)
                if (!sameDirection)
                {
                    int sheep2AnimationID = sheep2.Animations.SetNextBorderAnimation(sheep2CurrentAnim.ID, TNextAnimation.TOnly.PETSIDE);
                    // Only set new animation if one was found
                    if (sheep2AnimationID >= 0)
                        sheep2.SetNewAnimation(sheep2AnimationID);
                }
            }

            return animationID;
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

            if (y <= 0) return -1;
            Rectangle area = ScreenArea;
            CheckFullScreen();

                // Enumerate all windows on the desktop.
            NativeMethods.EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == Handle) return true;    // form itself, don't parse

                    // Enumerate only visible windows
                if (NativeMethods.IsWindowVisible(hWnd))
                {
                    NativeMethods.RECT candidate;
                    if (!NativeMethods.GetWindowRect(new HandleRef(this, hWnd), out candidate) ||
                        PositionY + Height >= candidate.Top || PositionY + Height + y < candidate.Top ||
                        PositionX < candidate.Left - Width / 2 || PositionX + Width > candidate.Right + Width / 2 ||
                        PositionY <= 20 + area.Y) return true;
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
			IntPtr hwnd2 = NativeMethods.GetForegroundWindow();
			if (hwndFullscreenWindow == (IntPtr)0 && hwnd2 == Handle) return;

            if (NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out NativeMethods.RECT rct))
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

            /// <summary>
            /// Get a list of all valid windows
            /// </summary>
            /// <remarks>
            /// Ignores self, invisible windows and windows without a title bar
            /// </remarks>
            /// <returns>A dictionary of window keys values and names</returns>
        private Dictionary<IntPtr, string> EnumerateWindows()
        {
            NativeMethods.TITLEBARINFO titleBarInfo = new NativeMethods.TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();

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

            return windows;
        }

        private bool FollowWindow()
        {
            if (hwndWindow != IntPtr.Zero)
            {
				// Get window size and position of the current pet
				if (!NativeMethods.GetWindowRect(new HandleRef(this, hwndWindow), out NativeMethods.RECT rctO)) return false;

				// window disappeared! Maybe it was closed.
				if (rctO.Top == 0 && rctO.Bottom == 0)
                {
                    return false;
                }

                if (currentWindowSize.Top != rctO.Top || currentWindowSize.Left != rctO.Left || currentWindowSize.Right != rctO.Right)
                {
                    int oldWidth = currentWindowSize.Right - currentWindowSize.Left;
                    double ratio = oldWidth > 0 ? (double)(rctO.Right - rctO.Left) / oldWidth : 1;
                    double dy = rctO.Top - currentWindowSize.Top;
                    PositionX = rctO.Left + (PositionX - currentWindowSize.Left) * ratio;
                    PositionY += dy;
                    if (!motion.Initialized) motion.Reset(Left, Top, motionClock.Elapsed.TotalMilliseconds);
                    motion.Follow(currentWindowSize.Left, rctO.Left, ratio, dy);
                    RenderMotion(motionClock.Elapsed.TotalMilliseconds);
                    currentWindowSize = rctO;
                    return true;
                }
            }
            return false;
        }

            /// <summary>
            /// Check if given window is still valid (if another window cover the visual of this window, it must not be used as window)
            /// </summary>
            /// <remarks>
            /// If 'window' is left blank, it defaults to hwndWindow
            /// </remarks>
            /// <param name="bCheck">Check if it is still valid. Set false if window is not proofed, true if pet is already walking on a window => check if window is still valid.</param>
            /// <param name="window">The window to be checked. If left blank, default to hwndWindow</param>
            /// <returns>True if window is still valid and present. False if window is not anymore there.</returns>
            /// <seealso cref="NativeMethods.GetWindow(IntPtr, int)"/>
            /// <seealso cref="NativeMethods.GetTitleBarInfo(IntPtr, ref NativeMethods.TITLEBARINFO)"/>
        private bool CheckTopWindow(bool bCheck, IntPtr window = default)
        {
            // Default to hwndWindow if window is blank
            if (window == default)
                window = hwndWindow;

            // Check only if we have a valid window handler
            if (window != IntPtr.Zero)
            {
				// Get window size and position of the current pet
				if (!NativeMethods.GetWindowRect(new HandleRef(this, window), out NativeMethods.RECT rctO)) return true;

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
					if (hwnd2 == window)
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
                            if (NativeMethods.GetWindowRect(new HandleRef(this, hwnd2), out NativeMethods.RECT rct) &&
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
        private void PictureBox1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left && Name.IndexOf("child") < 0)
            {
                hwndWindow = (IntPtr)0;             // Remove window handles
                TopMost = false;
                TopMost = true;                     // Set again the topmost
				IsDragging = true;                   // Flag it as dragging pet
                IsTossing = false;                     // Flag it as not being tossed
                BeginSmoothDrag();
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
                        //Top = ScreenBounds.Y + spa.Start.Y.GetValue(DisplayIndex);
                        //Left = ScreenBounds.X + spa.Start.X.GetValue(DisplayIndex);
                        //PositionX = Left;
                        //PositionY = Top;
                        //OffsetY = 0.0;
                        int spawnIndex = menu.Index;
                        Play(false, spawnIndex);
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
        private void PictureBox1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Left || Name.IndexOf("child") >= 0 || !IsDragging) return;
            if (e.Button == MouseButtons.Left && Name.IndexOf("child") < 0 && IsDragging)
            {
                double now = motionClock.Elapsed.TotalMilliseconds;
                UpdateDragPosition(Cursor.Position, now);
                TossForce = dragVelocity.TossForce(now);

                // Log toss force
                StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "Toss force: (" + TossForce.X + ", " + TossForce.Y + ")");

                // If the toss force's magnitude (length) is bigger than this value, fling that thang >:)
                if(TossForce.Length() > 5)
                {
                    // If toss animation exists, set it. Otherwise, remain in the dragging animation.
                    if(Animations.AnimationToss != -1)
                        SetNewAnimation(Animations.AnimationToss);

                    IsTossing = true;

                    // Init vertical toss velocity
                    tossVertVel = TossForce.Y;
                    tossUpdatedAt = now;

                    // Force the interval while tossing to be a nice and smooth 30 (prevents laggy tossing for pets with slow drag animation intervals)
                    timer1.Interval = 30;
                }
                // If toss force wasn't large enough, fall like normal
                else
                {
                    SetNewAnimation(Animations.AnimationFall);
                }
                
            }
            if(IsDragging)
            {
                // if it was dragged, check if the screen is different
                // if(Program.MyData.GetMultiscreen()) <-- If manually moved to another screen, set the new screen as default screen.
                {
                    Screen[] screens = Screen.AllScreens;
                    for(var k=0;k<screens.Length;k++)
                    {
                        Rectangle bounds = screens[k].Bounds;

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
            pictureBox1.Capture = false;
            ResetMotion();
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
        /// Drops are allowed for files (windows drag&drop) and text (webbrowser drag&drop)
        /// </summary>
        /// <param name="sender">Caller object.</param>
        /// <param name="e">Mouse event values.</param>
        private void Form2_DragEnter(object sender, DragEventArgs e)
        {
            StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "dragging file...");
            if (e.Data.GetDataPresent(DataFormats.FileDrop) ||
				e.Data.GetDataPresent(DataFormats.Text)) e.Effect = DragDropEffects.Copy;
		}

		/// <summary>
		/// Pet allows dropping other files. If a XML file was dropped, this one will be loaded.
		/// It can be a XML file, but it can be also a Browser-XML-string (see https://adrianotiger.github.io/web-esheep/tools/minecraft.html webpage)
		/// </summary>
		/// <example>
        /// Javascript example to generate a xml string (file would also be possible, but some browsers will not allow it)
		/// myElement.addEventListener("dragstart", (e)=> {
		///   e.dataTransfer.setData(
		///     "text/plain",
		///     `${xmlString}`
		///   );
		///   e.dataTransfer.setData("text/xml", xmlString);
		///   e.dataTransfer.effectAllowed = "copy";
		/// });
		/// </example>
		/// <param name="sender">Caller object.</param>
		/// <param name="e">Dragging event values.</param>
		private void Form2_DragDrop(object sender, DragEventArgs e)
        {
            var files = (string[])e.Data.GetData(DataFormats.FileDrop);
			var xmlString = e.Data.GetData(DataFormats.Text);
            // If a file was dropped (XML as a file)
			if (files != null)
            {
				StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "files dragged:");
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
			// If a string was dropped (XML as a string)
			else if (xmlString != null)
            {
				StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.info, "xml string dragged:");
				try
				{
					var doc = new XmlDocument();
					doc.LoadXml(xmlString as String);

					Program.Mainthread.KillSheep(this);
					Program.MyData.SetXml(xmlString as String, "..");
					Program.Mainthread.LoadNewXMLFromString(xmlString as String);
				}
				catch
				{
					StartUp.AddDebugInfo(StartUp.DEBUG_TYPE.error, "invalid animation xml");
				}
			}
        }

        private void Form2_FormClosed(object sender, FormClosedEventArgs e)
        {
            if(!IsDisposed) Dispose();
        }

		private void PictureBox1_Click(object sender, EventArgs e)
		{

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
