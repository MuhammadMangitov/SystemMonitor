using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SystemMonitor.Models
{
    public class JwtResponse
    {
        public string Token { get; set; }
    }

    public class JwtToken
    {
        public int Id { get; set; }
        public string Token { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
