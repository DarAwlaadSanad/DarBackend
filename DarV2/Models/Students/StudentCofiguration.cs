using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DarV2.Models
{
    public class StudentConfiguration : IEntityTypeConfiguration<Student>
    {
        public void Configure(EntityTypeBuilder<Student> builder)
        {
            builder.HasKey(s => s.Id);

            builder.Property(s => s.FullName)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(s => s.Notes)
                .HasMaxLength(500);

            builder.HasMany(s=>s.Phones)
                .WithOne(s=>s.Student)
                .HasForeignKey(s=>s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);
             
            builder.HasMany(s=>s.Images)
                .WithOne(s=>s.Student)
                .HasForeignKey(s=>s.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(x => x.AcademicYear)
                .WithMany(x => x.Students)
                .HasForeignKey(x => x.AcademicYearId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(s=>s.StudentFees)
                .WithOne(sf=>sf.Student)
                .HasForeignKey(sf=>sf.StudentId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(s=>s.MemorizationRecords)
                .WithOne(mr=>mr.Student)
                .HasForeignKey(mr=>mr.StudentId)
                .OnDelete(DeleteBehavior.Cascade);



            builder.HasIndex(s => s.SSN)
                .IsUnique();



        }
    }
}
