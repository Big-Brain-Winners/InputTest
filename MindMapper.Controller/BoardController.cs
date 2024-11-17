﻿using brainflow;
using brainflow.math;
using MindMapper.Common;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace MindMapper.Controller;

public class BoardController
{
    private readonly Config _config;
    private readonly BrainFlowInputParams _inputParams;


    public BoardController()
    {
        var appRoot = AppDomain.CurrentDomain.BaseDirectory;
        var configPath = Path.Combine(appRoot, "config.json");
        _config = Config.Load(configPath);
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

        Console.WriteLine("starting loop");

        List<Channel> channelHandlers = [];
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
                channelControlOutput = new XboxButtonControlOutput(availableButtons[binding.ControlIndex], controller,
                    binding.Inverted);
            }
            else if (binding.ControlType == ControlTypeCode.axis)
            {
                Console.WriteLine("Adding an axis");
                channelControlOutput =
                    new XboxAxisControlOutput(availableAxies[binding.ControlIndex], controller, binding.Inverted);
                Console.WriteLine("routing output to " + availableAxies[binding.ControlIndex].Name);
                Console.WriteLine("Inverted? " + binding.Inverted);
                
            }
            else if (binding.ControlType == ControlTypeCode.slider)
            {
                channelControlOutput = new XboxSliderControlOutput(availableSliders[binding.ControlIndex], controller,
                    binding.Inverted);
            }
            else
            {
                throw new Exception($"Unknown control type {binding.ControlType}");
            }
            
            
            
            channelHandlers.Add(new Channel(i, _config, binding.ChannelType, channelControlOutput, !binding.Analog));
            Console.WriteLine($"Channel #{i}: {binding.ChannelType} {binding.Analog}");
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
                    case ConsoleKey.E:
                        pollingTime += 10;
                        break;
                    case ConsoleKey.D:
                        if (pollingTime > 10)
                            pollingTime -= 10;
                        break;
                }
            }

            Console.WriteLine($"PollingTime: {pollingTime}");

            foreach (var channelHandler in channelHandlers)
            {
                channelHandler.handleData(unprocessedData);
            }

            controller.SubmitReport();
        }
    }
}