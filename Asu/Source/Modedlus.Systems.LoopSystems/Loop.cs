namespace Modedlus.Systems.LoopSystems;

public interface Loop
{
    public EventHandler<LoopEventArgs> OnLoop { get; set; }
}

public class LoopEventArgs : EventArgs
{
    public float DeltaTime { get; set; }
}