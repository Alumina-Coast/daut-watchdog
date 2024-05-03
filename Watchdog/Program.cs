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
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();

            var yamlData = File.ReadAllText(filePath);
            return deserializer.Deserialize<Config>(yamlData);
        }
    }

    public class Config
    {
        public List<string> FilePaths { get; set; } = new List<string>();
        public int DaysWithoutModification { get; set; } = 1;
        public EmailSettings EmailSettings { get; set; } = new EmailSettings();
    }

    public class EmailSettings
    {
        public string FromEmail { get; set; }
        public string ToEmail { get; set; }
        public string SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
