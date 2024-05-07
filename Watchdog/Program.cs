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
}
