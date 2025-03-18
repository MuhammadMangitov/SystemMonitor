using Newtonsoft.Json;

public class AdapterDetails
{
    [JsonProperty("nic_name")]
    public string NicName { get; set; }

    [JsonProperty("ip_address")]
    public string IpAddress { get; set; }

    [JsonProperty("mac_address")]
    public string MacAddress { get; set; }

    [JsonProperty("available")]
    public string Available { get; set; }
}
