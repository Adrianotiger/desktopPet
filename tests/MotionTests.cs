using System;
using System.Drawing;
using DesktopPet;

class MotionTests
{
    static int checks;
    static void Near(double expected, double actual, string description)
    {
        if (Math.Abs(expected - actual) > 0.001) throw new Exception(description + ": " + actual + " != " + expected);
        checks++;
    }
    static void Main()
    {
        var track = new MotionTrack();
        track.Reset(100, 200, 0);
        track.MoveTo(140, 160, 0, 200);
        Near(110, track.Sample(50).X, "quarter-step movement");
        Near(180, track.Sample(100).Y, "diagonal interpolation");
        Near(140, track.Sample(500).X, "late tick clamps to endpoint");
        track.Reset(100, 200, 0);
        track.MoveTo(140, 200, 0, 200);
        track.MoveTo(80, 200, 50, 100);
        Near(110, track.Sample(50).X, "direction change is continuous");
        Near(95, track.Sample(100).X, "direction change interpolates");
        Near(80, track.Sample(150).X, "no overshoot at border");
        track.Reset(-1800, -200, 200);
        Near(-1800, track.Sample(210).X, "respawn negative monitor does not slide across screens");
        track.MoveTo(-1700, -100, 200, 100);
        track.Follow(-1900, -1850, 2, 20);
        Near(-1650, track.Sample(200).X, "resize transforms current presentation");
        Near(-1450, track.Sample(300).X, "resize transforms logical target");
        Near(-80, track.Sample(300).Y, "window following translates both endpoints");

        foreach (int cadence in new[] { 8, 20, 40 })
        {
            var velocity = new DragVelocity();
            velocity.Reset(0, 0, 0);
            for (int t = cadence; t <= 160; t += cadence) velocity.Add(t, -t, t);
            Near(10, velocity.TossForce(160).X, "toss speed independent of event cadence");
            Near(-10, velocity.TossForce(160).Y, "toss vertical direction");
            for (int t = 168; t <= 280; t += 8) velocity.Add(160, -160, t);
            Near(0, velocity.TossForce(280).Length(), "holding still before release stops toss");
            Near(0, velocity.TossForce(500).Length(), "stale samples do not throw pet");
        }
        Console.WriteLine("PASS " + checks + " motion and drag checks");
    }
}
