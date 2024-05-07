using System.IO;
using MailKit.Net.Smtp;
using MimeKit;

namespace Watchdog
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly Config _config;

        public Worker(ILogger<Worker> logger, Config config)
        {
            _logger = logger;
            _config = config;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                //foreach (var filePath in _config.FilePaths)
                //{
                //    var fileInfo = new FileInfo(filePath);
                //    if (!fileInfo.Exists)
                //    {
                //        _logger.LogWarning($"File not found: {filePath}");
                //        continue;
                //    }

                //    var lastWriteTime = fileInfo.LastWriteTime;
                //    if ((DateTime.Now - lastWriteTime).Days >= _config.DaysWithoutModification)
                //    {
                //        _logger.LogInformation($"File {filePath} has not been modified in {_config.DaysWithoutModification} days.");
                //        // await SendWarningEmail(filePath, lastWriteTime);
                //    }
                //}

                await Task.Delay(60000, stoppingToken);
            }
        }

        //private async Task SendWarningEmail(string filePath, DateTime lastModified)
        //{
        //    if (_config.EmailSettings is null)
        //    {
        //        return;
        //    }
        //    var message = new MimeMessage();
        //    message.From.Add(new MailboxAddress(_config.EmailSettings.FromEmail, _config.EmailSettings.FromEmail));
        //    message.To.Add(new MailboxAddress(_config.EmailSettings.ToEmail, _config.EmailSettings.ToEmail));
        //    message.Subject = "File Modification Alert";
        //    message.Body = new TextPart("plain")
        //    {
        //        Text = $"The file {filePath} has not been modified since {lastModified}."
        //    };

        //    using var client = new SmtpClient();
        //    await client.ConnectAsync(_config.EmailSettings.SmtpServer, _config.EmailSettings.SmtpPort, true);
        //    await client.AuthenticateAsync(_config.EmailSettings.Username, _config.EmailSettings.Password);
        //    await client.SendAsync(message);
        //    await client.DisconnectAsync(true);
        //}
    }
}
