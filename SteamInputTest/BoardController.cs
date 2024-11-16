using brainflow;
using brainflow.math;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

public class BoardController
{
    private readonly Config _config;
    private readonly BrainFlowInputParams _inputParams;
    
    
    public BoardController()
    {
        _config = Config.Load("config.json");
        _inputParams = new BrainFlowInputParams()
        {
            serial_port = _config.BrainflowSettings.SerialPort,
            mac_address = _config.BrainflowSettings.MacAddress
        };
    }

    public void Start()
    {
        BoardShim.enable_dev_board_logger();
        BoardShim boardShim = new BoardShim(_config.BrainflowSettings.BoardId, _inputParams);
        
        boardShim.prepare_session();
        
        var client = new ViGEmClient();
        var controller = client.CreateXbox360Controller();
        
        // Enable accelerometer
        boardShim.config_board("n");
        
        boardShim.start_stream();
        
        //interrupt handler, makes program give up resources when closed with ctrl C
        Console.CancelKeyPress += delegate
        {
            controller.Disconnect();
            client.Dispose();
            boardShim.stop_stream();
            boardShim.release_session();
        };
        
        BoardControlLoop(boardShim, controller);
    }
    
    void BoardControlLoop(BoardShim boardShim, IXbox360Controller controller)
    {
        int pollingTime = _config.AdjustmentSettings.PollingTime;
        int threshold = _config.AdjustmentSettings.Threshold;
        int rollingAvgSize = _config.AdjustmentSettings.RollingAvgSize;
        int rollingAvgHead = _config.AdjustmentSettings.RollingAvgHead;
        int emg_channel_count = _config.AdjustmentSettings.EmgChannels;
        List<double> offsets = _config.AdjustmentSettings.BaseOffsets;
        List<List<double>> rolling = new();

        for (int i = 0; i < emg_channel_count; i++)
        {
            rolling.Add(new List<double>());
        }


        while (true)
        {
            Thread.Sleep(pollingTime);
            double[,] unprocessedData = boardShim.get_current_board_data(10);
            int[] emgChannels = BoardShim.get_emg_channels(_config.BrainflowSettings.BoardId);
            int[] accel_channels = BoardShim.get_accel_channels(_config.BrainflowSettings.BoardId);

            //config control check
            if (Console.KeyAvailable)
            {
                var keypressed = Console.ReadKey().Key;
                switch (keypressed)
                {
                    case ConsoleKey.W:
                        threshold += 50;
                        break;
                    case ConsoleKey.S:
                        threshold -= 50;
                        break;
                    case ConsoleKey.Q:
                        rollingAvgSize += 1;
                        break;
                    case ConsoleKey.A:
                        if (rollingAvgSize > 1)
                            rollingAvgSize -= 1;
                        break;
                    case ConsoleKey.E:
                        pollingTime += 10;
                        break;
                    case ConsoleKey.D:
                        if (pollingTime > 10)
                            pollingTime -= 10;
                        break;
                    case ConsoleKey.R:
                        offsets[0] += 0.05;
                        break;
                    case ConsoleKey.F:
                        offsets[0] -= 0.05;
                        break;
                    case ConsoleKey.T:
                        offsets[1] += 0.05;
                        break;
                    case ConsoleKey.G:
                        offsets[1] -= 0.05;
                        break;
                }
            }

            Console.WriteLine($"Threshold: {threshold}");
            Console.WriteLine($"Rolling Avg Size: {rollingAvgSize}");
            Console.WriteLine($"PollingTIme: {pollingTime}");
            Console.WriteLine($"XOffset: {offsets[0]}");
            Console.WriteLine($"YOffset: {offsets[1]}");


            Xbox360Button[] emg_buttons = [Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y];
            int buttonIndex = 0;
            foreach (var index in emgChannels)
            {
                // get data point for current time, add to rolling window
                var row = unprocessedData.GetRow(index);
                double pointRunningSum = 0;
                for (int dataIndex = 0; dataIndex < row.Length; dataIndex++)
                {
                    pointRunningSum += Math.Abs(row[dataIndex]);
                }

                var pointAvg = pointRunningSum / row.Length;
                rolling[buttonIndex].Insert(rollingAvgHead, pointAvg);

                // get rolling average
                double rollingAvgSum = 0;
                for (int i = 0; i < rollingAvgSize; i++)
                {
                    if (i >= rolling[buttonIndex].Count)
                    {
                        rollingAvgSum += 0;
                    }
                    else
                    {
                        rollingAvgSum += rolling[buttonIndex][i];
                    }
                }

                double rollingAvg = rollingAvgSum / rollingAvgSize;


                controller.SetButtonState(emg_buttons[buttonIndex], rollingAvg > threshold);
                Console.WriteLine($"Button {buttonIndex}: {rollingAvg} {rollingAvg > threshold}");
                buttonIndex++;
                // controller.SubmitReport();
            }

            rollingAvgHead = (rollingAvgHead + 1) % rollingAvgSize;

            int gyroIndex = 0;
            Xbox360Axis?[] gyroAxies = [Xbox360Axis.LeftThumbX, null, Xbox360Axis.LeftThumbY];
            foreach (var index in accel_channels)
            {
                if (gyroIndex < gyroAxies.Length)
                {
                    const int signed16IntMax = 32767;
                    var row = unprocessedData.GetRow(index);
                    var avgVal = row.Average();
                    double gyroValue = avgVal;
                    gyroValue = Math.Clamp(gyroValue, -1, 1);
                    if (gyroIndex == 0)
                        gyroValue = gyroValue * -1;
                    // gyroValue += 1; // offset to 0, 2
                    // gyroValue += offsets[gyroIndex]; // add offset
                    // gyroValue = gyroValue % 2; // wrap around
                    gyroValue = gyroValue * (signed16IntMax); // convert into range between 0 and 2x 16 bit limit
                    // gyroValue = gyroValue - signed16IntMax; // center back around 0;
                    short finalGyroValue = Convert.ToInt16(gyroValue);
                    if (gyroAxies[gyroIndex] != null)
                    {
                        controller.SetAxisValue(gyroAxies[gyroIndex], finalGyroValue);
                        Console.WriteLine($"Gyro Value(index {gyroIndex}: {gyroValue}");
                    }
                    
                    
                    // controller.SubmitReport();
                }

                gyroIndex++;
            }

            controller.SubmitReport();
        }
    }
}