using System;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;

namespace DesktopPet
{
    // Interpolate presentation only; collision/animation coordinates stay authoritative.
    internal sealed class MotionTrack
    {
        private double fromX, fromY, toX, toY, started, duration;
        public bool Initialized { get; private set; }
        public void Reset(double x, double y, double now)
        {
            fromX = toX = x; fromY = toY = y;
            started = now; duration = 0; Initialized = true;
        }
        public PointF Sample(double now)
        {
            double t = duration <= 0 ? 1 : Math.Max(0, Math.Min(1, (now - started) / duration));
            return new PointF((float)(fromX + (toX - fromX) * t), (float)(fromY + (toY - fromY) * t));
        }
        public void MoveTo(double x, double y, double now, double milliseconds)
        {
            if (!Initialized) { Reset(x, y, now); return; }
            PointF position = Sample(now);
            fromX = position.X; fromY = position.Y; toX = x; toY = y;
            started = now; duration = Math.Max(1, milliseconds);
        }
        public bool IsMoving(double now)
        {
            return Initialized && now < started + duration && (fromX != toX || fromY != toY);
        }
        public void Follow(double oldLeft, double newLeft, double ratio, double dy)
        {
            fromX = newLeft + (fromX - oldLeft) * ratio;
            toX = newLeft + (toX - oldLeft) * ratio;
            fromY += dy; toY += dy;
        }
    }

    internal sealed class DragVelocity
    {
        private struct Sample { public double Time, X, Y; }
        private readonly List<Sample> samples = new List<Sample>();
        public void Reset(double x, double y, double now)
        {
            samples.Clear(); Add(x, y, now);
        }
        public void Add(double x, double y, double now)
        {
            if (samples.Count > 0 && now <= samples[samples.Count - 1].Time) return;
            samples.Add(new Sample { X = x, Y = y, Time = now });
            while (samples.Count > 2 && samples[1].Time <= now - 80) samples.RemoveAt(0);
        }
        public Vector2 TossForce(double now)
        {
            if (samples.Count < 2) return Vector2.Zero;
            Sample first = samples[0], last = samples[samples.Count - 1];
            double elapsed = last.Time - first.Time;
            if (elapsed < 8 || now - last.Time > 80) return Vector2.Zero;
            return new Vector2((float)((last.X - first.X) / elapsed * 10), (float)((last.Y - first.Y) / elapsed * 10));
        }
    }
}
