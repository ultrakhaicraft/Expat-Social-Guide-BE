using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Infratructure.Models.HR
{
    public class CreateHREmployeeModel
    {
        [Required]
        public string EmployeeCode { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@fpt\.edu\.vn$",
            ErrorMessage = "Email must be @fpt.edu.vn")]
        public string CompanyEmail { get; set; }

        [EmailAddress]
        public string? PersonalEmail { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        [Required]
        public string Department { get; set; }

        public string? Position { get; set; }
        public string? Campus { get; set; }
        public string? Building { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime? ContractEndDate { get; set; }
    }

    public class UpdateHREmployeeModel
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PersonalEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
        public string? Campus { get; set; }
        public string? Building { get; set; }
        public DateTime? ContractEndDate { get; set; }
    }

    public class ImportHREmployeeModel
    {
        [Required]
        public IFormFile File { get; set; }
        public bool UpdateExisting { get; set; } = false;
    }

    public class HREmployeeDto
    {
        public Guid Id { get; set; }
        public string EmployeeCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}";
        public string CompanyEmail { get; set; }
        public string? PersonalEmail { get; set; }
        public string? PhoneNumber { get; set; }
        public string Department { get; set; }
        public string? Position { get; set; }
        public string? Campus { get; set; }
        public string Status { get; set; }
        public bool IsRegistered { get; set; }
        public DateTime? RegisteredAt { get; set; }
        public bool IsEmailVerified { get; set; }
        public DateTime? JoinDate { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class HRStatisticsDto
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int InactiveEmployees { get; set; }
        public int RegisteredUsers { get; set; }
        public int UnregisteredUsers { get; set; }
        public int VerifiedEmails { get; set; }
        public Dictionary<string, int> ByDepartment { get; set; }
        public Dictionary<string, int> ByCampus { get; set; }
    }
}
