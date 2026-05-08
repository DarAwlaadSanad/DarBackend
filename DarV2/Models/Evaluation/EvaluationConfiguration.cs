using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class EvaluationConfiguration : IEntityTypeConfiguration<Evaluation>
    {
        public void Configure(EntityTypeBuilder<Evaluation> builder)
        {
            builder.HasKey(e => e.Id);

            builder.Property(e => e.Score)
                .HasPrecision(5, 2);

            builder.Property(e => e.Comment)
                .HasMaxLength(500);

            
            builder.HasOne(e => e.Student)
                .WithMany(s => s.Evaluations)
                .HasForeignKey(e => e.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(e => new { e.SessionId, e.StudentId });
        }
    }

}
