using DarV2.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Context
{
    public class DarContext : IdentityDbContext<ApplicationUser>
    {
        public DarContext(DbContextOptions<DarContext> options) : base(options) { }

        public DbSet<Student> Students { get; set; }
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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.SeedRole();
            builder.SeedAcademicYear();
            base.OnModelCreating(builder);
            builder.ApplyConfigurationsFromAssembly(typeof(DarContext).Assembly);

        }
    }

}
