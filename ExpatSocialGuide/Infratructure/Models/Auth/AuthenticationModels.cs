using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Infratructure.Models.Auth
{
    // Request Models
    public class LoginModel
    {
        [Required(ErrorMessage = "Email là bắt buộc")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
        public string Password { get; set; }

        public bool RememberMe { get; set; } = false;
        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
    }

    public class GoogleLoginModel
    {
        [Required]
        public string IdToken { get; set; }
        public string? DeviceId { get; set; }
    }

    public class RegisterModel
    {
        [Required]
        [EmailAddress]
        [RegularExpression(@"^[a-zA-Z0-9._%+-]+@fpt\.edu\.vn$",
            ErrorMessage = "Email phải có định dạng @fpt.edu.vn")]
        public string Email { get; set; }

        [Required]
        [MinLength(8)]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{8,}$",
            ErrorMessage = "Mật khẩu phải chứa chữ hoa, chữ thường, số và ký tự đặc biệt")]
        public string Password { get; set; }

        [Required]
        [Compare("Password")]
        public string ConfirmPassword { get; set; }

        [Required]
        public string EmployeeCode { get; set; }

        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        public string? PhoneNumber { get; set; }
    }

    public class ForgotPasswordModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Code { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    public class VerifyEmailModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Code { get; set; }
    }

    public class RefreshTokenModel
    {
        [Required]
        public string AccessToken { get; set; }

        [Required]
        public string RefreshToken { get; set; }
    }

    public class ChangePasswordModel
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(8)]
        public string NewPassword { get; set; }

        [Required]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }

    // Response Models
    public class AuthenticationResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public UserDto? User { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public object? Data { get; set; }
        public List<string>? Errors { get; set; }
    }

    public class UserDto
    {
        public Guid Id { get; set; }
        public string Email { get; set; }
        public string? FullName { get; set; }
        public string? ProfilePicture { get; set; }
        public List<string> Roles { get; set; }
        public bool IsEmailVerified { get; set; }
        public string? Department { get; set; }
        public string? Position { get; set; }
    }
}