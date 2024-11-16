using brainflow;
using brainflow.math;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.DualShock4;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace SteamInputTest;

class Program
{
    
    static void Main(string[] args)
    {
        BoardController controller = new();
        
        controller.Start();
    }
    
}