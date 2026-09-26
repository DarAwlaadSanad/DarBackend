namespace DarV2.Models;
public enum DayOfWeekAr
{
    Sunday = 0,
    Monday = 1,
    Tuesday = 2,
    Wednesday = 3,
    Thursday = 4,
    Friday = 5,
    Saturday = 6,
}

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Excused = 3,   // غياب بعذر
    Late = 4       // حضور متأخر
}

public enum TypeSchool
{
    Public = 0,
    Azhar = 1,
    Another = 2
}

public enum Gender
{
    Male = 1,   // ذكر
    Female = 2  // أنثى
}
