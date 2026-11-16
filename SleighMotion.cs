namespace DesktopSleigh;

internal sealed class SleighMotion
{
    public const double WaveSeconds = 1.5;
    public const double TravelSeconds = 30;

    private double waveRemaining;
    private double travelRemaining = TravelSeconds;

    public double Distance { get; set; }
    public double WalkingDistance { get; private set; }
    public bool IsMoving { get; private set; }
    public bool IsWaving => waveRemaining > 0;

    public void BeginWave()
    {
        waveRemaining = WaveSeconds;
        travelRemaining = TravelSeconds;
        IsMoving = false;
    }

    public bool Tick(double elapsed, bool paused, double speed)
    {
        IsMoving = false;
        if (paused)
        {
            return false;
        }

        if (IsWaving)
        {
            waveRemaining = Math.Max(0, waveRemaining - elapsed);
            return false;
        }

        travelRemaining -= elapsed;
        if (travelRemaining <= 0)
        {
            BeginWave();
            return true;
        }

        double travel = speed * elapsed;
        Distance += travel;
        WalkingDistance += travel;
        IsMoving = travel > 0;
        return false;
    }
}
