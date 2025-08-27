using Infratructure.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interface
{
    public interface IAuthenService
    {
        Task<AuthenticationResult> LoginAsync(LoginModel model);
        Task<AuthenticationResult> GoogleLoginAsync(GoogleLoginModel model);
        Task<ServiceResult> RegisterAsync(RegisterModel model);
        Task<ServiceResult> ForgotPasswordAsync(ForgotPasswordModel model);
        Task<ServiceResult> ResetPasswordAsync(ResetPasswordModel model);
        Task<ServiceResult> VerifyEmailAsync(VerifyEmailModel model);
        Task<AuthenticationResult> RefreshTokenAsync(RefreshTokenModel model);
        Task<ServiceResult> LogoutAsync(Guid userId, string token);
        Task<ServiceResult> ChangePasswordAsync(Guid userId, ChangePasswordModel model);
    }
}
