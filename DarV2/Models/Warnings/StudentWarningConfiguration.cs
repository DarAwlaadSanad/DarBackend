using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class StudentWarningConfiguration : IEntityTypeConfiguration<StudentWarning>
    {
        public void Configure(EntityTypeBuilder<StudentWarning> builder)
        {
            builder.HasKey(w => w.Id);

            builder.Property(w => w.Reason)
                .HasMaxLength(1000);

            builder.HasOne(w => w.Student)
                .WithMany(s => s.Warnings)
                .HasForeignKey(w => w.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(w => w.Group)
                .WithMany()
                .HasForeignKey(w => w.GroupId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(w => w.CreatedByUser)
                .WithMany()
                .HasForeignKey(w => w.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
