using System;

namespace Domain.Entities
{
    public class HREmployee : BaseEntity
    {
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string CompanyEmail { get; set; } // @fpt.edu.vn
        public string? PersonalEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string Department { get; set; }
        public string? Position { get; set; }
        public string? Campus { get; set; }
        public string? Building { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ContractEndDate { get; set; }

        // Employment Status
        public string Status { get; set; } = "Active"; // Active, Inactive, Suspended, Resigned

        // Account Registration
        public bool IsRegistered { get; set; } = false;
        public Guid? UserId { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public bool IsEmailVerified { get; set; } = false;
        public DateTime? EmailVerifiedAt { get; set; }

        // Navigation
        public virtual User? User { get; set; }
    }
}