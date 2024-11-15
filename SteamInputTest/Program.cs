using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

class Program
{
    static void Main(string[] args)
    {
        var client = new ViGEmClient();

        var ds4 = client.CreateDualShock4Controller();

        ds4.Connect();
        Console.WriteLine("Connected to DS4");
        
        Task.Run(() => MonitorOutputReports(ds4));

        while (true)
        {
            Console.WriteLine("Press any key to press Cross");
            var input = Console.ReadKey(intercept: true).Key;

            if (input == ConsoleKey.Escape)
            {
                break;
            }

            if (input == ConsoleKey.Enter)
            {
                Thread.Sleep(2000);
                ds4.SetButtonState(DualShock4Button.Cross, true);
                ds4.SubmitReport();
                Console.WriteLine("Cross pressed");

                
            }

            if (input == ConsoleKey.LeftArrow)
            {
                ds4.SetAxisValue(DualShock4Axis.LeftThumbX, 0);
                ds4.SubmitReport();
                
                Thread.Sleep(2000);
                
                ds4.SetAxisValue(DualShock4Axis.LeftThumbX, 128);
                ds4.SubmitReport();
            }
        }

        ds4.Disconnect();
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
