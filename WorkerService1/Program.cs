using System.Text.Json;

namespace WorkerService1
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = Host.CreateApplicationBuilder(args);
            builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

            var jsonOptions = new JsonSerializerOptions();
            jsonOptions.Converters.Add(new ConditionConverter());
            builder.Services.AddSingleton(jsonOptions);

            builder.Services.Configure<Settings>(builder.Configuration.GetSection("Settings"));
            builder.Services.AddHostedService<Worker>();

            var host = builder.Build();
            host.Run();
        }
    }
}