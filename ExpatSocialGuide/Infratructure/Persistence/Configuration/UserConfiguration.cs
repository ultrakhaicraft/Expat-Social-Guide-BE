using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Persistence.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.NormalizedEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.PasswordHash)
                .HasMaxLength(500);

            builder.Property(x => x.EmployeeCode)
                .HasMaxLength(50);

            builder.Property(x => x.AccountType)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(x => x.Email).IsUnique();
            builder.HasIndex(x => x.NormalizedEmail).IsUnique();
            builder.HasIndex(x => x.EmployeeCode);
            builder.HasIndex(x => x.GoogleId);

            builder.HasCheckConstraint("CK_User_EmailDomain",
                "Email LIKE '%@fpt.edu.vn' OR Email LIKE '%@gmail.com'");

            builder.HasOne(x => x.HREmployee)
                .WithOne(x => x.User)
                .HasForeignKey<User>(x => x.HREmployeeId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(x => x.UserProfile)
                .WithOne(x => x.User)
                .HasForeignKey<UserProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
