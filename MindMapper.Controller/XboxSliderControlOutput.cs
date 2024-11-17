using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MindMapper.Controller;

public class XboxSliderControlOutput : XboxControlOutput
{
    private Xbox360Slider slider;

    public XboxSliderControlOutput(Xbox360Slider slider, IXbox360Controller controller, bool inverted) : base(
        controller, inverted)
    {
        this.slider = slider;
    }

    public override void SendAnalogSignal(double value, double min, double max)
    {
        double clampedVal = Math.Clamp(value, min, max);
        double transformedAnalogValue = (clampedVal - min) / (max - min);
        if (inverted) transformedAnalogValue = 1 - transformedAnalogValue;
        byte finalSliderValue = Convert.ToByte(transformedAnalogValue * 255);
        this.controller.SetSliderValue(slider, finalSliderValue);
    }

    public override void SendBinarySignal(bool value)
    {
        if (inverted) value = !value;
        if (value)
        {
            this.controller.SetSliderValue(slider, Convert.ToByte(255));
        }
        else
        {
            this.controller.SetSliderValue(slider, Convert.ToByte(0));
        }
    }
    
    public override void Neutralize()
    {
        this.controller.SetSliderValue(slider, Convert.ToByte(0));
    }
}