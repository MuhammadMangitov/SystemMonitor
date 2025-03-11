using System;

namespace SystemMonitor.Models
{
    public class ProgramDetails
    {
        public string name { get; set; }
        public string version { get; set; }
        public DateTime? installed_date { get; set; }
        public string type { get; set; }
        public int? size { get; set; }
    }
}
