using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true);
        Task<bool> SendVerificationEmailAsync(string email, string token, string code);
        Task<bool> SendPasswordResetEmailAsync(string email, string token, string code);
        Task<bool> SendWelcomeEmailAsync(string email, string fullName);
        Task<bool> SendAccountLockedEmailAsync(string email, DateTime lockedUntil);
    }
}
