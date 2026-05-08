using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class FeePlanConfiguration : IEntityTypeConfiguration<FeePlan>
    {
        public void Configure(EntityTypeBuilder<FeePlan> builder)
        {
            builder.Property(x => x.Amount)
    .HasPrecision(18, 2);

            builder.HasKey(f => f.Id);
            builder.HasOne(f => f.Group)
                .WithMany(g => g.FeePlans)
                .HasForeignKey(f => f.GroupId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
