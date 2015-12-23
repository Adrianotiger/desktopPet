using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace desktopPet
{
    public partial class Form2 : Form
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetWindowRect(HandleRef hWnd, out RECT lpRect);
        
        [DllImport("user32.dll")]
        private static extern bool EnumWindows(EnumWindowsProc enumFunc, int lParam);

        [DllImport("USER32.DLL")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("USER32.DLL")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        [DllImport("USER32.DLL")]
        private static extern bool GetTitleBarInfo(IntPtr hWnd, ref TITLEBARINFO pti);

        [DllImport("USER32.DLL")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("USER32.DLL")]
        private static extern IntPtr GetWindow(IntPtr hWnd, int nCmdShow);
                
        delegate bool EnumThreadDelegate(IntPtr hWnd, IntPtr lParam);
        private delegate bool EnumWindowsProc(IntPtr hWnd, int lParam);

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

        int iAnimationStep;
        TAnimation CurrentAnimation;
        IntPtr hwndWindow = (IntPtr)0;
        bool bMoveLeft = true;
        //Form2 formX = null;
        Animations Animations;
        Xml Xml;
        bool bDragging = false;

        public Form2()
        {
            InitializeComponent();
        }

        public Form2(Animations animations, Xml xml)
        {
            Animations = animations;
            Xml = xml;
            InitializeComponent();
            Visible = false;
        }

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

        public void Show(int x, int y)
        {
            Width = x;
            Height = y;

            pictureBox1.Width = x;
            pictureBox1.Height = y;
            pictureBox1.Top = 0;
            pictureBox1.Left = 0;
            pictureBox1.Tag = 0;

            Top = Screen.PrimaryScreen.WorkingArea.Height - y;
            Left = Screen.PrimaryScreen.WorkingArea.Width;
            iAnimationStep = 0;

            Show();
        }

        public void addImage(Image im)
        {
            if(imageList1.Images.Count == 0)
            {
                imageList1.ImageSize = new Size(im.Width, im.Height);
            }
            imageList1.Images.Add(im);
        }

        public void Play(bool first)
        {
            timer1.Enabled = false;

            iAnimationStep = 0;
            hwndWindow = (IntPtr)0;

            TSpawn spawn = Animations.GetRandomSpawn();
            Top = spawn.Start.Y.GetValue();
            Left = spawn.Start.X.GetValue();
            Visible = true;
            SetNewAnimation(spawn.Next);
            
            timer1.Enabled = true;
        }
        
        public void Kill()
        {
            //SetNewAnimation(AnimationType.Die);
        }

        public void Whats()
        {
            //SetNewAnimation(AnimationType.What);
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            timer1.Enabled = false;
            if (iAnimationStep < 0) iAnimationStep = 0;
            NextStep();
            iAnimationStep++;
            timer1.Enabled = true;
        }

        private int GetRandomNumber()
        {
            Random Rand = new Random();
            return Rand.Next(0, 100);
        }

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

            if (bDragging)
            {
                Left = Cursor.Position.X - Width / 2;
                Top = Cursor.Position.Y + 2;
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
                    if (Left + x < 0)    // left screen border!
                    {
                        x = -Left;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL));
                        bNewAnimation = true;
                    }
                }
                else
                {
                    RECT rct;
                    if (GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (Left + x < rct.Left)    // left window border!
                        {
                            x = -Left + rct.Left;
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
                    if (Left + x + Width > Screen.PrimaryScreen.WorkingArea.Width)    // right screen border!
                    {
                        x = Screen.PrimaryScreen.WorkingArea.Width - Width - Left;
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.VERTICAL));
                        bNewAnimation = true;
                    }
                }
                else
                {
                    RECT rct;
                    if (GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (Left + x + Width > rct.Right)    // right window border!
                        {
                            x = rct.Right - Width - Left;
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
                    if (Top + y > Screen.PrimaryScreen.WorkingArea.Height - Height) // border detected!
                    {
                        SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.TASKBAR));
                        bNewAnimation = true;
                        y = Screen.PrimaryScreen.WorkingArea.Height - Top - Height;
                    }
                    else
                    {
                        int iWindowTop = FallDetect(y);
                        if (iWindowTop > 0)
                        {
                            SetNewAnimation(Animations.SetNextBorderAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                            bNewAnimation = true;
                            y = iWindowTop - Top - Height;
                        }
                    }
                }
            }

            if (iAnimationStep >= CurrentAnimation.Sequence.TotalSteps - 1) // animation over
            {
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
                    SetNewAnimation(Animations.SetNextSequenceAnimation(CurrentAnimation.ID, TNextAnimation.TOnly.WINDOW));
                }
                else
                {
                    SetNewAnimation(Animations.SetNextSequenceAnimation(CurrentAnimation.ID, Top + Height + y >= Screen.PrimaryScreen.WorkingArea.Height - 2 ? TNextAnimation.TOnly.TASKBAR : TNextAnimation.TOnly.NONE));
                }
                bNewAnimation = true;
            }
            else if(CurrentAnimation.Gravity)
            {
                if(hwndWindow == (IntPtr)0)
                {
                    if(Top + y < Screen.PrimaryScreen.WorkingArea.Height - Height)
                    {
                        if(Top + y + 3 >= Screen.PrimaryScreen.WorkingArea.Height - Height) // allow 3 pixels to move without fall
                        {
                            y = Screen.PrimaryScreen.WorkingArea.Height - Top - Height;
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

            Left += x;
            Top += y;
        }

        private int FallDetect(int y)
        {
            RECT rct;
            Dictionary<IntPtr, string> windows = new Dictionary<IntPtr, string>();
            TITLEBARINFO titleBarInfo = new TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

            EnumWindows(delegate (IntPtr hWnd, int lParam)
            {
                if (hWnd == Handle) return true;

                if (IsWindowVisible(hWnd))
                {
                    if (!GetTitleBarInfo(hWnd, ref titleBarInfo)) return true;
                    if ((titleBarInfo.rgstate[0] & 0x00008000) > 0) return true;    // invisible

                    StringBuilder sTitle = new StringBuilder(128);
                    GetWindowText(hWnd, sTitle, 128);

                    if (sTitle.Length > 0)
                    {
                        windows[hWnd] = sTitle.ToString();
                    }
                }
                return true;
            }, 0);

            foreach (KeyValuePair<IntPtr, string> window in windows)
            {
                if (GetWindowRect(new HandleRef(this, window.Key), out rct))
                {
                    //Console.WriteLine("Window title: {0}", window.Value);

                    if (Top + Height < rct.Top && Top + Height + y >= rct.Top &&
                        Left >= rct.Left && Left + Width <= rct.Right &&
                        Top > 30)
                    {
                        hwndWindow = window.Key;
                        if (!CheckTopWindow(false))
                        {
                            ShowWindow(window.Key, 0);
                            ShowWindow(window.Key, 5);
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
        private void Walk(int step)
        {
            pictureBox1.Image = imageList1.Images[2 + step % 2];
            if (bMoveLeft)
            {
                this.Left -= 4;
                if ((int)hwndWindow != 0)
                {
                    RECT rct;
                    if (CheckTopWindow(true))
                    {
                        hwndWindow = (IntPtr)0;
                        SetNewAnimation(AnimationType.FallWinDown);
                    }
                    else if (GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (this.Top < rct.Top - 45 || this.Top > rct.Top - 25)
                        {
                            SetNewAnimation(AnimationType.FallWinDown);
                        }
                        else if (this.Left < rct.Left - 5)
                        {
                            this.Left = rct.Left - 5;
                            SetNewAnimation(AnimationType.Rotate1);
                        }
                    }
                    else
                    {
                        hwndWindow = (IntPtr)0;
                        SetNewAnimation(AnimationType.FallWinDown);
                    }
                }
                else if (this.Left <= 0)
                {
                    this.Left = 0;
                    SetNewAnimation(AnimationType.Rotate1);
                }
            }
            else
            {
                this.Left += 4;
                if ((int)hwndWindow != 0)
                {
                    RECT rct;
                    if (CheckTopWindow(true))
                    {
                        hwndWindow = (IntPtr)0;
                        SetNewAnimation(AnimationType.FallWinDown);
                    }
                    else if (GetWindowRect(new HandleRef(this, hwndWindow), out rct))
                    {
                        if (this.Top < rct.Top - 45 || this.Top > rct.Top - 25)
                        {
                            SetNewAnimation(AnimationType.FallWinDown);
                        }
                        else if (this.Right > rct.Right - 35)
                        {
                            this.Left = rct.Right - 35;
                            SetNewAnimation(AnimationType.Rotate1);
                        }
                    }
                    else
                    {
                        hwndWindow = (IntPtr)0;
                        SetNewAnimation(AnimationType.FallWinDown);
                    }
                }
                else if (this.Left >= Screen.PrimaryScreen.WorkingArea.Width - 40)
                {
                    this.Left = Screen.PrimaryScreen.WorkingArea.Width - 40;
                    SetNewAnimation(AnimationType.Rotate1);
                }
            }
            
            if (step > 20)
            {
                int rand = GetRandomNumber();

                if (rand == 0)
                    SetNewAnimation(AnimationType.Sleep1);
                else if(rand == 1)
                    SetNewAnimation(AnimationType.Sleep2);
                else if (rand == 2 && (int)hwndWindow != 0)
                    SetNewAnimation(AnimationType.Piss);
                else if (rand < 8 && (int)hwndWindow == 0)
                    SetNewAnimation(AnimationType.Run);
                else if (rand < 9 && (int)hwndWindow == 0)
                    SetNewAnimation(AnimationType.Exit1);
                else
                    SetNewAnimation(AnimationType.Walk);
            }
        }

        private void Run(int step)
        {
            pictureBox1.Image = imageList1.Images[4 + step % 2];
            if (bMoveLeft)
            {
                this.Left -= 10;

                if (this.Left <= 0)
                {
                    this.Left = 0;
                    SetNewAnimation(AnimationType.Boing);
                }
            }
            else
            {
                this.Left += 10;

                if (this.Left >= Screen.PrimaryScreen.WorkingArea.Width - 40)
                {
                    this.Left = Screen.PrimaryScreen.WorkingArea.Width - 40;
                    SetNewAnimation(AnimationType.Boing);
                }
            }

            if (step >= 25)
            {
                if(GetRandomNumber() < 5)
                    SetNewAnimation(AnimationType.Run);
                else
                    SetNewAnimation(AnimationType.Walk);
            }
        }

        private void Boing(int step)
        {
            int[] iPos = { 62, 62, 62, 63, 64, 65, 66, 67, 68, 69, 70, 63, 64, 65, 66, 67, 68, 69, 70, 3, 3, 3, 3, 3 };
            pictureBox1.Image = imageList1.Images[iPos[step]];
            if (step > 2 && step < 20)
            {
                if (bMoveLeft) this.Left += 5;
                else this.Left -= 5;
            }

            if (step >= 21)
            {
                if(GetRandomNumber() < 2)
                    SetNewAnimation(AnimationType.Run);
                else if (GetRandomNumber() < 15)
                    SetNewAnimation(AnimationType.Rotate1);
                else
                    SetNewAnimation(AnimationType.Walk);
            }
        }

        private void Piss(int step)
        {
            if (step < 6)
            {
                int[] iPos = { 3, 12, 13, 103, 104, 105, 106 };
                pictureBox1.Image = imageList1.Images[iPos[step]];
            }
            else if (step < 40)
            {
                pictureBox1.Image = imageList1.Images[105 + (step % 2)];
            }
            else
            {
                int[] iPos = { 104, 104, 104, 13, 13, 13, 13, 12, 3, 3, 3, 3 };
                pictureBox1.Image = imageList1.Images[iPos[step - 40]];
                if (step > 49)
                {
                    SetNewAnimation(AnimationType.Walk);
                }
            }
            
        }

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

        private void ChangeDirection(int step)
        {
            pictureBox1.Image = imageList1.Images[9 + step % 3];
            if (step == 0)
            {
                for (int i = 0; i < imageList1.Images.Count; i++)
                {
                    if (i == 9 + 0 || i == 9 + 1 || i == 9 + 2) continue;
                    Image im = imageList1.Images[i];
                    im.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    imageList1.Images[i] = im;
                }
            }
            if (step >= 2)
            {
                pictureBox1.Update();
                for (int i = 9; i < 9 + 3; i++)
                {
                    Image im = imageList1.Images[i];
                    im.RotateFlip(RotateFlipType.RotateNoneFlipX);
                    imageList1.Images[i] = im;
                }
                bMoveLeft = !bMoveLeft;
                SetNewAnimation(AnimationType.Walk);
            }
        }


        private void Fall(int step)
        {
            pictureBox1.Image = imageList1.Images[46];

            if (step < 40)
                this.Top += step / 2;
            else 
                this.Top += 20;

            RECT rct;
            Dictionary<IntPtr, String> windows = new Dictionary<IntPtr, String>();
            TITLEBARINFO titleBarInfo = new TITLEBARINFO();
            titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

            EnumWindows(delegate(IntPtr hWnd, int lParam)
            {
                if (hWnd == this.Handle) return true;

                if (IsWindowVisible(hWnd))
                {
                    if (!GetTitleBarInfo(hWnd, ref titleBarInfo)) return true;
                    if ((titleBarInfo.rgstate[0] & 0x00008000) > 0) return true;    // invisible
                    
                    StringBuilder sTitle = new StringBuilder(128);
                    GetWindowText(hWnd, sTitle, 128);

                    if (sTitle.Length > 0)
                    {
                        windows[hWnd] = sTitle.ToString();
                    }
                }
                return true;
            }, 0);

            foreach (KeyValuePair<IntPtr, string> window in windows)
            {
                if (GetWindowRect(new HandleRef(this, window.Key), out rct))
                {
                    //Console.WriteLine("Window title: {0}", window.Value);

                    if (this.Top >= rct.Top - 10 && this.Top <= rct.Top + 20 &&
                        this.Left >= rct.Left && this.Left + 40 <= rct.Right && 
                        this.Top > 30)
                    {
                        hwndWindow = window.Key;
                        if (!CheckTopWindow(false))
                        {
                            this.Top = rct.Top - 39;
                            ShowWindow(window.Key, 0);
                            ShowWindow(window.Key, 5);
                            Thread.Sleep(100);
                            SetNewAnimation(AnimationType.Walk);
                            return;
                        }
                        else
                        {
                            hwndWindow = (IntPtr)0;
                        }
                    }
                }
            }
            
            if (this.Top > Screen.PrimaryScreen.WorkingArea.Height - 40)
            {
                this.Top = Screen.PrimaryScreen.WorkingArea.Height - 39;
                if (timer1.Interval < 500)
                {
                    if(step > 40)
                    {
                        pictureBox1.Image = imageList1.Images[48];
                    }
                    else
                    {
                        pictureBox1.Image = imageList1.Images[47];
                    }
                    timer1.Interval = 1000;
                }
                else
                {
                    SetNewAnimation(AnimationType.Walk);
                }
            }
        }

        private void Die(int step)
        {
            if(step <= 0) pictureBox1.Image = imageList1.Images[80];
            else          pictureBox1.Image = imageList1.Images[96];
            if (step > 10 && step <= 20)
            {
                this.Opacity = 1.0 - (step - 10) / 10.0;
            }
        }

        private void What(int step)
        {
            pictureBox1.Image = imageList1.Images[50 + step % 2];
            
            if (step > 10)
            {
                SetNewAnimation(AnimationType.Walk);
            }
        }

        private void Sleep1(int step)
        {
            if (step <= 11)
            {
                int[] iPos = { 3, 37, 38, 39, 39, 38, 37, 77, 78, 79, 80, 1 };
                pictureBox1.Image = imageList1.Images[iPos[step]];
                pictureBox1.Tag = 0;
                timer1.Interval = 150 + GetRandomNumber();
            }
            else if (pictureBox1.Tag.ToString() == "0")
            {
                if (step > 50 && GetRandomNumber() < 2)
                {
                    pictureBox1.Tag = step;
                    timer1.Interval = 300;
                }
            }
            else
            {
                int iBaseStep = Int32.Parse(pictureBox1.Tag.ToString());
                int[] iPos = { 56, 57, 58, 59, 39, 39, 38, 37, 3, 3, 3, 3, 3 };
                pictureBox1.Image = imageList1.Images[iPos[step - iBaseStep]];

                if (step - iBaseStep > 10)
                {
                    SetNewAnimation(AnimationType.Walk);
                }
            }
        }

        private void Sleep2(int step)
        {
            if (step <= 16)
            {
                int[] iPos = { 3, 3, 9, 10, 10, 10, 10, 34, 35, 35, 34, 34, 35, 35, 36, 35, 36 };
                pictureBox1.Image = imageList1.Images[iPos[step]];
                pictureBox1.Tag = 0;
                timer1.Interval = 150 + GetRandomNumber();
            }
            else if (pictureBox1.Tag.ToString() == "0")
            {
                if (step > 50 && GetRandomNumber() < 2)
                {
                    pictureBox1.Tag = step;
                    timer1.Interval = 300;
                }
            }
            else
            {
                int iBaseStep = Int32.Parse(pictureBox1.Tag.ToString());
                int[] iPos = { 35, 35, 35, 34, 10, 9, 3, 107, 108, 107, 108, 107, 3, 3, 3 };
                pictureBox1.Image = imageList1.Images[iPos[step - iBaseStep]];

                if (step - iBaseStep > 13)
                {
                    SetNewAnimation(AnimationType.Walk);
                }
            }
        }
        */
        
        private bool CheckTopWindow(bool bCheck)
        {
            if ((int)hwndWindow != 0)
            {
                RECT rctO;
                RECT rct;
                GetWindowRect(new HandleRef(this, hwndWindow), out rctO);

                if (bCheck)
                {
                    if (rctO.Top > Top + Height + 2) return true;
                    if (rctO.Top < Top + Height - 2) return true;
                }

                TITLEBARINFO titleBarInfo = new TITLEBARINFO();
                titleBarInfo.cbSize = Marshal.SizeOf(titleBarInfo);

                IntPtr hwnd2 = GetWindow(hwndWindow, 3);
                while (hwnd2 != (IntPtr)0)
                {
                    StringBuilder sTitle = new StringBuilder(128);
                    GetWindowText(hwnd2, sTitle, 128);

                    if (GetTitleBarInfo(hwnd2, ref titleBarInfo))
                    {
                        if (sTitle.Length > 0 && GetWindowRect(new HandleRef(this, hwnd2), out rct) && titleBarInfo.rcTitleBar.Bottom > 0)
                        {
                            if (rct.Top < rctO.Top && rct.Bottom > rctO.Top)
                            {
                                if (rct.Left < Left && rct.Right > Left + 40 && iAnimationStep > 4) return true;
                            }
                        }
                    }
                    hwnd2 = GetWindow(hwnd2, 3);
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
    }
}
