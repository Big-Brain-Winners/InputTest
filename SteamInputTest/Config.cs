using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamInputTest;

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
}

public enum controlTypeCode
{
    none = -1,
    button = 0,
    axis = 1,
    slider = 2,
}


public class BindingSettings
{
    [JsonPropertyName("ControlType")] public controlTypeCode ControlType { get; set; }
    [JsonPropertyName("ChannelType")] public channelType ChannelType { get; set; } = channelType.Null;

    [JsonPropertyName("ControlIndex")] public int ControlIndex { get; set; } = 0;

    [JsonPropertyName("Inverted")] public bool Inverted { get; set; } = false;

    [JsonPropertyName("Analog")] public bool Analog { get; set; } = false;

    [JsonPropertyName("threshold")] public int? Threshold { get; set; }

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

    [JsonPropertyName("Threshold")] public int Threshold { get; set; }

    [JsonPropertyName("RollingAvgSize")] public int RollingAvgSize { get; set; }

}