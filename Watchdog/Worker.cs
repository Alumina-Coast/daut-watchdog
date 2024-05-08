using System.Diagnostics;
using System.IO;
using MailKit.Net.Smtp;
using Microsoft.Extensions.FileSystemGlobbing;
using MimeKit;

namespace Watchdog
{
    public class Worker(ILogger<Worker> logger, Config config) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly Config _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            foreach (var guard in _config.Guardias)
            {
                guard.Guard();
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                foreach (var guard in _config.Guardias)
                {
                    try
                    {
                        foreach (var message in guard.GetReport())
                        {
                            _logger.LogInformation(message);
                        }
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                    }
                }

                await Task.Delay(_config.IterarCada, stoppingToken);
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
