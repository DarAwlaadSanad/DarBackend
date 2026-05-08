using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class GroupScheduleConfiguration : IEntityTypeConfiguration<GroupSchedule>
    {
        public void Configure(EntityTypeBuilder<GroupSchedule> builder)
        {
            builder.HasKey(gs => gs.Id);

            builder.Property(gs => gs.DayOfWeek)
                .IsRequired();

            builder.Property(gs => gs.StartTime)
                .IsRequired();

            builder.Property(gs => gs.EndTime)
                .IsRequired();

            builder.Property(gs => gs.EffectiveFrom)
                .IsRequired();

            // Index for quick lookup: all active schedules for a group
            builder.HasIndex(gs => new { gs.GroupId, gs.IsActive });
            builder.HasIndex(gs => new { gs.GroupId, gs.DayOfWeek });
        }
    }

}
