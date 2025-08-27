using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Interface
{
    public interface IAuthRepository
    {
        // User operations
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
        Task<User?> GetUserByGoogleIdAsync(string googleId);
        Task<User> CreateUserAsync(User user);
        Task UpdateUserAsync(User user);
        Task<bool> IsEmailExistsAsync(string email);

        // HR Employee operations
        Task<HREmployee?> GetHREmployeeByEmailAsync(string email);
        Task<HREmployee?> GetHREmployeeByEmailAndCodeAsync(string email, string employeeCode);
        Task UpdateHREmployeeAsync(HREmployee employee);

        // Token operations
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken);
        Task UpdateRefreshTokenAsync(RefreshToken refreshToken);
        Task RevokeAllUserTokensAsync(Guid userId);

        // Password Reset operations
        Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, string code);
        Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken resetToken);
        Task UpdatePasswordResetTokenAsync(PasswordResetToken resetToken);

        // Email Verification operations
        Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token, string code);
        Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken);
        Task UpdateEmailVerificationTokenAsync(EmailVerificationToken verificationToken);

        // Role operations
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task<UserRole> AddUserRoleAsync(UserRole userRole);
        Task<List<string>> GetUserRolesAsync(Guid userId);

        // Profile operations
        Task<UserProfile> CreateUserProfileAsync(UserProfile profile);
        Task UpdateUserProfileAsync(UserProfile profile);

        // Unit of Work
        Task<bool> SaveChangesAsync();
    }
}
