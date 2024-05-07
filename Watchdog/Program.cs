using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watchdog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);

            var config = ReadConfigFromYaml("config.yaml");
            builder.Services.AddSingleton(config);
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }

        private static Config ReadConfigFromYaml(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .WithTypeConverter(new ConditionConverter())
                .Build();

            var yamlData = File.ReadAllText(filePath);
            return deserializer.Deserialize<Config>(yamlData);
        }
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

    public class InactiveFor: ICondition
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
