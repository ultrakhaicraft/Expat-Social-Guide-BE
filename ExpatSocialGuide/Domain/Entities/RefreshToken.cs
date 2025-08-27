using System;

namespace Domain.Entities
{
    public class RefreshToken : BaseEntity
    {
        public Guid UserId { get; set; }
        public string Token { get; set; }
        public string JwtId { get; set; }
        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTime? RevokedAt { get; set; }
        public string? CreatedByIp { get; set; }
        public string? UserAgent { get; set; }
        public string? DeviceId { get; set; }

        // Navigation
        public virtual User User { get; set; }

        public bool IsActive => !IsRevoked && !IsUsed && DateTime.UtcNow < ExpiresAt;
    }
}