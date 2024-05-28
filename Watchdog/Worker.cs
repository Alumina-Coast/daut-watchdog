using System;
using System.Diagnostics;
using System.IO;
using MailKit.Net.Smtp;
using Microsoft.Extensions.FileSystemGlobbing;
using MimeKit;
using Serilog;

namespace Watchdog
{
    public class Worker(ILogger<Worker> logger, Config config) : BackgroundService
    {
        private readonly ILogger<Worker> _logger = logger;
        private readonly Config _config = config;

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                foreach (var guard in _config.Guardias)
                {
                    guard.WarningRaised += Guard_WarningRaised;
                }

                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Doing routine check.");
                    string body = $"{_config.ConfiguracionEmail.EmailHeader}\r\n";
                    bool send = false;

                    var faultyFileWatchers = new List<string>();
                    foreach (var guard in _config.Guardias)
                    {
                        try
                        {
                            if (!guard.IsGuarding)
                            {
                                guard.Start();
                            }
                        }
                        catch (Exception ex)
                        {
                            send = true;
                            faultyFileWatchers.Add($"Guardia '{guard.Nombre}' no está funcionando. Razón: '{ex.Message}'.");
                        }
                    }

                    if (faultyFileWatchers.Count != 0)
                    {
                        body += $"\nFALLA EN GUARDIAS:\n";
                        foreach (var message in faultyFileWatchers)
                        {
                            body += $"    - {message}\n";
                            _logger.LogInformation("{Message}", message);
                        }
                        body += "\r\n";
                    }

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
                                    _logger.LogInformation("{Message}", message);
                                }
                                body += "\r\n";
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError("{Message}", ex.Message);
                            send = true;
                            body += $"\nERROR: {ex.Message}\n";
                        }
                    }
                    if (send)
                    {
                        await SendWarningEmails("Reporte", body);
                    }

                    await Task.Delay(_config.IterarCada, stoppingToken);
                }
            }
            catch (TaskCanceledException)
            {

            }
            catch (Exception ex)
            {
                _logger.LogError("Sudden worker exit: {Message} - {Error}", ex.Message, ex.GetType().ToString());

                try
                {
                    await SendWarningEmails("Cierre inesperado", $"Sudden worker exit: {ex.Message}");
                }
                catch { }

                throw;
            }
        }

        private async void Guard_WarningRaised(object sender, WarningRaisedEventArgs e)
        {
            string body = $"{_config.ConfiguracionEmail.EmailHeader}\r\n\n{e.Parent.Nombre}\n    - {e.Message}";
            if (e.SystemError)
            {
                _logger.LogError("{Message}", e.Message);
                await SendWarningEmails(e.Parent.Nombre, body);
            }
            else
            {
                _logger.LogInformation("{Message}", e.Message);
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

                _logger.LogInformation("{Message}", await client.SendAsync(message));
                await client.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError("{Message}", ex.Message);
            }
        }
    }
}
