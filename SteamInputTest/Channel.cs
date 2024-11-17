using brainflow.math;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

enum channelType
{
    Emg,
    Accelerometer
}

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


    public Channel(int channelIndex, Config config, channelType channelType, ControlOutput controlOutput, bool binary)
    {
        _index = channelIndex;
        this._config = config;
        this._controlOutput = controlOutput;
        _binary = binary;
        _rollingWindowHeadIdx = 0;
        _rollingAvgWindow = new List<double>();

        if (channelType == channelType.Emg)
        {
            _inputRange[0] = 0;
            _inputRange[1] = 5000; //todo make high end configurable
            _absoluteInput = true;
        }
        else if (channelType == channelType.Accelerometer)
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
        for (int i = 0; i < _config.AdjustmentSettings.RollingAvgSize; i++) //todo make this a per-channel config
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

        double rollingAvg = rollingAvgSum / _config.AdjustmentSettings.RollingAvgSize; //todo make this a per-channel config


        if (_binary)
        {
            bool binaryVal = rollingAvg > _config.AdjustmentSettings.Threshold; //todo make the threshold a per-channel config
            _controlOutput.SendBinarySignal(binaryVal);
        }
        else
        {
            _controlOutput.SendAnalogSignal(rollingAvg, _inputRange[0], _inputRange[1]);
        }
        
    }
}