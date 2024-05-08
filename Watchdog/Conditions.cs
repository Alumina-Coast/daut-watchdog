using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public interface ICondition
    {
        public WatcherChangeTypes? Event { get; }
        public bool IsConditionMet(string filePath);
        public string PrintAsMessage(string filePath);
    }

    public class NewLineContains : ICondition
    {
        public WatcherChangeTypes? Event { get; init; } = WatcherChangeTypes.Changed;
        public string CompareString { get; init; } = string.Empty;

        public string PrintAsMessage(string filePath)
        {
            return $"La última línea del archivo '{filePath}' contiene el string: '{CompareString}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var lastLine = File.ReadAllLines(filePath).LastOrDefault();
                if (lastLine is not null)
                {
                    return lastLine.Contains(CompareString, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            return false;
        }
    }

    public class InactiveFor : ICondition
    {
        public WatcherChangeTypes? Event { get; } = null;
        public TimeSpan TimeSpan { get; init; } = TimeSpan.FromDays(1);

        public string PrintAsMessage(string filePath)
        {
            return $"Inactividad superior a {TimeSpan} detectada en '{filePath}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    return DateTime.UtcNow - fileInfo.LastAccessTimeUtc >= TimeSpan;
                }
            }
            return false;
        }
    }
}
