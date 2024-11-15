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
        var client = new ViGEmClient();
        var controller = client.CreateXbox360Controller();
        
        controller.Connect();
        Console.WriteLine("Xbox 360 controller connected");
        
        BoardShim.enable_dev_board_logger();
        
        BoardShim boardShim = new BoardShim(_boardId, _inputParams);
        
        BoardControlLoop(boardShim);
    }

    static void BoardControlLoop(BoardShim boardShim)
    {
        
        boardShim.prepare_session();
        
        // Enable accelerometer
        boardShim.config_board("n");
        
        boardShim.start_stream();
        
        Thread.Sleep(5000);
        
        double[,] unprocessedData = boardShim.get_current_board_data(1);
        int[] eeg_channels = BoardShim.get_eeg_channels(_boardId);
        int[] accel_channels = BoardShim.get_accel_channels(_boardId);

        foreach (var index in eeg_channels)
        {
            var row = unprocessedData.GetRow(index);
            
            
            // if (row[index] == match_action_value)
            // {
            //     controller.SetButtonState(Xbox360Button.A, true);
            //     controller.SubmitReport();
            // }
            
            
            
        }

        foreach (var index in accel_channels)
        {
            // SetAxisValue - 0 is left, 128 is middle, 255 is right
            // if (row[index] == match_move_value)
            // {
            //     controller.SetAxisValue(Xbox360Axis.LeftThumbX, 0);
            //     controller.SubmitReport();
            // }
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
