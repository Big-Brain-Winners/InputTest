using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;


namespace SteamInputTest;

public class XboxButtonControlOutput : XboxControlOutput
{
    private Xbox360Button button;

    public XboxButtonControlOutput(Xbox360Button button, IXbox360Controller controller, bool inverted) : base(
        controller, inverted)
    {
        this.button = button;
    }

    public override void SendBinarySignal(bool value)
    {
        if (inverted) value = !value;
        this.controller.SetButtonState(button, value);
    }
}