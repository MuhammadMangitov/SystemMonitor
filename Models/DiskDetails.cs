using Newtonsoft.Json;

public class DiskDetails
{
    [JsonProperty("drive_name")]
    public string DriveName { get; set; }

    [JsonProperty("total_size")]
    public long TotalSize { get; set; }

    [JsonProperty("free_size")]
    public long AvailableSpace { get; set; }
}