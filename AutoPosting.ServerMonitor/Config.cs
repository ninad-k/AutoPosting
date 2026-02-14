using System.Collections.Generic;

namespace AutoPosting.ServerMonitor
{
    public class AppConfig
    {
        public List<AccountConfig> Accounts { get; set; } = new List<AccountConfig>();
        
        // SMTP Settings
        public string SmtpHost { get; set; }
        public int SmtpPort { get; set; }
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        
        // WhatsApp Settings
        public string WhatsAppApiUrl { get; set; }
        public string WhatsAppApiKey { get; set; }
    }

    public class AccountConfig
    {
        public string Name { get; set; }
        public string Type { get; set; } // Email, WhatsApp
        public string TargetUrl { get; set; }
        public string Recipient { get; set; } // Email or Phone
        public bool IsScheduleEnabled { get; set; }
        public string Details { get; set; }
        public string Status { get; set; } // Active, Inactive, Error
        public string LastSync { get; set; }
        public DateTime LastRunTime { get; set; }
        public DateTime NextRunTime { get; set; }
        public int MessagesSentCount { get; set; }
    }
}
