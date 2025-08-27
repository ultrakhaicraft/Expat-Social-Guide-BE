using Application.Interface;
using Domain.Entities;
using Google.Apis.Auth;
using Infratructure.Interface;
using Infratructure.Models.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Shared.Constants;
using Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Application.Service
{
    public class AuthenService : IAuthenService
    {
        private readonly IAuthRepository _authRepository;
        private readonly ITokenService _tokenService;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthenService> _logger;

        public AuthenService(
            IAuthRepository authRepository,
            ITokenService tokenService,
            IEmailService emailService,
            IConfiguration configuration,
            ILogger<AuthenService> logger)
        {
            _authRepository = authRepository;
            _tokenService = tokenService;
            _emailService = emailService;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<AuthenticationResult> LoginAsync(LoginModel model)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(model.Email);

                if (user == null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                // Check account lock
                if (user.IsLocked && user.LockedUntil > DateTime.UtcNow)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = $"Tài khoản bị khóa đến {user.LockedUntil:dd/MM/yyyy HH:mm}"
                    };
                }

                // Verify password
                if (string.IsNullOrEmpty(user.PasswordHash) ||
                    !PasswordHelper.VerifyPassword(model.Password, user.PasswordHash))
                {
                    user.FailedLoginAttempts++;
                    user.LastFailedLoginAt = DateTime.UtcNow;

                    if (user.FailedLoginAttempts >= 5)
                    {
                        user.IsLocked = true;
                        user.LockedUntil = DateTime.UtcNow.AddMinutes(30);
                        user.LockReason = "Too many failed attempts";

                        await _authRepository.UpdateUserAsync(user);
                        await _emailService.SendAccountLockedEmailAsync(user.Email, user.LockedUntil.Value);
                    }
                    else
                    {
                        await _authRepository.UpdateUserAsync(user);
                    }

                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Email hoặc mật khẩu không đúng"
                    };
                }

                // Check email verification
                if (!user.IsEmailVerified)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Email chưa được xác thực"
                    };
                }

                // Check active status
                if (!user.IsActive)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Tài khoản đã bị vô hiệu hóa"
                    };
                }

                // Success login
                user.FailedLoginAttempts = 0;
                user.LastLoginAt = DateTime.UtcNow;
                user.LastLoginIp = model.IpAddress;
                user.LastLoginProvider = "Password";
                await _authRepository.UpdateUserAsync(user);

                // Generate tokens
                var roles = await _authRepository.GetUserRolesAsync(user.Id);
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token
                await _authRepository.CreateRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    CreatedByIp = model.IpAddress,
                    DeviceId = model.DeviceId
                });

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.UserProfile?.FullName,
                        ProfilePicture = user.UserProfile?.ProfilePictureUrl,
                        Roles = roles,
                        IsEmailVerified = user.IsEmailVerified,
                        Department = user.UserProfile?.Department,
                        Position = user.UserProfile?.Position
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LoginAsync");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AuthenticationResult> GoogleLoginAsync(GoogleLoginModel model)
        {
            try
            {
                // Validate Google token
                var settings = new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = new[] { _configuration["Authentication:Google:ClientId"] }
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(model.IdToken, settings);

                if (payload == null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Token Google không hợp lệ"
                    };
                }

                // Check domain
                if (!payload.Email.EndsWith("@fpt.edu.vn"))
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Chỉ email @fpt.edu.vn được phép đăng nhập"
                    };
                }

                // Find user
                var user = await _authRepository.GetUserByGoogleIdAsync(payload.Subject)
                        ?? await _authRepository.GetUserByEmailAsync(payload.Email);

                if (user == null)
                {
                    // Check HR Employee
                    var hrEmployee = await _authRepository.GetHREmployeeByEmailAsync(payload.Email);

                    if (hrEmployee == null)
                    {
                        return new AuthenticationResult
                        {
                            Success = false,
                            Message = "Email không tồn tại trong hệ thống HR"
                        };
                    }

                    if (hrEmployee.Status != AuthConstants.EmployeeStatus.Active)
                    {
                        return new AuthenticationResult
                        {
                            Success = false,
                            Message = "Tài khoản nhân viên không hoạt động"
                        };
                    }

                    // Create new user
                    user = new User
                    {
                        Email = payload.Email,
                        NormalizedEmail = payload.Email.ToUpper(),
                        GoogleId = payload.Subject,
                        GoogleEmail = payload.Email,
                        IsGoogleEmailVerified = payload.EmailVerified,
                        AccountType = AuthConstants.AccountTypes.Google,
                        IsEmailVerified = true,
                        EmailVerifiedAt = DateTime.UtcNow,
                        HREmployeeId = hrEmployee.Id,
                        EmployeeCode = hrEmployee.EmployeeCode,
                        IsActive = true
                    };

                    user = await _authRepository.CreateUserAsync(user);

                    // Create profile
                    await _authRepository.CreateUserProfileAsync(new UserProfile
                    {
                        UserId = user.Id,
                        FirstName = payload.GivenName ?? hrEmployee.FirstName,
                        LastName = payload.FamilyName ?? hrEmployee.LastName,
                        Department = hrEmployee.Department,
                        Position = hrEmployee.Position,
                        Campus = hrEmployee.Campus,
                        ProfilePictureUrl = payload.Picture
                    });

                    // Assign role
                    var role = await _authRepository.GetRoleByNameAsync(AuthConstants.Roles.Employee);
                    if (role != null)
                    {
                        await _authRepository.AddUserRoleAsync(new UserRole
                        {
                            UserId = user.Id,
                            RoleId = role.Id
                        });
                    }

                    // Update HR record
                    hrEmployee.IsRegistered = true;
                    hrEmployee.UserId = user.Id;
                    hrEmployee.RegisteredAt = DateTime.UtcNow;
                    hrEmployee.IsEmailVerified = true;
                    await _authRepository.UpdateHREmployeeAsync(hrEmployee);
                }
                else
                {
                    // Update login info
                    user.GoogleId = payload.Subject;
                    user.LastLoginAt = DateTime.UtcNow;
                    user.LastLoginProvider = "Google";
                    await _authRepository.UpdateUserAsync(user);
                }

                // Check active status
                if (!user.IsActive)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Tài khoản đã bị vô hiệu hóa"
                    };
                }

                // Generate tokens
                var roles = await _authRepository.GetUserRolesAsync(user.Id);
                var accessToken = _tokenService.GenerateAccessToken(user, roles);
                var refreshToken = _tokenService.GenerateRefreshToken();

                // Save refresh token
                await _authRepository.CreateRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = refreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7),
                    DeviceId = model.DeviceId ?? "Google"
                });

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Đăng nhập Google thành công",
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.UserProfile?.FullName ?? payload.Name,
                        ProfilePicture = user.UserProfile?.ProfilePictureUrl ?? payload.Picture,
                        Roles = roles,
                        IsEmailVerified = true,
                        Department = user.UserProfile?.Department,
                        Position = user.UserProfile?.Position
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in GoogleLoginAsync");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Lỗi xác thực Google",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> RegisterAsync(RegisterModel model)
        {
            try
            {
                // Check email exists
                if (await _authRepository.IsEmailExistsAsync(model.Email))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Email đã được sử dụng"
                    };
                }

                // Check HR Employee
                var hrEmployee = await _authRepository.GetHREmployeeByEmailAndCodeAsync(
                    model.Email,
                    model.EmployeeCode
                );

                if (hrEmployee == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Email hoặc mã nhân viên không tồn tại trong hệ thống HR"
                    };
                }

                if (hrEmployee.Status != AuthConstants.EmployeeStatus.Active)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Tài khoản nhân viên không hoạt động"
                    };
                }

                if (hrEmployee.IsRegistered)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Nhân viên này đã đăng ký tài khoản"
                    };
                }

                // Create user
                var user = new User
                {
                    Email = model.Email,
                    NormalizedEmail = model.Email.ToUpper(),
                    PasswordHash = PasswordHelper.HashPassword(model.Password),
                    HREmployeeId = hrEmployee.Id,
                    EmployeeCode = hrEmployee.EmployeeCode,
                    AccountType = AuthConstants.AccountTypes.Internal,
                    IsActive = true
                };

                user = await _authRepository.CreateUserAsync(user);

                // Create profile
                await _authRepository.CreateUserProfileAsync(new UserProfile
                {
                    UserId = user.Id,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    PhoneNumber = model.PhoneNumber,
                    Department = hrEmployee.Department,
                    Position = hrEmployee.Position,
                    Campus = hrEmployee.Campus
                });

                // Assign role
                var role = await _authRepository.GetRoleByNameAsync(AuthConstants.Roles.Employee);
                if (role != null)
                {
                    await _authRepository.AddUserRoleAsync(new UserRole
                    {
                        UserId = user.Id,
                        RoleId = role.Id
                    });
                }

                // Create verification token
                var verificationToken = new EmailVerificationToken
                {
                    UserId = user.Id,
                    Token = PasswordHelper.GenerateToken(),
                    Code = PasswordHelper.GenerateCode(),
                    Email = user.Email
                };

                await _authRepository.CreateEmailVerificationTokenAsync(verificationToken);

                // Update HR record
                hrEmployee.IsRegistered = true;
                hrEmployee.UserId = user.Id;
                hrEmployee.RegisteredAt = DateTime.UtcNow;
                await _authRepository.UpdateHREmployeeAsync(hrEmployee);

                // Send verification email
                await _emailService.SendVerificationEmailAsync(
                    user.Email,
                    verificationToken.Token,
                    verificationToken.Code
                );

                return new ServiceResult
                {
                    Success = true,
                    Message = "Đăng ký thành công. Vui lòng kiểm tra email để xác thực tài khoản"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RegisterAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordModel model)
        {
            try
            {
                var user = await _authRepository.GetUserByEmailAsync(model.Email);

                // Don't reveal if email exists
                if (user == null)
                {
                    return new ServiceResult
                    {
                        Success = true,
                        Message = "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu"
                    };
                }

                // Check if Google-only account
                if (string.IsNullOrEmpty(user.PasswordHash))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Tài khoản này sử dụng đăng nhập Google"
                    };
                }

                // Create reset token
                var resetToken = new PasswordResetToken
                {
                    UserId = user.Id,
                    Token = PasswordHelper.GenerateToken()
                };

                await _authRepository.CreatePasswordResetTokenAsync(resetToken);

                // Send email
                await _emailService.SendPasswordResetEmailAsync(
                    user.Email,
                    resetToken.Token,
                    resetToken.Code
                );

                return new ServiceResult
                {
                    Success = true,
                    Message = "Nếu email tồn tại, chúng tôi đã gửi hướng dẫn đặt lại mật khẩu"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ForgotPasswordAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> ResetPasswordAsync(ResetPasswordModel model)
        {
            try
            {
                var resetToken = await _authRepository.GetPasswordResetTokenAsync(
                    model.Token,
                    model.Code
                );

                if (resetToken == null || !resetToken.IsValid)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn"
                    };
                }

                // Update password
                var user = resetToken.User;
                user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
                user.FailedLoginAttempts = 0;
                user.IsLocked = false;
                user.LockedUntil = null;
                await _authRepository.UpdateUserAsync(user);

                // Mark token as used
                resetToken.IsUsed = true;
                resetToken.UsedAt = DateTime.UtcNow;
                await _authRepository.UpdatePasswordResetTokenAsync(resetToken);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Mật khẩu đã được đặt lại thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ResetPasswordAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> VerifyEmailAsync(VerifyEmailModel model)
        {
            try
            {
                var token = await _authRepository.GetEmailVerificationTokenAsync(
                    model.Token,
                    model.Code
                );

                if (token == null || !token.IsValid)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Token không hợp lệ hoặc đã hết hạn"
                    };
                }

                // Update user
                var user = token.User;
                user.IsEmailVerified = true;
                user.EmailVerifiedAt = DateTime.UtcNow;
                await _authRepository.UpdateUserAsync(user);

                // Mark token as used
                token.IsUsed = true;
                token.UsedAt = DateTime.UtcNow;
                await _authRepository.UpdateEmailVerificationTokenAsync(token);

                // Update HR if exists
                if (user.HREmployeeId.HasValue)
                {
                    var hrEmployee = await _authRepository.GetHREmployeeByEmailAsync(user.Email);
                    if (hrEmployee != null)
                    {
                        hrEmployee.IsEmailVerified = true;
                        hrEmployee.EmailVerifiedAt = DateTime.UtcNow;
                        await _authRepository.UpdateHREmployeeAsync(hrEmployee);
                    }
                }

                // Send welcome email
                await _emailService.SendWelcomeEmailAsync(
                    user.Email,
                    user.UserProfile?.FullName ?? user.Email
                );

                return new ServiceResult
                {
                    Success = true,
                    Message = "Email đã được xác thực thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in VerifyEmailAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenModel model)
        {
            try
            {
                // Validate refresh token
                var storedToken = await _authRepository.GetRefreshTokenAsync(model.RefreshToken);

                if (storedToken == null || !storedToken.IsActive)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Refresh token không hợp lệ"
                    };
                }

                // Get principal from expired token
                var principal = _tokenService.GetPrincipalFromExpiredToken(model.AccessToken);

                if (principal == null)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "Access token không hợp lệ"
                    };
                }

                // Get user
                var userId = Guid.Parse(principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "");
                var user = await _authRepository.GetUserByIdAsync(userId);

                if (user == null || !user.IsActive)
                {
                    return new AuthenticationResult
                    {
                        Success = false,
                        Message = "User không tồn tại hoặc đã bị vô hiệu hóa"
                    };
                }

                // Revoke old token
                storedToken.IsUsed = true;
                await _authRepository.UpdateRefreshTokenAsync(storedToken);

                // Generate new tokens
                var roles = await _authRepository.GetUserRolesAsync(user.Id);
                var newAccessToken = _tokenService.GenerateAccessToken(user, roles);
                var newRefreshToken = _tokenService.GenerateRefreshToken();

                // Save new refresh token
                await _authRepository.CreateRefreshTokenAsync(new RefreshToken
                {
                    UserId = user.Id,
                    Token = newRefreshToken,
                    JwtId = Guid.NewGuid().ToString(),
                    ExpiresAt = DateTime.UtcNow.AddDays(7)
                });

                return new AuthenticationResult
                {
                    Success = true,
                    Message = "Token được làm mới thành công",
                    AccessToken = newAccessToken,
                    RefreshToken = newRefreshToken,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(15),
                    User = new UserDto
                    {
                        Id = user.Id,
                        Email = user.Email,
                        FullName = user.UserProfile?.FullName,
                        ProfilePicture = user.UserProfile?.ProfilePictureUrl,
                        Roles = roles,
                        IsEmailVerified = user.IsEmailVerified
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in RefreshTokenAsync");
                return new AuthenticationResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> LogoutAsync(Guid userId, string token)
        {
            try
            {
                await _authRepository.RevokeAllUserTokensAsync(userId);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Đăng xuất thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in LogoutAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        public async Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordModel model)
        {
            try
            {
                var user = await _authRepository.GetUserByIdAsync(userId);

                if (user == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "User không tồn tại"
                    };
                }

                // Verify old password
                if (!PasswordHelper.VerifyPassword(model.OldPassword, user.PasswordHash))
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Mật khẩu cũ không đúng"
                    };
                }

                // Update password
                user.PasswordHash = PasswordHelper.HashPassword(model.NewPassword);
                await _authRepository.UpdateUserAsync(user);

                return new ServiceResult
                {
                    Success = true,
                    Message = "Đổi mật khẩu thành công"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in ChangePasswordAsync");
                return new ServiceResult
                {
                    Success = false,
                    Message = "Có lỗi xảy ra",
                    Errors = new List<string> { ex.Message }
                };
            }
        }
    }
}