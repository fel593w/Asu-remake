using Modedlus.Systems.LoopSystems;

namespace Silk.Net.Interface.OpenGl;

internal class RenderLoop : Loop
{
    public event EventHandler<LoopEventArgs> OnLoop;

    public void Invoke(LoopEventArgs _args)
    {
        OnLoop?.Invoke(null, _args);
    }
}