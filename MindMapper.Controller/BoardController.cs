using System.ComponentModel;
using brainflow;
using brainflow.math;
using MindMapper.Common;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MindMapper.Controller;

public class BoardController
{
    private Config _config;
    private readonly BrainFlowInputParams _inputParams;
    private List<Channel> channelHandlers = new List<Channel>();
    private List<String> configFiles = new List<String>();
    private String currentConfigFileName = "cconfig.json";
    private int currentFileIndex = 0;
    private ViGEmClient _contClient;
    private IXbox360Controller _controller;


    public BoardController()
    {
        RefreshConfigFiles();
        var appRoot = AppDomain.CurrentDomain.BaseDirectory;
        var configPath = Path.Combine(appRoot, currentConfigFileName);
        _config = Config.Load(configPath);
        _inputParams = new BrainFlowInputParams()
        {
            serial_port = _config.BrainflowSettings.SerialPort,
            mac_address = _config.BrainflowSettings.MacAddress
        };

        var client = new ViGEmClient();
        _controller = client.CreateXbox360Controller();
    }

    public void Start()
    {
        Console.Write($"using com port ${_config.BrainflowSettings.SerialPort}\n");
        BoardShim.enable_dev_board_logger();
        BoardShim boardShim = new BoardShim(_config.BrainflowSettings.BoardId, _inputParams);

        boardShim.prepare_session();

        _controller.Connect();

        // Enable accelerometer
        boardShim.config_board("n");

        boardShim.start_stream();

        //interrupt handler, makes program give up resources when closed with ctrl C
        Console.CancelKeyPress += delegate
        {
            _controller.Disconnect();
            _contClient.Dispose();
            boardShim.stop_stream();
            boardShim.release_session();
        };

        BoardControlLoop(boardShim);
    }

    void BoardControlLoop(BoardShim boardShim)
    {
        int pollingTime = _config.AdjustmentSettings.PollingTime;

        Console.WriteLine("starting loop");
        BuildHandlers();

        Console.Clear();
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
                        pollingTime += 10;
                        break;
                    case ConsoleKey.S:
                        if (pollingTime > 10)
                            pollingTime -= 10;
                        break;

                    case ConsoleKey.R:
                        FullConfigRefresh();
                        break;
                    
                    case ConsoleKey.A:
                        DecrementConfigFileIndex();
                        break;
                    
                    case ConsoleKey.D:
                        IncrementConfigFileIndex();
                        break;
                }
            }
            Console.SetCursorPosition(0, 0);
            Console.WriteLine($"PollingTime: {pollingTime}");
            String configFileListString = "[ \"" + String.Join("\" , \"", configFiles.ToArray()) + "\" ]";
            Console.WriteLine($"Detected Configurations: {configFileListString}");
            Console.WriteLine($"Current Configuration: {currentConfigFileName}");
            foreach (var channelHandler in channelHandlers)
            {
                channelHandler.handleData(unprocessedData);
            }

            _controller.SubmitReport();
        }
    }

    public void FullConfigRefresh()
    {
        RefreshConfigFiles();
        UpdateHandlers();
    }

    public void UpdateHandlers()
    {
        _config = Config.Load(currentConfigFileName);
        ClearHandlers();
        BuildHandlers();
    }

    public void BuildHandlers()
    {
        Xbox360Button[] availableButtons =
        [
            Xbox360Button.A, Xbox360Button.X, Xbox360Button.Y, Xbox360Button.B, Xbox360Button.Left, Xbox360Button.Right,
            Xbox360Button.Up, Xbox360Button.Down, Xbox360Button.Back, Xbox360Button.Start, Xbox360Button.LeftShoulder,
            Xbox360Button.RightShoulder, Xbox360Button.LeftThumb, Xbox360Button.RightThumb, Xbox360Button.Guide
        ];
        Xbox360Axis[] availableAxies =
        [
            Xbox360Axis.LeftThumbX, Xbox360Axis.LeftThumbY, Xbox360Axis.RightThumbX, Xbox360Axis.RightThumbY
        ];
        Xbox360Slider[] availableSliders = [Xbox360Slider.LeftTrigger, Xbox360Slider.RightTrigger];


        for (int i = 0; i < _config.Bindings.Count; i++)
        {
            var binding = _config.Bindings[i];
            if (binding.ControlType == ControlTypeCode.none) continue;
            if (binding.ChannelType == ChannelType.Null) throw new ArgumentNullException(nameof(binding.ChannelType));

            ControlOutput channelControlOutput;

            if (binding.ControlType == ControlTypeCode.button)
            {
                channelControlOutput = new XboxButtonControlOutput(availableButtons[binding.ControlIndex], _controller,
                    binding.Inverted);
            }
            else if (binding.ControlType == ControlTypeCode.axis)
            {
                Console.WriteLine("Adding an axis");
                channelControlOutput =
                    new XboxAxisControlOutput(availableAxies[binding.ControlIndex], _controller, binding.Inverted);
                Console.WriteLine("routing output to " + availableAxies[binding.ControlIndex].Name);
                Console.WriteLine("Inverted? " + binding.Inverted);
            }
            else if (binding.ControlType == ControlTypeCode.slider)
            {
                channelControlOutput = new XboxSliderControlOutput(availableSliders[binding.ControlIndex], _controller,
                    binding.Inverted);
            }
            else
            {
                throw new Exception($"Unknown control type {binding.ControlType}");
            }

            channelHandlers.Add(new Channel(i, _config, binding.ChannelType, channelControlOutput, !binding.Analog));
            Console.WriteLine($"Channel #{i}: {binding.ChannelType} {binding.Analog}");
        }
    }

    public void ClearHandlers()
    {
        foreach (var channelHandler in channelHandlers)
        {
            channelHandler.Neutralize();
        }

        channelHandlers.Clear();
    }

    public void RefreshConfigFiles()
    {
        configFiles.Clear();
        String[] directoryFiles = Directory.GetFiles(".");
        foreach (var file in directoryFiles)
        {
            String justFileName = Path.GetFileName(file);
            String lowerFileName = justFileName.ToLower();
            if (lowerFileName.EndsWith(".json") && lowerFileName.Contains("cconfig")) configFiles.Add(justFileName);
        }

        configFiles.Sort((s, s1) => s.CompareTo(s1));

        if (configFiles.Count == 0) throw new FileNotFoundException("Could not find config file");

        // if current file has moved, try to find it, default to first existing config if unfindable
        if (currentFileIndex >= configFiles.Count || configFiles[currentFileIndex] != currentConfigFileName)
        {
            int foundIdx = configFiles.FindIndex((s) => s.Equals(currentConfigFileName));

            if (foundIdx == -1)
            {
                foundIdx = 0;
            }

            currentConfigFileName = configFiles[foundIdx];
            currentFileIndex = foundIdx;
        }
    }


    private void IncrementConfigFileIndex()
    {
        RefreshConfigFiles();
        currentFileIndex = (currentFileIndex + 1) % configFiles.Count;
        currentConfigFileName = configFiles[currentFileIndex];
        UpdateHandlers();
    }

    private void DecrementConfigFileIndex()
    {
        RefreshConfigFiles();
        currentFileIndex = (currentFileIndex - 1);
        if (currentFileIndex < 0) currentFileIndex = configFiles.Count - 1;
        currentConfigFileName = configFiles[currentFileIndex];
        UpdateHandlers();
    }
}