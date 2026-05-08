using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class SurahConfiguration : IEntityTypeConfiguration<Surah>
    {
        public void Configure(EntityTypeBuilder<Surah> builder)
        {
            builder.HasKey(s => s.Id);

            //builder.HasMany(s => s.Students)
            //       .WithOne(s => s.Surah)
            //       .HasForeignKey(s => s.SurahId)
            //       .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
