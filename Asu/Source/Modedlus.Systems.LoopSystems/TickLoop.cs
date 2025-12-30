namespace Modedlus.Systems.LoopSystems;

public class TickLoop : Loop
{
    public event EventHandler<LoopEventArgs> OnLoop;

    public float TickRate { get { return 1.0f / TickInterval; } set { TickInterval = 1.0f / value; } }
    public float TickInterval = 0.02f;

    private float CurentInterval = 0.0f;

    private DateTime LastTickTime;

    public TickLoop()
    {
        LastTickTime = DateTime.Now;
    }

    public void Tick()
    {
        // Update Interval If it change outside
        CurentInterval = TickInterval;

        DateTime CurrentTime = DateTime.Now;
        float DeltaTime = (float)(CurrentTime - LastTickTime).TotalSeconds;
        LastTickTime = CurrentTime;

        OnLoop?.Invoke(this, new LoopEventArgs { DeltaTime = DeltaTime });
    }
}