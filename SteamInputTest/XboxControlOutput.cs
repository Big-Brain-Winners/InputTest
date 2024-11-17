using Nefarius.ViGEm.Client.Targets;

namespace SteamInputTest;

public abstract class XboxControlOutput : ControlOutput
{
    protected IXbox360Controller controller;

    public XboxControlOutput(IXbox360Controller controller, bool inverted) : base(inverted)
    {
        this.controller = controller;
    }
}