namespace Modedlus.Systems.LoopSystems;

public interface Loop
{
    public event EventHandler<LoopEventArgs> OnLoop;
}

public class LoopEventArgs : EventArgs
{
    public float DeltaTime { get; set; }
}