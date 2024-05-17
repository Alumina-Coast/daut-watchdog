using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public interface ICondition
    {
        public WatcherChangeTypes? TipoDeEvento { get; }
        public bool Unanime { get; set; }
        public bool IsConditionMet(string filePath);
        public string PrintAsMessage(string filePath);
    }

    public class NewLineContains : ICondition
    {
        public WatcherChangeTypes? TipoDeEvento { get; init; } = WatcherChangeTypes.Changed;
        public bool Unanime { get; set; } = false;
        public string Contiene { get; init; } = string.Empty;

        public string PrintAsMessage(string filePath)
        {
            return $"La última línea del archivo '{filePath}' contiene el string: '{Contiene}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var lastLine = File.ReadAllLines(filePath).LastOrDefault();
                if (lastLine is not null)
                {
                    return lastLine.Contains(Contiene, StringComparison.InvariantCultureIgnoreCase);
                }
            }
            return false;
        }
    }

    public class FileInactiveFor : ICondition
    {
        public WatcherChangeTypes? TipoDeEvento { get; } = null;
        public bool Unanime { get; set; } = false;
        public TimeSpan TiempoLimite { get; init; } = TimeSpan.FromDays(1);

        public string PrintAsMessage(string filePath)
        {
            return $"Inactividad superior a {TiempoLimite} detectada en '{filePath}'.";
        }

        public bool IsConditionMet(string filePath)
        {
            if (File.Exists(filePath))
            {
                var fileInfo = new FileInfo(filePath);
                if (fileInfo.Exists)
                {
                    return DateTime.UtcNow - fileInfo.LastAccessTimeUtc >= TiempoLimite;
                }
            }
            return false;
        }
    }
}
