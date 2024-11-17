using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

public class XboxAxisControlOutput : XboxControlOutput
{
    private Xbox360Axis axis;

    public XboxAxisControlOutput(Xbox360Axis axis, IXbox360Controller controller, bool inverted) : base(controller,
        inverted)
    {
        this.axis = axis;
    }

    public override void SendAnalogSignal(double value, double min, double max)
    {
        double clampedVal = Math.Clamp(value, min, max);
        double transformedAnalogValue = (clampedVal - min) / (max - min);
        Console.WriteLine($"Original Value {value}, clamped Value {clampedVal}, min: {min}, max: {max}, transformed: {transformedAnalogValue}");
        short finalAxisValue = Convert.ToInt16((transformedAnalogValue * 32767 * 2) - 32767);
        if (inverted)
        {
            finalAxisValue *= -1;
        }
        this.controller.SetAxisValue(axis, finalAxisValue);
    }

    public override void SendBinarySignal(bool value)
    {
        if (inverted) value = !value;
        if (value)
        {
            this.controller.SetAxisValue(axis, Convert.ToInt16(32767));
        }
        else
        {
            this.controller.SetAxisValue(axis, Convert.ToInt16(0));
        }
    }
}