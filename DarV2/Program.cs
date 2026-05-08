using CloudinaryDotNet;
using DarV2.Context;
using DarV2.Models;
using DarV2.settings;
using DarV2.UnitofWork;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using DarV2.Service;

namespace DarV2
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            builder.Services.AddOpenApi();
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
            // Group schedule
            builder.Services.AddScoped<IGroupScheduleService, GroupScheduleService>();
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
                        .AllowAnyOrigin()
                        .AllowAnyHeader()
                        .AllowAnyMethod();
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
                });
            }

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

            app.Run();
        }
    }
}
