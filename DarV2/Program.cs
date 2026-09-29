using CloudinaryDotNet;
using DarV2.Context;
using DarV2.Models;
using DarV2.settings;
using DarV2.UnitofWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DarV2.Service;
using DarV2.Service.Finance;

namespace DarV2
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers(options =>
            {
                options.SuppressImplicitRequiredAttributeForNonNullableReferenceTypes = true;
            }).AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                options.JsonSerializerOptions.NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString;
                options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            });
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
            builder.Services.AddSignalR();
            //register DbContext
            builder.Services.AddDbContext<DarContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<DarContext>()
                .AddDefaultTokenProviders();
            builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Auth service
            builder.Services.AddScoped<IAuthService, AuthService>();
            // Group service
            builder.Services.AddScoped<IGroupService, GroupService>();
            // User service
            builder.Services.AddScoped<IUserService, UserService>();
            // Student service
            builder.Services.AddScoped<IStudentService, StudentService>();
            builder.Services.AddScoped<IStudentWarningService, StudentWarningService>();
            // Group schedule
            builder.Services.AddScoped<IGroupScheduleService, GroupScheduleService>();
            builder.Services.AddScoped<IFinanceService, FinanceService>();
            builder.Services.AddScoped<ICenterFinanceService, CenterFinanceService>();
            builder.Services.AddScoped<IExamService, ExamService>();
            builder.Services.AddScoped<ICompetitionService, CompetitionService>();
            
            // Attendance & Evaluation
            builder.Services.AddScoped<IAttendanceService, AttendanceService>();
            builder.Services.AddScoped<IEvaluationService, EvaluationService>();
            // FeePlan
            builder.Services.AddScoped<IFeePlanService, FeePlanService>();
            // AcademicYear
            builder.Services.AddScoped<IAcademicYearService, AcademicYearService>();
            // StudentFee
            builder.Services.AddScoped<IStudentFeeService, StudentFeeService>();
            // Memorization
            builder.Services.AddScoped<IMemorizationService, MemorizationService>();
            // Teacher Attendance
            builder.Services.AddScoped<ITeacherAttendanceService, TeacherAttendanceService>();
builder.Services.AddScoped<DarV2.Service.AttendanceLocation.IAttendanceLocationService, DarV2.Service.AttendanceLocation.AttendanceLocationService>();
            // Roles
            builder.Services.AddScoped<IRoleService, RoleService>();
            // Sessions
            builder.Services.AddScoped<ISessionService, SessionService>();
            builder.Services.AddScoped<DarV2.Service.Notification.INotificationService, DarV2.Service.Notification.NotificationService>();
            builder.Services.AddHostedService<DarV2.Service.Notification.SessionReminderBackgroundService>();
            // Finance
            builder.Services.AddScoped<DarV2.Service.Finance.IFinanceService, DarV2.Service.Finance.FinanceService>();
            // Export
            builder.Services.AddScoped<DarV2.Service.Export.IExportService, DarV2.Service.Export.ExportService>();
            // Rooms
            builder.Services.AddScoped<IRoomService, RoomService>();
            // Chat
            builder.Services.AddScoped<IChatService, ChatService>();
            // Book service
            builder.Services.AddScoped<DarV2.Service.Book.IBookService, DarV2.Service.Book.BookService>();

            builder.Services.Configure<CloudinarySettings>(
                builder.Configuration.GetSection("Cloudinary"));


            builder.Services.AddSingleton(provider =>
            {
                var config = provider.GetRequiredService<IOptions<CloudinarySettings>>().Value;
                return new Cloudinary(new Account(
                    config.CloudName,
                    config.ApiKey,
                    config.ApiSecret));
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy
                        .SetIsOriginAllowed(_ => true)
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // JWT Authentication
            var jwtSection = builder.Configuration.GetSection("Jwt");
            var key = jwtSection["Key"];
            if (!string.IsNullOrEmpty(key))
            {
                builder.Services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "JwtBearer";
                    options.DefaultChallengeScheme = "JwtBearer";
                })
                .AddJwtBearer("JwtBearer", options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSection["Issuer"],
                        ValidAudience = jwtSection["Audience"],
                        IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                            System.Text.Encoding.UTF8.GetBytes(key))
                    };
                    options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var accessToken = context.Request.Query["access_token"];
                            var path = context.HttpContext.Request.Path;
                            if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
                            {
                                context.Token = accessToken;
                            }
                            return Task.CompletedTask;
                        }
                    };
                });
            }

            builder.Services.AddAuthorization(options =>
            {
                foreach (var permission in Permissions.GetAllPermissions())
                {
                    options.AddPolicy(permission, policy =>
                        policy.RequireAssertion(ctx =>
                        {
                            if (ctx.User.IsInRole("Admin") || ctx.User.IsInRole("SuperAdmin")) return true;
                            if (ctx.User.HasClaim("Permission", permission)) return true;
                            if (permission.EndsWith(".View"))
                            {
                                var managePerm = permission.Substring(0, permission.Length - 5) + ".Manage";
                                if (ctx.User.HasClaim("Permission", managePerm)) return true;
                            }
                            return false;
                        }));
                }
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(c =>
                        c.SwaggerEndpoint("/openapi/v1.json", "Dar API V1"));
            }
            app.UseCors("AllowAll");
            app.UseHttpsRedirection();

            app.UseAuthentication();
            app.UseAuthorization();


            app.MapControllers();
            app.MapHub<DarV2.Hubs.NotificationHub>("/hubs/notifications");
            app.MapHub<DarV2.Hubs.ChatHub>("/hubs/chat");

            // Seed initial books if table is empty
            await DarV2.BookSeeder.SeedBooksAsync(app.Services);
            app.Run();
        }
    }
}

