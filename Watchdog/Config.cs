using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public class Config
    {
        public List<DirectoryGuard> Guardias { get; set; } = [];
        public TimeSpan IterarCada { get; set; } = TimeSpan.FromDays(1);
        public SmtpSettings ConfiguracionEmail { get; set; } = new();
        public List<string> DireccionesEmail { get; set; } = [];
    }

    public class SmtpSettings
    {
        public string FromEmail { get; set; } = string.Empty;
        public string Subject { get; set; } = string.Empty;
        public string EmailHeader { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public bool UseSsl { get; set; } = false;
        public bool UseCredentials { get; set; } = false;
    }
}
