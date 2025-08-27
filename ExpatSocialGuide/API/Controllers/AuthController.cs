    using API.Response;
    using Application.Interface;
    using Infratructure.Models.Auth;
    using Infratructure.Models.Auth;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Mvc;
    using System;
    using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

    namespace API.Controllers
    {
        [Route("api/[controller]")]
        [ApiController]
        public class AuthController : ControllerBase
        {
            private readonly IAuthenService _authenService;
            private readonly IConfiguration _configuration;

            public AuthController(IAuthenService authenService,IConfiguration configuration)
            {
                _authenService = authenService;
            _configuration = configuration;
            }

            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                model.IpAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

                var result = await _authenService.LoginAsync(model);

                if (result.Success)
                {
                    var response = new AuthResponseDto
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        ExpiresAt = result.ExpiresAt.Value,
                        User = new UserInfoDto
                        {
                            Id = result.User.Id,
                            Email = result.User.Email,
                            FullName = result.User.FullName,
                            ProfilePicture = result.User.ProfilePicture,
                            Roles = result.User.Roles,
                            IsEmailVerified = result.User.IsEmailVerified,
                            Department = result.User.Department,
                            Position = result.User.Position
                        }
                    };

                    return Ok(ApiResponse<AuthResponseDto>.SuccessResult(response, result.Message));
                }

                return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }

            [HttpPost("google-login")]
            public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.GoogleLoginAsync(model);

                if (result.Success)
                {
                    var response = new AuthResponseDto
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        ExpiresAt = result.ExpiresAt.Value,
                        User = new UserInfoDto
                        {
                            Id = result.User.Id,
                            Email = result.User.Email,
                            FullName = result.User.FullName,
                            ProfilePicture = result.User.ProfilePicture,
                            Roles = result.User.Roles,
                            IsEmailVerified = result.User.IsEmailVerified,
                            Department = result.User.Department,
                            Position = result.User.Position
                        }
                    };

                    return Ok(ApiResponse<AuthResponseDto>.SuccessResult(response, result.Message));
                }

                return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }
        [HttpGet("google-signin")]
        public IActionResult GoogleSignIn()
        {
            var clientId = _configuration["Authentication:Google:ClientId"];
            var redirectUri = "https://localhost:7186/api/Auth/google-callback";

            var googleAuthUrl = $"https://accounts.google.com/o/oauth2/v2/auth?" +
                $"client_id={clientId}" +
                $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                $"&response_type=code" +
                $"&scope=openid%20email%20profile" +
                $"&access_type=offline";

            return Redirect(googleAuthUrl);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback(string code)
        {
            try
            {
                // Exchange code for tokens
                var client = new HttpClient();
                var tokenRequest = new Dictionary<string, string>
        {
            {"code", code},
            {"client_id", _configuration["Authentication:Google:ClientId"]},
            {"client_secret", _configuration["Authentication:Google:ClientSecret"]},
            {"redirect_uri", "https://localhost:7186/api/Auth/google-callback"},
            {"grant_type", "authorization_code"}
        };

                var tokenResponse = await client.PostAsync(
                    "https://oauth2.googleapis.com/token",
                    new FormUrlEncodedContent(tokenRequest)
                );

                var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
                var tokenData = JsonSerializer.Deserialize<JsonElement>(tokenContent);
                var idToken = tokenData.GetProperty("id_token").GetString();

                // Gọi service của bạn
                var result = await _authenService.GoogleLoginAsync(new GoogleLoginModel
                {
                    IdToken = idToken,
                    DeviceId = "web-browser"
                });

                if (result.Success)
                {
                    // Return tokens as JSON hoặc redirect
                    return Ok(new
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        User = result.User
                    });
                }

                return BadRequest(result.Message);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error: {ex.Message}");
            }
        }
        [HttpPost("register")]
            public async Task<IActionResult> Register([FromBody] RegisterModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.RegisterAsync(model);

                if (result.Success)
                {
                    return Ok(ApiResponse<object>.SuccessResult(result.Data, result.Message));
                }

                return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }

            [HttpPost("forgot-password")]
            public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.ForgotPasswordAsync(model);
                return Ok(ApiResponse<object>.SuccessResult(null, result.Message));
            }

            [HttpPost("reset-password")]
            public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.ResetPasswordAsync(model);

                if (result.Success)
                {
                    return Ok(ApiResponse<object>.SuccessResult(null, result.Message));
                }

                return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }

            [HttpPost("verify-email")]
            public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.VerifyEmailAsync(model);

                if (result.Success)
                {
                    return Ok(ApiResponse<object>.SuccessResult(null, result.Message));
                }

                return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }

            [HttpPost("refresh-token")]
            public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                var result = await _authenService.RefreshTokenAsync(model);

                if (result.Success)
                {
                    var response = new AuthResponseDto
                    {
                        AccessToken = result.AccessToken,
                        RefreshToken = result.RefreshToken,
                        ExpiresAt = result.ExpiresAt.Value,
                        User = new UserInfoDto
                        {
                            Id = result.User.Id,
                            Email = result.User.Email,
                            FullName = result.User.FullName,
                            ProfilePicture = result.User.ProfilePicture,
                            Roles = result.User.Roles,
                            IsEmailVerified = result.User.IsEmailVerified,
                            Department = result.User.Department,
                            Position = result.User.Position
                        }
                    };

                    return Ok(ApiResponse<AuthResponseDto>.SuccessResult(response, result.Message));
                }

                return Unauthorized(ApiResponse<object>.FailResult(result.Message, result.Errors));
            }

            [HttpPost("logout")]
            [Authorize]
            public async Task<IActionResult> Logout()
            {
                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        return BadRequest(ApiResponse<object>.FailResult("User not found"));
                    }

                    var userId = Guid.Parse(userIdClaim);
                    var authHeader = Request.Headers["Authorization"].ToString();
                    var token = authHeader.Replace("Bearer ", "");

                    var result = await _authenService.LogoutAsync(userId, token);

                    if (result.Success)
                    {
                        return Ok(ApiResponse<object>.SuccessResult(null, result.Message));
                    }

                    return BadRequest(ApiResponse<object>.FailResult(result.Message));
                }
                catch (Exception ex)
                {
                    return BadRequest(ApiResponse<object>.FailResult($"Logout failed: {ex.Message}"));
                }
            }

            [HttpPost("change-password")]
            [Authorize]
            public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ApiResponse<object>.FailResult("Invalid model", ModelState));
                }

                try
                {
                    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (string.IsNullOrEmpty(userIdClaim))
                    {
                        return BadRequest(ApiResponse<object>.FailResult("User not found"));
                    }

                    var userId = Guid.Parse(userIdClaim);

                    var result = await _authenService.ChangePasswordAsync(userId, model);

                    if (result.Success)
                    {
                        return Ok(ApiResponse<object>.SuccessResult(null, result.Message));
                    }

                    return BadRequest(ApiResponse<object>.FailResult(result.Message, result.Errors));
                }
                catch (Exception ex)
                {
                    return BadRequest(ApiResponse<object>.FailResult($"Change password failed: {ex.Message}"));
                }
            }
        }
    }