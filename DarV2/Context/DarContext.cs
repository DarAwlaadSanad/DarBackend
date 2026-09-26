using DarV2.Models;
using DarV2.Models.Finance;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Context
{
    public class DarContext : IdentityDbContext<ApplicationUser>
    {
        public DarContext(DbContextOptions<DarContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
        public DbSet<SalaryPayment> SalaryPayments { get; set; }
        public DbSet<Room> Rooms { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<GroupSchedule> GroupSchedules { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<StudentGroup> StudentGroups { get; set; }
        public DbSet<Attendance> Attendances { get; set; }
        public DbSet<Evaluation> Evaluations { get; set; }
        public DbSet<StudentFee> StudentFees { get; set; }
        public DbSet<FeePlan> FeePlans { get; set; } 
        public DbSet<AcademicYear> AcademicYears { get; set; }
        public DbSet<Phone> Phones { get; set; }
        public DbSet<Image> Images { get; set; }
        public DbSet<TeacherAttendance> TeacherAttendances { get; set; }
        public DbSet<Exam> Exams { get; set; }
        public DbSet<ExamResult> ExamResults { get; set; }
        public DbSet<Models.Finance.UserContract> UserContracts { get; set; }
        public DbSet<Models.Finance.FinancialSetting> FinancialSettings { get; set; }
        public DbSet<Models.Finance.FinancialTransaction> FinancialTransactions { get; set; }
        public DbSet<Models.Finance.CenterExpense> CenterExpenses { get; set; }
        public DbSet<Models.Finance.CenterIncome> CenterIncomes { get; set; }
        public DbSet<Competition> Competitions { get; set; }
        public DbSet<CompetitionLevel> CompetitionLevels { get; set; }
        public DbSet<CompetitionResult> CompetitionResults { get; set; }
        public DbSet<AppNotification> Notifications { get; set; }
        public DbSet<ChatRoom> ChatRooms { get; set; }
        public DbSet<ChatMessage> ChatMessages { get; set; }
        public DbSet<StudentWarning> StudentWarnings { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.SeedRole();
            builder.SeedRoleClaims();
            builder.SeedAcademicYear();
            
            builder.Entity<Group>()
                .HasOne(g => g.Room)
                .WithMany(r => r.Groups)
                .HasForeignKey(g => g.RoomId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<SalaryPayment>()
                .HasOne(s => s.User)
                .WithMany()
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(DarContext).Assembly);
        }

        protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
        {
            configurationBuilder.Properties<decimal>().HavePrecision(18, 2);
        }
    }
}
