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
        Console.Write($"using com port ${_config.BrainflowSettings.SerialPort}\n");
        BoardShim.enable_dev_board_logger();
        BoardShim boardShim = new BoardShim(_config.BrainflowSettings.BoardId, _inputParams);

        boardShim.prepare_session();

        var client = new ViGEmClient();
        var controller = client.CreateXbox360Controller();
        controller.Connect();

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
        int emg_channel_count = _config.AdjustmentSettings.EmgChannels;
        List<double> offsets = _config.AdjustmentSettings.BaseOffsets;
        

        Console.WriteLine("starting loop");

        List<Channel> channelHandlers = [];
        Xbox360Button[] emg_buttons = [Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y];
        int[] emgChannels = BoardShim.get_emg_channels(_config.BrainflowSettings.BoardId);
        
        for (int i = 0; i < emg_channel_count; i++)
        {
            ControlOutput channelControlOutput = new XboxButtonControlOutput(emg_buttons[i], controller, false);
            channelHandlers.Add(new Channel(emgChannels[i], _config, channelType.Emg, channelControlOutput, true));
        }
        
        Xbox360Axis?[] gyroAxies = [Xbox360Axis.LeftThumbX, null, Xbox360Axis.LeftThumbY];
        int[] accel_channels = BoardShim.get_accel_channels(_config.BrainflowSettings.BoardId);
        for (int i = 0; i < accel_channels.Count(); i++)
        {
            if (gyroAxies[i] == null) continue;
            bool inverted = i == 0; //need to invert x axis
            ControlOutput channelControlOutput = new XboxAxisControlOutput(gyroAxies[i], controller, inverted);
            channelHandlers.Add(
                new Channel(accel_channels[i], _config, channelType.Accelerometer, channelControlOutput, false));
        }

        while (true)
        {
            Thread.Sleep(pollingTime);
            double[,] unprocessedData = boardShim.get_current_board_data(10);

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

            foreach (var channelHandler in channelHandlers)
            {
                channelHandler.handleData(unprocessedData);
            }

            controller.SubmitReport();
        }
    }
}