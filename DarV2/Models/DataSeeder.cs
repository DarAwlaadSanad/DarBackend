using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Models
{
    public static class DataSeeder
    {
        public static void SeedRole(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "1",
                    Name = "Admin",
                    NormalizedName = "ADMIN"
                },
                new IdentityRole
                {
                    Id = "2",
                    Name = "Teacher",
                    NormalizedName = "TEACHER"
                }
            );
        }

        public static void SeedRoleClaims(this ModelBuilder modelBuilder)
        {
            int adminClaimId = 1;
            
            // 1. Admin Claims
            var adminClaims = Permissions.GetAllPermissions();
            foreach(var p in adminClaims)
            {
                modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string> { Id = adminClaimId++, RoleId = "1", ClaimType = "Permission", ClaimValue = p }
                );
            }
            
            // 2. Teacher Claims
            int teacherClaimId = 1000;
            var teacherClaims = new List<string> 
            { 
                Permissions.ViewStudents, 
                Permissions.ViewGroups, 
                Permissions.ViewAttendance, 
                Permissions.ViewTeacherDashboard 
            };
            foreach(var p in teacherClaims)
            {
                 modelBuilder.Entity<IdentityRoleClaim<string>>().HasData(
                    new IdentityRoleClaim<string> { Id = teacherClaimId++, RoleId = "2", ClaimType = "Permission", ClaimValue = p }
                );
            }
        }

        public static void SeedAcademicYear(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<AcademicYear>().HasData(
                new AcademicYear
                {
                    Id = 1,
                    Name = "الاول الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 2,
                    Name = "الاول الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 3,
                    Name = "الثاني الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 4,
                    Name = "الثاني الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 5,
                    Name = "الثالث الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 6,
                    Name = "الثالث الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 7,
                    Name = "الرابع الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 8,
                    Name = "الرابع الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 9,
                    Name = "الخامس الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 10,
                    Name = "الخامس الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 11,
                    Name = "السادس الابتدائي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 12,
                    Name = "السادس الابتدائي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 13,
                    Name = "الاول الاعدادي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 14,
                    Name = "الاول الاعدادي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 15,
                    Name = "الثاني الاعدادي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 16,
                    Name = "الثاني الاعدادي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 17,
                    Name = "الثالث الاعدادي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 18,
                    Name = "الثالث الاعدادي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 19,
                    Name = "الاول الثانوي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 20,
                    Name = "الاول الثانوي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 21,
                    Name = "الثاني الثانوي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 22,
                    Name = "الثاني الثانوي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 23,
                    Name = "الثالث الثانوي",
                    TypeSchool = TypeSchool.Public
                },
                new AcademicYear
                {
                    Id = 24,
                    Name = "الثالث الثانوي",
                    TypeSchool = TypeSchool.Azhar
                },
                new AcademicYear
                {
                    Id = 25,
                    Name = "غير ذلك",
                    TypeSchool = TypeSchool.Another
                }
            );
        }

        public static void SeedSurah(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Surah>().HasData(
                new Surah { Id = 1, Name = "الفاتحة", TotalAyahs = 7 },
                new Surah { Id = 2, Name = "البقرة", TotalAyahs = 286 },
                new Surah { Id = 3, Name = "آل عمران", TotalAyahs = 200 },
                new Surah { Id = 4, Name = "النساء", TotalAyahs = 176 },
                new Surah { Id = 5, Name = "المائدة", TotalAyahs = 120 },
                new Surah { Id = 6, Name = "الأنعام", TotalAyahs = 165 },
                new Surah { Id = 7, Name = "الأعراف", TotalAyahs = 206 },
                new Surah { Id = 8, Name = "الأنفال", TotalAyahs = 75 },
                new Surah { Id = 9, Name = "التوبة", TotalAyahs = 129 },
                new Surah { Id = 10, Name = "يونس", TotalAyahs = 109 },
                new Surah { Id = 11, Name = "هود", TotalAyahs = 123 },
                new Surah { Id = 12, Name = "يوسف", TotalAyahs = 111 },
                new Surah { Id = 13, Name = "الرعد", TotalAyahs = 43 },
                new Surah { Id = 14, Name = "إبراهيم", TotalAyahs = 52 },
                new Surah { Id = 15, Name = "الحجر", TotalAyahs = 99 },
                new Surah { Id = 16, Name = "النحل", TotalAyahs = 128 },
                new Surah { Id = 17, Name = "الإسراء", TotalAyahs = 111 },
                new Surah { Id = 18, Name = "الكهف", TotalAyahs = 110 },
                new Surah { Id = 19, Name = "مريم", TotalAyahs = 98 },
                new Surah { Id = 20, Name = "طه", TotalAyahs = 135 },
                new Surah { Id = 21, Name = "الأنبياء", TotalAyahs = 112 },
                new Surah { Id = 22, Name = "الحج", TotalAyahs = 78 },
                new Surah { Id = 23, Name = "المؤمنون", TotalAyahs = 118 },
                new Surah { Id = 24, Name = "النور", TotalAyahs = 64 },
                new Surah { Id = 25, Name = "الفرقان", TotalAyahs = 77 },
                new Surah { Id = 26, Name = "الشعراء", TotalAyahs = 227 },
                new Surah { Id = 27, Name = "النمل", TotalAyahs = 93 },
                new Surah { Id = 28, Name = "القصص", TotalAyahs = 88 },
                new Surah { Id = 29, Name = "العنكبوت", TotalAyahs = 69 },
                new Surah { Id = 30, Name = "الروم", TotalAyahs = 60 },
                new Surah { Id = 31, Name = "لقمان", TotalAyahs = 34 },
                new Surah { Id = 32, Name = "السجدة", TotalAyahs = 30 },
                new Surah { Id = 33, Name = "الأحزاب", TotalAyahs = 73 },
                new Surah { Id = 34, Name = "سبأ", TotalAyahs = 54 },
                new Surah { Id = 35, Name = "فاطر", TotalAyahs = 45 },
                new Surah { Id = 36, Name = "يس", TotalAyahs = 83 },
                new Surah { Id = 37, Name = "الصافات", TotalAyahs = 182 },
                new Surah { Id = 38, Name = "ص", TotalAyahs = 88 },
                new Surah { Id = 39, Name = "الزمر", TotalAyahs = 75 },
                new Surah { Id = 40, Name = "غافر", TotalAyahs = 85 },
                new Surah { Id = 41, Name = "فصلت", TotalAyahs = 54 },
                new Surah { Id = 42, Name = "الشورى", TotalAyahs = 53 },
                new Surah { Id = 43, Name = "الزخرف", TotalAyahs = 89 },
                new Surah { Id = 44, Name = "الدخان", TotalAyahs = 59 },
                new Surah { Id = 45, Name = "الجاثية", TotalAyahs = 37 },
                new Surah { Id = 46, Name = "الأحقاف", TotalAyahs = 35 },
                new Surah { Id = 47, Name = "محمد", TotalAyahs = 38 },
                new Surah { Id = 48, Name = "الفتح", TotalAyahs = 29 },
                new Surah { Id = 49, Name = "الحجرات", TotalAyahs = 18 },
                new Surah { Id = 50, Name = "ق", TotalAyahs = 45 },
                new Surah { Id = 51, Name = "الذاريات", TotalAyahs = 60 },
                new Surah { Id = 52, Name = "الطور", TotalAyahs = 49 },
                new Surah { Id = 53, Name = "النجم", TotalAyahs = 62 },
                new Surah { Id = 54, Name = "القمر", TotalAyahs = 55 },
                new Surah { Id = 55, Name = "الرحمن", TotalAyahs = 78 },
                new Surah { Id = 56, Name = "الواقعة", TotalAyahs = 96 },
                new Surah { Id = 57, Name = "الحديد", TotalAyahs = 29 },
                new Surah { Id = 58, Name = "المجادلة", TotalAyahs = 22 },
                new Surah { Id = 59, Name = "الحشر", TotalAyahs = 24 },
                new Surah { Id = 60, Name = "الممتحنة", TotalAyahs = 13 },
                new Surah { Id = 61, Name = "الصف", TotalAyahs = 14 },
                new Surah { Id = 62, Name = "الجمعة", TotalAyahs = 11 },
                new Surah { Id = 63, Name = "المنافقون", TotalAyahs = 11 },
                new Surah { Id = 64, Name = "التغابن", TotalAyahs = 18 },
                new Surah { Id = 65, Name = "الطلاق", TotalAyahs = 12 },
                new Surah { Id = 66, Name = "التحريم", TotalAyahs = 12 },
                new Surah { Id = 67, Name = "الملك", TotalAyahs = 30 },
                new Surah { Id = 68, Name = "القلم", TotalAyahs = 52 },
                new Surah { Id = 69, Name = "الحاقة", TotalAyahs = 52 },
                new Surah { Id = 70, Name = "المعارج", TotalAyahs = 44 },
                new Surah { Id = 71, Name = "نوح", TotalAyahs = 28 },
                new Surah { Id = 72, Name = "الجن", TotalAyahs = 28 },
                new Surah { Id = 73, Name = "المزمل", TotalAyahs = 20 },
                new Surah { Id = 74, Name = "المدثر", TotalAyahs = 56 },
                new Surah { Id = 75, Name = "القيامة", TotalAyahs = 40 },
                new Surah { Id = 76, Name = "الإنسان", TotalAyahs = 31 },
                new Surah { Id = 77, Name = "المرسلات", TotalAyahs = 50 },
                new Surah { Id = 78, Name = "النبأ", TotalAyahs = 40 },
                new Surah { Id = 79, Name = "النازعات", TotalAyahs = 46 },
                new Surah { Id = 80, Name = "عبس", TotalAyahs = 42 },
                new Surah { Id = 81, Name = "التكوير", TotalAyahs = 29 },
                new Surah { Id = 82, Name = "الانفطار", TotalAyahs = 19 },
                new Surah { Id = 83, Name = "المطففين", TotalAyahs = 36 },
                new Surah { Id = 84, Name = "الانشقاق", TotalAyahs = 25 },
                new Surah { Id = 85, Name = "البروج", TotalAyahs = 22 },
                new Surah { Id = 86, Name = "الطارق", TotalAyahs = 17 },
                new Surah { Id = 87, Name = "الأعلى", TotalAyahs = 19 },
                new Surah { Id = 88, Name = "الغاشية", TotalAyahs = 26 },
                new Surah { Id = 89, Name = "الفجر", TotalAyahs = 30 },
                new Surah { Id = 90, Name = "البلد", TotalAyahs = 20 },
                new Surah { Id = 91, Name = "الشمس", TotalAyahs = 15 },
                new Surah { Id = 92, Name = "الليل", TotalAyahs = 21 },
                new Surah { Id = 93, Name = "الضحى", TotalAyahs = 11 },
                new Surah { Id = 94, Name = "الشرح", TotalAyahs = 8 },
                new Surah { Id = 95, Name = "التين", TotalAyahs = 8 },
                new Surah { Id = 96, Name = "العلق", TotalAyahs = 19 },
                new Surah { Id = 97, Name = "القدر", TotalAyahs = 5 },
                new Surah { Id = 98, Name = "البينة", TotalAyahs = 8 },
                new Surah { Id = 99, Name = "الزلزلة", TotalAyahs = 8 },
                new Surah { Id = 100, Name = "العاديات", TotalAyahs = 11 },
                new Surah { Id = 101, Name = "القارعة", TotalAyahs = 11 },
                new Surah { Id = 102, Name = "التكاثر", TotalAyahs = 8 },
                new Surah { Id = 103, Name = "العصر", TotalAyahs = 3 },
                new Surah { Id = 104, Name = "الهمزة", TotalAyahs = 9 },
                new Surah { Id = 105, Name = "الفيل", TotalAyahs = 5 },
                new Surah { Id = 106, Name = "قريش", TotalAyahs = 4 },
                new Surah { Id = 107, Name = "الماعون", TotalAyahs = 7 },
                new Surah { Id = 108, Name = "الكوثر", TotalAyahs = 3 },
                new Surah { Id = 109, Name = "الكافرون", TotalAyahs = 6 },
                new Surah { Id = 110, Name = "النصر", TotalAyahs = 3 },
                new Surah { Id = 111, Name = "المسد", TotalAyahs = 5 },
                new Surah { Id = 112, Name = "الإخلاص", TotalAyahs = 4 },
                new Surah { Id = 113, Name = "الفلق", TotalAyahs = 5 },
                new Surah { Id = 114, Name = "الناس", TotalAyahs = 6 }
            );
        }
    }
}
