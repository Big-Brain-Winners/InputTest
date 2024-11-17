using System.Text.Json;
using System.Text.Json.Serialization;

namespace MindMapper.Common;

public class Config
{
    [JsonPropertyName("Bindings")] public List<BindingSettings> Bindings { get; set; }

    [JsonPropertyName("BrainflowSettings")]
    public BrainflowSettings BrainflowSettings { get; set; }

    [JsonPropertyName("AdjustmentSettings")]
    public AdjustmentSettings AdjustmentSettings { get; set; }

    public static Config Load(string filePath)
    {
        var file = System.IO.File.ReadAllText(filePath);
        var config = JsonSerializer.Deserialize<Config>(file);

        if (config is null)
        {
            throw new ApplicationException("Failed to load or parse the configuration file.");
        }

        return config;
    }

    public static void CopyToAppData()
    {
        var path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "/MindMapper";
        var file = path + "/config.json";
        
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        
        if (!File.Exists(file))
        {
            var conf = Config.Load("config.json");
            var json = JsonSerializer.Serialize(conf);
            File.WriteAllText(file, json);
        }
    }
}




public class BindingSettings
{
    [JsonPropertyName("ControlType")] public ControlTypeCode ControlType { get; set; }
    [JsonPropertyName("ChannelType")] public ChannelType ChannelType { get; set; } = ChannelType.Null;

    [JsonPropertyName("ControlIndex")] public int ControlIndex { get; set; } = 0;

    [JsonPropertyName("Inverted")] public bool Inverted { get; set; } = false;

    [JsonPropertyName("Analog")] public bool Analog { get; set; } = false;

    [JsonPropertyName("Threshold")] public int? Threshold { get; set; }
    
    [JsonPropertyName("RollingAvgSize")] public int RollingAvgSize { get; set; } = 1;

    [JsonPropertyName("BaseOffsets")] public double Offsets { get; set; } = 0.0; //todo reimplement offsets on analog signals
}

public class BrainflowSettings
{
    [JsonPropertyName("BoardId")] public int BoardId { get; set; }

    [JsonPropertyName("SerialPort")] public string SerialPort { get; set; }

    [JsonPropertyName("MacAddress")] public string MacAddress { get; set; }
}

public class AdjustmentSettings
{
    [JsonPropertyName("PollingTime")] public int PollingTime { get; set; }
    
}