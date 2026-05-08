using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class SessionConfiguration : IEntityTypeConfiguration<Session>
    {
        public void Configure(EntityTypeBuilder<Session> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.SessionDate)
                .IsRequired();

            builder.Property(s => s.StartTime)
                .IsRequired();

            builder.Property(s => s.EndTime)
                .IsRequired();

            // Session → optional GroupSchedule source
            builder.HasOne(s => s.GroupSchedule)
                .WithMany()
                .HasForeignKey(s => s.GroupScheduleId)
                .OnDelete(DeleteBehavior.SetNull);

            // Session → Attendances
            builder.HasMany(s => s.Attendances)
                .WithOne(a => a.Session)
                .HasForeignKey(a => a.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // Session → Evaluations
            builder.HasMany(s => s.Evaluations)
                .WithOne(e => e.Session)
                .HasForeignKey(e => e.SessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // منع تكرار حصة بنفس التاريخ والوقت لنفس المجموعة
            builder.HasIndex(s => new { s.GroupId, s.SessionDate, s.StartTime })
                .IsUnique();

            builder.HasIndex(s => s.SessionDate);
        }
    }

}
