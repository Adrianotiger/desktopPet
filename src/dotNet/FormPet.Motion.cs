using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace DesktopPet
{
    public partial class FormPet
    {
        private readonly Stopwatch motionClock = Stopwatch.StartNew();
        private readonly MotionTrack motion = new MotionTrack();
        private readonly DragVelocity dragVelocity = new DragVelocity();
        private Timer motionTimer;
        private Point dragAnchor;
        private double tossUpdatedAt;
        private bool spritesFlipped;
        private readonly Dictionary<int, Image> flippedSprites = new Dictionary<int, Image>();
        private Timer followFallback;
        private IntPtr followHook, hookedWindow;
        private WinEventCallback followCallback;
        private const uint EventObjectLocationChange = 0x800B;
        private delegate void WinEventCallback(IntPtr hook, uint evt, IntPtr window, int objectId, int childId, uint thread, uint time);
        [DllImport("user32.dll")]
        private static extern IntPtr SetWinEventHook(uint min, uint max, IntPtr module, WinEventCallback callback, uint process, uint thread, uint flags);
        [DllImport("user32.dll")]
        private static extern bool UnhookWinEvent(IntPtr hook);
        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr window, out uint process);

        private Image GetSprite(int index)
        {
            if (!spritesFlipped) return imageList1.Images[index];
            Image image;
            if (!flippedSprites.TryGetValue(index, out image))
            {
                image = imageList1.Images[index];
                image.RotateFlip(RotateFlipType.RotateNoneFlipX);
                flippedSprites.Add(index, image);
            }
            return image;
        }

        private void SyncFollowHook()
        {
            if (hookedWindow == hwndWindow) return;
            if (followHook != IntPtr.Zero) UnhookWinEvent(followHook);
            followHook = IntPtr.Zero;
            hookedWindow = hwndWindow;
            if (hookedWindow != IntPtr.Zero)
            {
                uint process;
                uint thread = GetWindowThreadProcessId(hookedWindow, out process);
                if (thread != 0)
                    followHook = SetWinEventHook(EventObjectLocationChange, EventObjectLocationChange, IntPtr.Zero, followCallback, process, thread, 0);
                if (followHook == IntPtr.Zero) Debug.WriteLine("Window event hook unavailable; using timer fallback.");
            }
            followFallback.Enabled = hookedWindow != IntPtr.Zero;
        }

        private void AdvanceToss(double now)
        {
            // Preserve the original force units (pixels per 30 ms), but integrate
            // continuously. Bound pauses to 50 ms so an interrupted UI cannot teleport.
            double dt = Math.Max(0, Math.Min(50, now - tossUpdatedAt)) / 30.0;
            tossUpdatedAt = now;
            if (dt == 0) return;
            Rectangle area = ScreenArea;
            double nextX = PositionX + TossForce.X * dt;
            double right = area.Right - Width;
            if (nextX < area.Left || nextX > right)
            {
                double edge = nextX < area.Left ? area.Left : right;
                // Consume the remaining travel after impact instead of pausing a frame.
                nextX = edge - (nextX - edge) * .3;
                TossForce.X *= -.3f;
            }
            PositionX = Math.Max(area.Left, Math.Min(right, nextX));
            double dy = tossVertVel * dt + .75 * dt * dt;
            tossVertVel += 1.5 * dt;
            int windowTop = dy > 0 ? FallDetect((int)Math.Ceiling(dy)) : -1;
            double ground = area.Bottom - Height;
            bool land = PositionY + dy >= ground;
            if (windowTop != -1 && windowTop - Height <= ground)
            {
                ground = windowTop - Height;
                land = true;
            }
            PositionY = land ? ground : PositionY + dy;
            OffsetY = 0;
            motion.Reset(PositionX, PositionY, now);
            RenderMotion(now);
            if (land)
            {
                if ((TossForce.X < 0 && !IsMovingLeft) || (TossForce.X > 0 && IsMovingLeft))
                {
                    IsMovingLeft = !IsMovingLeft;
                    spritesFlipped = !spritesFlipped;
                }
                IsTossing = false;
                SetNewAnimation(tossVertVel < 40 ? Animations.AnimationFallSoft : Animations.AnimationFallHard);
                pictureBox1.Image = GetSprite(CurrentAnimation.Sequence.Frames[0]);
                SyncFollowHook();
            }
        }

        private bool timerResolutionRequested;

        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern uint timeBeginPeriod(uint period);
        [DllImport("winmm.dll", ExactSpelling = true)]
        private static extern uint timeEndPeriod(uint period);

        private void SetMotionTimer(bool enabled)
        {
            if (enabled && !timerResolutionRequested)
            {
                timerResolutionRequested = timeBeginPeriod(1) == 0;
                if (!timerResolutionRequested) Debug.WriteLine("High-resolution motion timing unavailable; using default Windows timer precision.");
            }
            motionTimer.Enabled = enabled;
            if (!enabled && timerResolutionRequested)
            {
                timeEndPeriod(1);
                timerResolutionRequested = false;
            }
        }

        private void DisposeMotion()
        {
            // Releasing a captured PictureBox during disposal must not start a toss.
            IsDragging = false;
            IsTossing = false;
            if (motionTimer != null) SetMotionTimer(false);
            if (followFallback != null) followFallback.Stop();
            if (followHook != IntPtr.Zero) { UnhookWinEvent(followHook); followHook = IntPtr.Zero; }
            foreach (Image image in flippedSprites.Values) image.Dispose();
            flippedSprites.Clear();
        }

        private void InitializeMotion()
        {
            followCallback = (hook, evt, window, objectId, childId, thread, time) =>
            {
                if (!IsDisposed && !Disposing && window == hwndWindow && objectId == 0 && childId == 0 && !IsLeaving && !IsDragging)
                    FollowWindow();
            };
            followFallback = new Timer(components) { Interval = 100 };
            followFallback.Tick += (sender, e) =>
            {
                SyncFollowHook();
                if (!IsLeaving && !IsDragging) FollowWindow();
            };
            motionTimer = new Timer(components) { Interval = 8 };
            motionTimer.Tick += MotionTimer_Tick;
            pictureBox1.MouseMove += (sender, e) =>
            {
                if (IsDragging) UpdateDragPosition(Cursor.Position, motionClock.Elapsed.TotalMilliseconds);
            };
            pictureBox1.MouseCaptureChanged += (sender, e) =>
            {
                if (!IsDisposed && !Disposing && IsDragging && !pictureBox1.Capture)
                    PictureBox1_MouseUp(sender, new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
            };
        }

        private void ResetMotion()
        {
            motion.Reset(Left, Top, motionClock.Elapsed.TotalMilliseconds);
            SyncFollowHook();
            SetMotionTimer(IsDragging || IsTossing);
        }

        private void QueueMotion(int milliseconds)
        {
            double now = motionClock.Elapsed.TotalMilliseconds;
            if (!motion.Initialized) motion.Reset(Left, Top, now);
            SyncFollowHook();
            motion.MoveTo(PositionX, PositionY + OffsetY, now, milliseconds);
            SetMotionTimer(true);
        }

        private void RenderMotion(double now)
        {
            PointF p = motion.Sample(now);
            Point position = new Point((int)Math.Round(p.X), (int)Math.Round(p.Y));
            if (Location != position) Location = position;
        }

        private void MotionTimer_Tick(object sender, EventArgs e)
        {
            if (IsDisposed || Disposing) { SetMotionTimer(false); return; }
            double now = motionClock.Elapsed.TotalMilliseconds;
            if (IsDragging)
            {
                UpdateDragPosition(Cursor.Position, now);
                return;
            }
            if (IsTossing) AdvanceToss(now);
            else
            {
                SyncFollowHook();
                if (hwndWindow != IntPtr.Zero && !IsLeaving) FollowWindow();
                // FollowWindow may have rendered a newer timestamp. Never render backwards.
                if (motion.Initialized) RenderMotion(motionClock.Elapsed.TotalMilliseconds);
            }
            if (!IsTossing && !motion.IsMoving(now)) SetMotionTimer(false);
        }

        private void BeginSmoothDrag()
        {
            // Preserve the exact grab point, like dragging a normal window.
            SyncFollowHook();
            dragAnchor = PointToClient(Cursor.Position);
            PositionX = Left; PositionY = Top;
            OffsetY = 0;
            double now = motionClock.Elapsed.TotalMilliseconds;
            motion.Reset(Left, Top, now);
            dragVelocity.Reset(Left, Top, now);
            pictureBox1.Capture = true;
            SetMotionTimer(true);
        }

        private void UpdateDragPosition(Point cursor, double now)
        {
            Point position = new Point(cursor.X - dragAnchor.X, cursor.Y - dragAnchor.Y);
            PositionX = position.X; PositionY = position.Y;
            OffsetY = 0;
            dragVelocity.Add(PositionX, PositionY, now);
            motion.Reset(PositionX, PositionY, now);
            if (Location != position) Location = position;
        }
    }
}
