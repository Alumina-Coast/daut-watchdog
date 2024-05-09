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
                guard.WarningRaised += Guard_WarningRaised;
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
                            await SendWarningEmails(message);
                        }
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        await SendWarningEmails(ex.Message);
                    }
                }

                await Task.Delay(_config.IterarCada, stoppingToken);
            }
        }

        private async void Guard_WarningRaised(object sender, WarningRaisedEventArgs e)
        {
            if (e.SystemError)
            {
                _logger.LogError(e.Message);
                await SendWarningEmails(e.Message);
            }
            else
            {
                _logger.LogInformation(e.Message);
                await SendWarningEmails(e.Message);
            }
        }

        private async Task SendWarningEmails(string body)
        {
            _logger.LogInformation("Sending out warning emails");
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_config.ConfiguracionEmail.SmtpServer, _config.ConfiguracionEmail.SmtpPort, true);
                await client.AuthenticateAsync(_config.ConfiguracionEmail.Username, _config.ConfiguracionEmail.Password);
                foreach (var emailAddress in _config.DireccionesEmail)
                {
                    var message = new MimeMessage();
                    message.From.Add(new MailboxAddress(_config.ConfiguracionEmail.FromEmail, _config.ConfiguracionEmail.FromEmail));
                    message.To.Add(new MailboxAddress(emailAddress, emailAddress));
                    message.Subject = "File Modification Alert";
                    message.Body = new TextPart("plain")
                    {
                        Text = body
                    };

                    await client.SendAsync(message);
                }
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
