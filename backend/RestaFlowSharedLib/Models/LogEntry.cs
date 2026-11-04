using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantSharedLib.Models
{
    public class LogEntry
    {
        public int Id { get; set; }
        public string LogLevel { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public string Source { get; set; }
        public DateTime Timestamp { get; set; }
        public string UserName { get; set; }
    }
}
