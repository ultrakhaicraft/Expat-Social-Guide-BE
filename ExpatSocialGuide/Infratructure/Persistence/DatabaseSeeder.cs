using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Shared.Constants;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Persistence
{
    public static class DatabaseSeeder
    {
        public static void Seed(ModelBuilder modelBuilder)
        {
            // Seed Roles
            var adminRole = new Role
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = AuthConstants.Roles.Admin,
                NormalizedName = AuthConstants.Roles.Admin.ToUpper(),
                Description = "System Administrator",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var employeeRole = new Role
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = AuthConstants.Roles.Employee,
                NormalizedName = AuthConstants.Roles.Employee.ToUpper(),
                Description = "Employee User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var guestRole = new Role
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Name = AuthConstants.Roles.Guest,
                NormalizedName = AuthConstants.Roles.Guest.ToUpper(),
                Description = "Guest User",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<Role>().HasData(adminRole, employeeRole, guestRole);

            // Seed HR Employee
            var hrEmployee = new HREmployee
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                EmployeeCode = "ADMIN001",
                FirstName = "Admin",
                LastName = "System",
                CompanyEmail = "admin@fpt.edu.vn",
                Department = "IT",
                Position = "System Administrator",
                Campus = "HCM",
                Status = AuthConstants.EmployeeStatus.Active,
                IsRegistered = true,
                IsEmailVerified = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<HREmployee>().HasData(hrEmployee);

            // Seed Admin User
            var adminUser = new User
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Email = "admin@fpt.edu.vn",
                NormalizedEmail = "ADMIN@FPT.EDU.VN",
                PasswordHash = PasswordHelper.HashPassword("Admin@123"),
                HREmployeeId = hrEmployee.Id,
                EmployeeCode = hrEmployee.EmployeeCode,
                AccountType = AuthConstants.AccountTypes.Internal,
                IsEmailVerified = true,
                EmailVerifiedAt = DateTime.UtcNow,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<User>().HasData(adminUser);

            // Seed Admin Profile
            var adminProfile = new UserProfile
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                UserId = adminUser.Id,
                FirstName = "Admin",
                LastName = "System",
                Department = "IT",
                Position = "System Administrator",
                Campus = "HCM",
                PreferredLanguage = "vi",
                OnboardingCompleted = true,
                CreatedAt = DateTime.UtcNow
            };

            modelBuilder.Entity<UserProfile>().HasData(adminProfile);

            // Seed UserRole
            modelBuilder.Entity<UserRole>().HasData(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = "System"
            });
        }
    }
}