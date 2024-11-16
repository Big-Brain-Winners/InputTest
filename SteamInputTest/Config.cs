﻿using System.Text.Json;
using System.Text.Json.Serialization;

namespace SteamInputTest;



public class Config
{
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

public class BrainflowSettings
{
    [JsonPropertyName("BoardId")]
    public int BoardId { get; set; }

    [JsonPropertyName("SerialPort")]
    public string SerialPort { get; set; }

    [JsonPropertyName("MacAddress")]
    public string MacAddress { get; set; }
}

public class AdjustmentSettings
{
    [JsonPropertyName("PollingTime")]
    public int PollingTime { get; set; }

    [JsonPropertyName("Threshold")]
    public int Threshold { get; set; }

    [JsonPropertyName("RollingAvgSize")]
    public int RollingAvgSize { get; set; }

    [JsonPropertyName("EmgChannels")]
    public int EmgChannels { get; set; }

    [JsonPropertyName("BaseOffsets")]
    public List<double> BaseOffsets { get; set; }
}