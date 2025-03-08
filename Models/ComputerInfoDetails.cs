using System.ComponentModel;

namespace SystemMonitor.Models
{
    public class ComputerInfoDetails
    {
        public string username { get; set; }
        public string computer_name { get; set; }
        public string mac_address { get; set; }
        public long ram { get; set; } 
        public long storage { get; set; } 
    }
}
