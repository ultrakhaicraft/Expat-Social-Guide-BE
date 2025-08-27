using Domain.Entities;
using Infratructure.Interface;
using Infratructure.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Infratructure.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly ESGDBContext _context;

        public AuthRepository(ESGDBContext context)
        {
            _context = context;
        }

        // User operations
        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var normalizedEmail = email.ToUpper();
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserProfile)
                .Include(u => u.HREmployee)
                .FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail);
        }

        public async Task<User?> GetUserByIdAsync(Guid userId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserProfile)
                .Include(u => u.HREmployee)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetUserByGoogleIdAsync(string googleId)
        {
            return await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .Include(u => u.UserProfile)
                .Include(u => u.HREmployee)
                .FirstOrDefaultAsync(u => u.GoogleId == googleId);
        }

        public async Task<User> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            _context.Users.Update(user);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsEmailExistsAsync(string email)
        {
            var normalizedEmail = email.ToUpper();
            return await _context.Users.AnyAsync(u => u.NormalizedEmail == normalizedEmail);
        }

        // HR Employee operations
        public async Task<HREmployee?> GetHREmployeeByEmailAsync(string email)
        {
            return await _context.HREmployees
                .FirstOrDefaultAsync(h => h.CompanyEmail.ToUpper() == email.ToUpper());
        }

        public async Task<HREmployee?> GetHREmployeeByEmailAndCodeAsync(string email, string employeeCode)
        {
            return await _context.HREmployees
                .FirstOrDefaultAsync(h => h.CompanyEmail.ToUpper() == email.ToUpper()
                                        && h.EmployeeCode == employeeCode);
        }

        public async Task UpdateHREmployeeAsync(HREmployee employee)
        {
            _context.HREmployees.Update(employee);
            await _context.SaveChangesAsync();
        }

        // Token operations
        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task<RefreshToken> CreateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task UpdateRefreshTokenAsync(RefreshToken refreshToken)
        {
            _context.RefreshTokens.Update(refreshToken);
            await _context.SaveChangesAsync();
        }

        public async Task RevokeAllUserTokensAsync(Guid userId)
        {
            var tokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
            }

            await _context.SaveChangesAsync();
        }

        // Password Reset operations
        public async Task<PasswordResetToken?> GetPasswordResetTokenAsync(string token, string code)
        {
            return await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.Code == code);
        }

        public async Task<PasswordResetToken> CreatePasswordResetTokenAsync(PasswordResetToken resetToken)
        {
            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync();
            return resetToken;
        }

        public async Task UpdatePasswordResetTokenAsync(PasswordResetToken resetToken)
        {
            _context.PasswordResetTokens.Update(resetToken);
            await _context.SaveChangesAsync();
        }

        // Email Verification operations
        public async Task<EmailVerificationToken?> GetEmailVerificationTokenAsync(string token, string code)
        {
            return await _context.EmailVerificationTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.Token == token && t.Code == code);
        }

        public async Task<EmailVerificationToken> CreateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
        {
            _context.EmailVerificationTokens.Add(verificationToken);
            await _context.SaveChangesAsync();
            return verificationToken;
        }

        public async Task UpdateEmailVerificationTokenAsync(EmailVerificationToken verificationToken)
        {
            _context.EmailVerificationTokens.Update(verificationToken);
            await _context.SaveChangesAsync();
        }

        // Role operations
        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles
                .FirstOrDefaultAsync(r => r.Name == roleName && r.IsActive);
        }

        public async Task<UserRole> AddUserRoleAsync(UserRole userRole)
        {
            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync();
            return userRole;
        }

        public async Task<List<string>> GetUserRolesAsync(Guid userId)
        {
            return await _context.UserRoles
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.Name)
                .ToListAsync();
        }

        // Profile operations
        public async Task<UserProfile> CreateUserProfileAsync(UserProfile profile)
        {
            _context.UserProfiles.Add(profile);
            await _context.SaveChangesAsync();
            return profile;
        }

        public async Task UpdateUserProfileAsync(UserProfile profile)
        {
            _context.UserProfiles.Update(profile);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync() > 0;
        }
    }
}