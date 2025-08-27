using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Models.Email
{
    public class EmailSettings
    {
        public string SmtpHost { get; set; } = "smtp.gmail.com";
        public int SmtpPort { get; set; } = 587;
        public string SmtpUser { get; set; }
        public string SmtpPass { get; set; }
        public string From { get; set; }
        public string FromName { get; set; } = "BEESRS System";
        public bool EnableSsl { get; set; } = true;
        public string AppUrl { get; set; } = "https://localhost:5001";
    }

    public class EmailTemplate
    {
        public string Name { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
    }

    public class EmailRequest
    {
        public string To { get; set; }
        public string? Cc { get; set; }
        public string? Bcc { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public bool IsHtml { get; set; } = true;
        public List<EmailAttachment>? Attachments { get; set; }
    }

    public class EmailAttachment
    {
        public string FileName { get; set; }
        public byte[] Content { get; set; }
        public string ContentType { get; set; }
    }
}
