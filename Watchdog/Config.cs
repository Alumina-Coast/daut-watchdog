using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Watchdog
{
    public class Config
    {
        public List<DirectoryCheck> DirectoryChecks { get; set; } = [];
        public int CheckEveryNDays { get; set; } = 1;
        public SmtpSettings EmailSettings { get; set; } = new();
        public List<string> EmailAddresses { get; set; }
    }

    public class SmtpSettings
    {
        public string FromEmail { get; set; } = string.Empty;
        public string SmtpServer { get; set; } = string.Empty;
        public int SmtpPort { get; set; } = 0;
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
