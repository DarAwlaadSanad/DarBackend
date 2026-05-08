using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class GroupConfiguration : IEntityTypeConfiguration<Group>
    {
        public void Configure(EntityTypeBuilder<Group> builder)
        {
            builder.HasKey(g => g.Id);

            builder.Property(g => g.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(g => g.Description)
                .HasMaxLength(500);

            builder.HasOne(g => g.Teacher)
                .WithMany(t => t.Groups)
                .HasForeignKey(g => g.TeacherId)
                .OnDelete(DeleteBehavior.SetNull);

            // Group → Schedules
            builder.HasMany(g => g.Schedules)
                .WithOne(gs => gs.Group)
                .HasForeignKey(gs => gs.GroupId)
                .OnDelete(DeleteBehavior.Cascade);

            // Group → Sessions
            // Remove cascade delete to avoid SQL Server "multiple cascade paths" error.
            builder.HasMany(g => g.Sessions)
                .WithOne(s => s.Group)
                .HasForeignKey(s => s.GroupId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.HasMany(g=>g.StudentFees)
                .WithOne(g=>g.Group)
                .HasForeignKey(g=>g.GroupId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.HasIndex(g => g.TeacherId);
            builder.HasIndex(g => g.Name);
        }
    }
}
