using System;

namespace Domain.Entities
{
    public class EmailVerificationToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string Code { get; set; } // 6-digit code
        public string Email { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }

        // Navigation
        public virtual User User { get; set; }

        public EmailVerificationToken()
        {
            ExpiresAt = DateTime.UtcNow.AddHours(24);
            Code = new Random().Next(100000, 999999).ToString();
        }

        public bool IsValid => !IsUsed && DateTime.UtcNow < ExpiresAt;
    }
}