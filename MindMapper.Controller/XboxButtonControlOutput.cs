using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;


namespace MindMapper.Controller;

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
        Console.WriteLine($"Setting button {button.Name} to {value}");
        if (inverted) value = !value;
        this.controller.SetButtonState(button, value);
    }
    
    public override void Neutralize()
    {
        this.controller.SetButtonState(button, false);
    }
    
    
}