using Serilog;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace Watchdog
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);

            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .WriteTo.File("logs/watchdog-.log", rollingInterval: RollingInterval.Day)
                .CreateLogger();
            try
            {
                Log.Information("Starting up the service");
                var config = ReadConfigFromYaml("config.yaml");
                var host = Host.CreateDefaultBuilder(args)
                    .ConfigureServices(services =>
                    {
                        services.AddSingleton(config);
                        services.AddHostedService<Worker>();
                    })
                    .UseWindowsService(options =>
                    {
                        options.ServiceName = "Utilidades Monitoreo";
                    })
                    .UseSerilog()
                    .Build();

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
