using System.Diagnostics;

namespace Handlers
{
    public class Delegates
    {
        public delegate void LogHandler(string message, EventLogEntryType msgType);
    }
}