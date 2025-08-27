using Application;
using Infratructure;
using Infratructure.Persistence;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Google;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using OfficeOpenXml; // ✅ EPPlus
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Load appsettings.json
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        var org = builder.Configuration["EPPlus:NonCommercial:Organization"];
        var user = builder.Configuration["EPPlus:NonCommercial:User"];

        if (!string.IsNullOrWhiteSpace(org))
        {
            ExcelPackage.License.SetNonCommercialOrganization(org);
        }
        else
        {
            ExcelPackage.License.SetNonCommercialPersonal(
                string.IsNullOrWhiteSpace(user) ? "NonCommercial User" : user
            );
        }
        // --- Services ---
        builder.Services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        builder.Services.AddEndpointsApiExplorer();

        // DB + DI
        builder.Services
            .AddDatabaseServices(builder.Configuration) // extension ở Infratructure.Persistence
            .AddInfratructure()
            .AddApplication();

        // --- Authentication ---
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            var jwtSecret = builder.Configuration["Jwt:Secret"];
            if (string.IsNullOrEmpty(jwtSecret))
            {
                throw new InvalidOperationException("JWT Secret is not configured in appsettings.json");
            }

            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = builder.Configuration["Jwt:Issuer"],
                ValidAudience = builder.Configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero
            };

            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    Console.WriteLine($"Authentication failed: {context.Exception.Message}");
                    return Task.CompletedTask;
                },
                OnTokenValidated = context =>
                {
                    Console.WriteLine("Token validated successfully");
                    return Task.CompletedTask;
                },
                OnMessageReceived = context =>
                {
                    var accessToken = context.Request.Query["access_token"];
                    if (string.IsNullOrEmpty(accessToken))
                    {
                        accessToken = context.Request.Cookies["accessToken"];
                    }
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        context.Token = accessToken!;
                    }
                    return Task.CompletedTask;
                }
            };
        })
        .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, opts =>
        {
            opts.LoginPath = "/signin-google";
            opts.SlidingExpiration = true;
        })
        .AddGoogle(googleOptions =>
        {
            googleOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
            googleOptions.ClientId = builder.Configuration["Authentication:Google:ClientId"];
            googleOptions.ClientSecret = builder.Configuration["Authentication:Google:ClientSecret"];
            googleOptions.CallbackPath = "/signin-google";
        });

        // --- CORS ---
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("AllowSpecificOrigin", policy =>
            {
                policy
                    .WithOrigins(
                    // Điền thêm origin FE nếu có, ví dụ:
                    // "https://your-frontend.example.com",
                    // "http://localhost:5173"
                    )
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials();
            });
        });

        // --- Swagger ---
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "BEESRS",
                Version = "v1.0"
            });

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                c.IncludeXmlComments(xmlPath);
            }

            // ✅ Chỉ cần nhập token, Swagger sẽ tự thêm "Authorization: Bearer <token>"
            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Nhập trực tiếp JWT, KHÔNG cần 'Bearer '",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
                    },
                    Array.Empty<string>()
                }
            });
        });

        // Tuỳ chọn time zone
        var timeZone = TimeZoneInfo.FindSystemTimeZoneById("SE Asia Standard Time");

        var app = builder.Build();

        // --- Middleware pipeline ---
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
                c.InjectJavascript("/swagger/custom-swagger.js"); // nếu có
            });
        }
        else
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API V1");
            });
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseCors("AllowSpecificOrigin");

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
