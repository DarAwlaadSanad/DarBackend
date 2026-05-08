using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class AttendanceConfiguration : IEntityTypeConfiguration<Attendance>
    {
        public void Configure(EntityTypeBuilder<Attendance> builder)
        {
            builder.HasKey(a => a.Id);

            builder.Property(a => a.Status)
                .IsRequired();

            builder.Property(a => a.Notes)
                .HasMaxLength(300);

            builder.HasOne(a => a.Student)
                .WithMany(s => s.Attendances)
                .HasForeignKey(a => a.StudentId)
                .OnDelete(DeleteBehavior.Cascade);


            // طالب واحد - حضور واحد لكل حصة
            builder.HasIndex(a => new { a.SessionId, a.StudentId })
                .IsUnique();
        }
    }

}
