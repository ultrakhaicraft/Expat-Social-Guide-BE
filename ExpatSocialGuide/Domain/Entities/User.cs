using System;
using System.Collections.Generic;

namespace Domain.Entities
{
    public class User : BaseEntity
    {
        // Authentication
        public string Email { get; set; }
        public string NormalizedEmail { get; set; }
        public string? PasswordHash { get; set; }

        // HR Link
        public Guid? HREmployeeId { get; set; }
        public string? EmployeeCode { get; set; }

        // Account Type
        public string AccountType { get; set; } = "Internal"; // Internal, Google, External

        // Account Status
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsLocked { get; set; } = false;
        public DateTime? LockedUntil { get; set; }
        public string? LockReason { get; set; }

        // Security
        public int FailedLoginAttempts { get; set; } = 0;
        public DateTime? LastFailedLoginAt { get; set; }
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public string? LastLoginProvider { get; set; }

        // Google Account
        public string? GoogleId { get; set; }
        public string? GoogleEmail { get; set; }
        public bool IsGoogleEmailVerified { get; set; } = false;

        // Two Factor Authentication
        public bool TwoFactorEnabled { get; set; } = false;
        public string? TwoFactorSecret { get; set; }

        // Navigation Properties
        public virtual HREmployee? HREmployee { get; set; }
        public virtual UserProfile? UserProfile { get; set; }
        public virtual ICollection<UserRole> UserRoles { get; set; }
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; }
        public virtual ICollection<PasswordResetToken> PasswordResetTokens { get; set; }
        public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; }

        public User()
        {
            UserRoles = new HashSet<UserRole>();
            RefreshTokens = new HashSet<RefreshToken>();
            PasswordResetTokens = new HashSet<PasswordResetToken>();
            EmailVerificationTokens = new HashSet<EmailVerificationToken>();
        }
    }
}