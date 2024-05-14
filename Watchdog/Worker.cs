using System;
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
                string body = $"{_config.ConfiguracionEmail.EmailHeader}\r\n";
                bool send = false;
                foreach (var guard in _config.Guardias)
                {
                    try
                    {
                        var report = guard.GetReport();
                        if (report.Count > 0)
                        {
                            send = true;
                            body += $"\n{guard.Nombre}:\n";
                            foreach (var message in report)
                            {
                                body += $"    - {message}\n";
                                _logger.LogInformation(message);
                            }
                            body += "\r\n" ;
                        }
                    } 
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.Message);
                        await SendWarningEmails(guard.Nombre, ex.Message);
                    }
                }
                if (send)
                {
                    await SendWarningEmails("Reporte", body);
                }

                await Task.Delay(_config.IterarCada, stoppingToken);
            }
        }

        private async void Guard_WarningRaised(object sender, WarningRaisedEventArgs e)
        {
            string body = $"{_config.ConfiguracionEmail.EmailHeader}\r\n\n{e.Parent.Nombre}\n    - {e.Message}";
            if (e.SystemError)
            {
                _logger.LogError(e.Message);
                await SendWarningEmails(e.Parent.Nombre, body);
            }
            else
            {
                _logger.LogInformation(e.Message);
                await SendWarningEmails(e.Parent.Nombre, body);
            }
        }

        private async Task SendWarningEmails(string subjectSuffix, string body)
        {
            _logger.LogInformation("Sending out warning emails");
            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(_config.ConfiguracionEmail.SmtpServer, _config.ConfiguracionEmail.SmtpPort, _config.ConfiguracionEmail.UseSsl);
                if (_config.ConfiguracionEmail.UseCredentials)
                {
                    await client.AuthenticateAsync(_config.ConfiguracionEmail.Username, _config.ConfiguracionEmail.Password);
                }
                var message = new MimeMessage();
                message.From.Add(new MailboxAddress(_config.ConfiguracionEmail.FromEmail, _config.ConfiguracionEmail.FromEmail));
                foreach (var emailAddress in _config.DireccionesEmail)
                {
                    message.To.Add(new MailboxAddress(emailAddress, emailAddress));
                }
                message.Subject = $"{_config.ConfiguracionEmail.Subject} - {subjectSuffix}";
                message.Body = new TextPart("plain")
                {
                    Text = body
                };

                _logger.LogInformation(await client.SendAsync(message));
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
    }
}
