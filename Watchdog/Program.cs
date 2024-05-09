using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watchdog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/watchdog-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            try
            {
                Log.Information("Starting up the service");
                var builder = Host.CreateApplicationBuilder(args);

                var config = ReadConfigFromYaml("config.yaml");
                builder.Services.AddSingleton(config);
                builder.Services.AddHostedService<Worker>();
                builder.Logging.AddSerilog();

                var host = builder.Build();
                host.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "There was a problem starting the service");
            }
            finally
            {
                Log.CloseAndFlush();
            }
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
