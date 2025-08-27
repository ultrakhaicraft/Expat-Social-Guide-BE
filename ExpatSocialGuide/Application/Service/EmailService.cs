using Application.Interface;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body, bool isHtml = true)
        {
            try
            {
                var smtpHost = _configuration["Email:SmtpHost"];
                var smtpPort = int.Parse(_configuration["Email:SmtpPort"]);
                var smtpUser = _configuration["Email:SmtpUser"];
                var smtpPass = _configuration["Email:SmtpPass"];
                var fromEmail = _configuration["Email:From"];

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var message = new MailMessage(fromEmail, to, subject, body)
                {
                    IsBodyHtml = isHtml
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Email sent to {to}");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to send email to {to}");
                return false;
            }
        }

        public async Task<bool> SendVerificationEmailAsync(string email, string token, string code)
        {
            var subject = "Xác thực tài khoản BEESRS";
            var appUrl = _configuration["Email:AppUrl"];
            var body = $@"
                <h2>Xác thực tài khoản</h2>
                <p>Mã xác thực của bạn là: <strong>{code}</strong></p>
                <p>Hoặc click vào link: <a href='{appUrl}/verify-email?token={token}&code={code}'>Xác thực ngay</a></p>
                <p>Link có hiệu lực trong 24 giờ.</p>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendPasswordResetEmailAsync(string email, string token, string code)
        {
            var subject = "Đặt lại mật khẩu BEESRS";
            var appUrl = _configuration["Email:AppUrl"];
            var body = $@"
                <h2>Đặt lại mật khẩu</h2>
                <p>Mã xác thực của bạn là: <strong>{code}</strong></p>
                <p>Hoặc click vào link: <a href='{appUrl}/reset-password?token={token}&code={code}'>Đặt lại mật khẩu</a></p>
                <p>Link có hiệu lực trong 1 giờ.</p>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendWelcomeEmailAsync(string email, string fullName)
        {
            var subject = "Chào mừng đến với BEESRS!";
            var body = $@"
                <h2>Xin chào {fullName}!</h2>
                <p>Chào mừng bạn đến với BEESRS - Hệ thống gợi ý địa điểm cho nhân viên Broadcom.</p>
                <p>Tài khoản của bạn đã được kích hoạt thành công.</p>";

            return await SendEmailAsync(email, subject, body);
        }

        public async Task<bool> SendAccountLockedEmailAsync(string email, DateTime lockedUntil)
        {
            var subject = "Tài khoản đã bị tạm khóa";
            var body = $@"
                <h2>Tài khoản tạm khóa</h2>
                <p>Tài khoản của bạn đã bị tạm khóa do nhiều lần đăng nhập thất bại.</p>
                <p>Thời gian mở khóa: <strong>{lockedUntil:dd/MM/yyyy HH:mm}</strong></p>";

            return await SendEmailAsync(email, subject, body);
        }
    }
}