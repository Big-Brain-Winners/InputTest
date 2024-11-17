namespace MindMapper.Controller;

public abstract class ControlOutput
{
    protected bool inverted;

    protected ControlOutput(bool inverted)
    {
        this.inverted = inverted;
    }
    public virtual void SendAnalogSignal(double value, double min, double max)
    {
        throw new NotImplementedException("This control type does not support analog signals.");
    }

    public virtual void SendBinarySignal(bool value)
    {
        throw new NotImplementedException("This control type does not support binary signals.");
    }
    // sets any outputs to 0
    public virtual void Neutralize()
    {
        throw new NotImplementedException("This control type does not support controls.");
    }
}