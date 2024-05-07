using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
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

        public bool Check(string filePath)
        {
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Exists)
            {
                return false;
            }

            return DateTime.UtcNow - fileInfo.LastAccessTimeUtc >= TimeSpan;
        }
    }

    public class Config
    {
        public List<ICondition> Conditions { get; set; } = [];
        public int CheckEveryNDays { get; set; } = 1;
        public EmailSettings EmailSettings { get; set; } = new EmailSettings();
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; } = string.Empty;
        public string ToEmail { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
