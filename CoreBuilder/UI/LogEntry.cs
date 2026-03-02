using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CoreBuilder.UI
{
    public class LogEntry
    {
        public string Message { get; set; } = string.Empty;
        public string Timestamp { get; set; } = string.Empty;

        // For the UI to decide whether to show the timestamp
        public override string ToString() => string.IsNullOrEmpty(Timestamp)
            ? Message
            : $"[{Timestamp}] {Message}";
    }
}
