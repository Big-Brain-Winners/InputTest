using brainflow;
using brainflow.math;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

class Program
{
    private static readonly int _boardId = (int)BoardIds.GANGLION_BOARD;

    private static readonly BrainFlowInputParams _inputParams = new BrainFlowInputParams
    {
        serial_port = "COM10"
    };

    static void Main(string[] args)
    {
        BoardShim.enable_dev_board_logger();

        BoardShim boardShim = new BoardShim(_boardId, _inputParams);


        var client = new ViGEmClient();
        var controller = client.CreateXbox360Controller();

        //interrupt handler, makes program give up resources when closed with ctrl C
        Console.CancelKeyPress += delegate {
            controller.Disconnect();
            client.Dispose();
            boardShim.stop_stream();
            boardShim.release_session();
        };
        
        controller.Connect();
        Console.WriteLine("Xbox 360 controller connected");

        boardShim.prepare_session();

        // Enable accelerometer
        boardShim.config_board("n");

        boardShim.start_stream();

        BoardControlLoop(boardShim, controller);

        //Thread.Sleep(5000);
    }

    static void BoardControlLoop(BoardShim boardShim, IXbox360Controller controller)
    {
        while (true)
        {
            Thread.Sleep(100);
            double[,] unprocessedData = boardShim.get_current_board_data(1);
            int[] emgChannels = BoardShim.get_emg_channels(_boardId);
            int[] accel_channels = BoardShim.get_accel_channels(_boardId);

            Xbox360Button[] emg_buttons = [Xbox360Button.A, Xbox360Button.B, Xbox360Button.X, Xbox360Button.Y];
            int buttonIndex = 0;
            int threshold = 500;
            foreach (var index in emgChannels)
            {
                var row = unprocessedData.GetRow(index);
                controller.SetButtonState(emg_buttons[buttonIndex], Math.Abs(row[0]) > threshold);
                Console.WriteLine($"Button {buttonIndex}: {row[0]} {Math.Abs(row[0]) > threshold}");
                buttonIndex++;
                // controller.SubmitReport();
            }

            int gyroIndex = 0;
            Xbox360Axis[] gyroAxies = [Xbox360Axis.LeftThumbX, Xbox360Axis.LeftThumbY];
            foreach (var index in accel_channels)
            {
                if (gyroIndex < gyroAxies.Length)
                {
                    var row = unprocessedData.GetRow(index);
                    double gyroValue = row[0];
                    gyroValue = Math.Clamp(gyroValue, -1, 1);
                    gyroValue = gyroValue * (32767.0);
                    short finalGyroValue = Convert.ToInt16(gyroValue);
                    controller.SetAxisValue(gyroAxies[gyroIndex], finalGyroValue);
                    Console.WriteLine($"Gyro Value(index {gyroIndex}: {gyroValue}");
                    // controller.SubmitReport();
                }

                gyroIndex++;
            }

            controller.SubmitReport();
        }
    }


    static void MonitorOutputReports(IDualShock4Controller ds4)
    {
        while (true)
        {
            try
            {
                // blocks for 250ms to not burn CPU cycles if no report is available
                var buffer = ds4.AwaitRawOutputReport(250, out var timedOut);

                if (timedOut)
                {
                    // Console.WriteLine("Timed out");
                    continue;
                }

                // you got a new report, parse it and do whatever you need to do :)
                // here we simply hex-dump the contents
                //Console.WriteLine($"[OUT] {string.Join(" ", buffer.Select(b => b.ToString("X2")))}");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                Thread.Sleep(1000);
            }
        }
    }
}
