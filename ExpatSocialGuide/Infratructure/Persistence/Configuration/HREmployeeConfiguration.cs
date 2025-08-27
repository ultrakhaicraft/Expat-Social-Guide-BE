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
    public class HREmployeeConfiguration : IEntityTypeConfiguration<HREmployee>
    {
        public void Configure(EntityTypeBuilder<HREmployee> builder)
        {
            builder.ToTable("HREmployees");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.EmployeeCode)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.CompanyEmail)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.Department)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Status)
                .IsRequired()
                .HasMaxLength(20);

            builder.HasIndex(x => x.EmployeeCode).IsUnique();
            builder.HasIndex(x => x.CompanyEmail).IsUnique();

            builder.HasCheckConstraint("CK_HREmployee_EmailDomain",
                "CompanyEmail LIKE '%@fpt.edu.vn'");
        }
    }
}
