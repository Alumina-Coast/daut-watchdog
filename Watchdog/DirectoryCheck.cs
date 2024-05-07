using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public class DirectoryCheck
    {
        public string Directory { get; set; }
        public List<ICondition> Conditions { get; set; }
    }

    public enum ConditionEvent
    {
        INACTIVITY,
        CREATION,
        MODIFICATION,
        DELETION,
    }

    public interface ICondition
    {
        public ConditionEvent Event { get; }
        public bool Check(string filePath);
    }

    public class LastLineContains : ICondition
    {
        public ConditionEvent Event { get; init; } = ConditionEvent.MODIFICATION;
        public string CompareString { get; init; } = string.Empty;

        public bool Check(string filePath)
        {
            if (File.Exists(filePath))
            {
                var lastLine = File.ReadAllLines(filePath).Last();
                return lastLine.Contains(CompareString, StringComparison.InvariantCultureIgnoreCase);
            }
            return false;
        }
    }

    public class InactiveFor : ICondition
    {
        public ConditionEvent Event { get; } = ConditionEvent.INACTIVITY;
        public TimeSpan TimeSpan { get; init; } = TimeSpan.FromDays(1);

        public string PrintAsError(string directory)
        {
            return $"Directorio {directory} inactivo por ";
        }

        public bool Check(string directory)
        {
            var fileInfo = new FileInfo(directory);
            if (!fileInfo.Exists)
            {
                return false;
            }

            return DateTime.UtcNow - fileInfo.LastAccessTimeUtc >= TimeSpan;
        }
    }
}
