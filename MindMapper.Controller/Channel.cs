using brainflow.math;
using MindMapper.Common;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MindMapper.Controller;



class Channel
{
    private readonly int _index;
    private readonly ControlOutput _controlOutput;
    private readonly Config _config;
    private readonly int _rollingWindowHeadIdx;
    private readonly List<double> _rollingAvgWindow;
    private readonly bool _binary; // if off outputs processed analog values
    private readonly int[] _inputRange = new int[2]; //[lowend, highend]
    private bool _absoluteInput;


    public Channel(int channelIndex, Config config, ChannelType channelType, ControlOutput controlOutput, bool binary)
    {
        _index = channelIndex;
        this._config = config;
        this._controlOutput = controlOutput;
        _binary = binary;
        _rollingWindowHeadIdx = 0;
        _rollingAvgWindow = new List<double>();

        if (channelType == ChannelType.Emg)
        {
            _inputRange[0] = 0;
            _inputRange[1] = 5000; //todo make high end configurable
            _absoluteInput = true;
        }
        else if (channelType == ChannelType.Accelerometer)
        {
            _inputRange[0] = -1;
            _inputRange[1] = 1;
            _absoluteInput = false;
        }
        else
        {
            throw new ArgumentException("This channel type is not supported at this time.");
        }
    }

    public void handleData(double[,] unprocessedData)
    {
// get data point for current time, add to rolling window
        var row = unprocessedData.GetRow(_index);
        double pointRunningSum = 0;
        for (int dataIndex = 0; dataIndex < row.Length; dataIndex++)
        {
            if (_absoluteInput)
            {
                pointRunningSum += Math.Abs(row[dataIndex]);
            }
            else
            {
                pointRunningSum += row[dataIndex];
            }
        }

        var pointAvg = pointRunningSum / row.Length;
        _rollingAvgWindow.Insert(_rollingWindowHeadIdx, pointAvg);

        // get rolling average
        double rollingAvgSum = 0;
        for (int i = 0; i < _config.Bindings[_index].RollingAvgSize; i++)
        {
            if (i >= _rollingAvgWindow.Count)
            {
                rollingAvgSum += 0;
            }
            else
            {
                rollingAvgSum += _rollingAvgWindow[i];
            }
        }

        double rollingAvg = rollingAvgSum / _config.Bindings[_index].RollingAvgSize;
        Console.Write($"Channel {_index}: {rollingAvg.ToString("N2")}.  ");

        if (_binary)
        {
            bool binaryVal = rollingAvg > _config.Bindings[_index].Threshold; 
            _controlOutput.SendBinarySignal(binaryVal);
        }
        else
        {
            _controlOutput.SendAnalogSignal(rollingAvg, _inputRange[0], _inputRange[1]);
        }
        
    }

    public void Neutralize()
    {
        _controlOutput.Neutralize();
    }
    
    
}