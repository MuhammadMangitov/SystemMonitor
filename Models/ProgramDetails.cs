using System;

namespace SystemMonitor.Models
{
    public class ProgramDetails
    {
        public string name { get; set; }
        public string executable_path { get; set; }
        public long? file_size { get; set; }
        public DateTime? creation_time { get; set; }
        public string version { get; set; }
    }
}
