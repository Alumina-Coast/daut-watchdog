using Microsoft.Extensions.Options;

namespace WorkerService1
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IOptions<Settings> _settings;

        public Worker(ILogger<Worker> logger, IOptions<Settings> settings)
        {
            _logger = logger;
            _settings = settings;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                _logger.LogInformation("Checking every {NDays} days. Email will be sent from {FromEmail} to {ToEmail} using {SmtpServer}:{Port}",
                    _settings.Value.CheckEveryNDays, _settings.Value.EmailSettings.FromEmail, _settings.Value.EmailSettings.ToEmail,
                    _settings.Value.EmailSettings.SmtpServer, _settings.Value.EmailSettings.SmtpPort);

                // Example: Perform some email sending function here
                await Task.Delay(1000 * 60 * 60 * 24 * _settings.Value.CheckEveryNDays, stoppingToken);
            }
        }
    }
}
