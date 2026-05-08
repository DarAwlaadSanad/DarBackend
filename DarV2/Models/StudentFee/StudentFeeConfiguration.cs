using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class StudentFeeConfiguration : IEntityTypeConfiguration<StudentFee>
    {
        public void Configure(EntityTypeBuilder<StudentFee> builder)
        {
            builder.Property(x => x.RequiredAmount)
    .HasPrecision(18, 2);

            builder.Property(x => x.AmountPaid)
                .HasPrecision(18, 2);
        }
    }
}
