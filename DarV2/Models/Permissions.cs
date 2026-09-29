using System.Reflection;

namespace DarV2.Models
{
    public static class Permissions
    {
        // â”€â”€ Students â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewStudents    = "Permissions.Students.View";
        public const string ManageStudents  = "Permissions.Students.Manage";
        public const string DeleteStudents  = "Permissions.Students.Delete";
        public const string ViewStudentPasswords = "Permissions.Students.ViewPasswords";

        // â”€â”€ Groups â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewGroups      = "Permissions.Groups.View";
        public const string ManageGroups    = "Permissions.Groups.Manage";
        public const string DeleteGroups    = "Permissions.Groups.Delete";

        // â”€â”€ Attendance & Sessions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewAttendance   = "Permissions.Attendance.View";
        public const string ManageAttendance = "Permissions.Attendance.Manage";
        public const string ManageSessions   = "Permissions.Sessions.Manage";

        // â”€â”€ Exams â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewExams        = "Permissions.Exams.View";
        public const string ManageExams      = "Permissions.Exams.Manage";

        // â”€â”€ Competitions â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewCompetitions   = "Permissions.Competitions.View";
        public const string ManageCompetitions = "Permissions.Competitions.Manage";

        // â”€â”€ Memorization â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewMemorization   = "Permissions.Memorization.View";
        public const string ManageMemorization = "Permissions.Memorization.Manage";

        // â”€â”€ Fees & Finance â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewFees           = "Permissions.Fees.View";
        public const string ManageFees         = "Permissions.Fees.Manage";
        public const string ExemptFees         = "Permissions.Fees.Exempt";
        public const string ViewFinance        = "Permissions.Finance.View";
        public const string ManageFinance      = "Permissions.Finance.Manage";

        // â”€â”€ Group Fees â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewGroupFees      = "Permissions.GroupFees.View";
        public const string ManageGroupFees    = "Permissions.GroupFees.Manage";

        // â”€â”€ Fee Plans â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewFeePlans       = "Permissions.FeePlans.View";
        public const string ManageFeePlans     = "Permissions.FeePlans.Manage";

        // â”€â”€ Schedules â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewSchedules      = "Permissions.Schedules.View";
        public const string ManageSchedules    = "Permissions.Schedules.Manage";

        // â”€â”€ Academic Years â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewAcademicYears    = "Permissions.AcademicYears.View";
        public const string ManageAcademicYears  = "Permissions.AcademicYears.Manage";

        // â”€â”€ Teacher Attendance â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewTeacherAttendance   = "Permissions.TeacherAttendance.View";
        public const string ManageTeacherAttendance = "Permissions.TeacherAttendance.Manage";
        public const string BypassAttendanceSessionRequirement = "Permissions.TeacherAttendance.BypassSessionRequirement";

        // â”€â”€ Reports & Export â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewReports  = "Permissions.Reports.View";
        public const string ExportData   = "Permissions.Reports.Export";
        public const string ImportData   = "Permissions.Reports.Import";

        // â”€â”€ Users â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewUsers    = "Permissions.Users.View";
        public const string ManageUsers  = "Permissions.Users.Manage";

        // â”€â”€ Roles â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewRoles    = "Permissions.Roles.View";
        public const string ManageRoles  = "Permissions.Roles.Manage";

        // â”€â”€ Teacher Dashboard â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewTeacherDashboard = "Permissions.TeacherDashboard.View";

        // ── Chat ──────────────────────────────────────────────────────────
        public const string ViewStaffChat       = "Permissions.Chat.ViewStaff";
        public const string ViewStudentChats    = "Permissions.Chat.ViewStudentChats";

        // ── Warnings ──────────────────────────────────────────────────────
        public const string ViewWarnings        = "Permissions.Warnings.View";
        public const string ManageWarnings      = "Permissions.Warnings.Manage";

        // â”€â”€ Library â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewLibrary         = "Permissions.Library.View";
        public const string ManageLibrary       = "Permissions.Library.Manage";

        public static List<string> GetAllPermissions()
        {
            var permissions = new List<string>();
            var fields = typeof(Permissions).GetFields(
                BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            foreach (var field in fields)
            {
                if (field.IsLiteral && !field.IsInitOnly && field.FieldType == typeof(string))
                {
                    var value = field.GetValue(null)?.ToString();
                    if (value != null && value != ViewRooms && value != ManageRooms)
                        permissions.Add(value);
                }
            }

            // Append Rooms at the end to keep IDs stable for EF Core seeding
            permissions.Add(ViewRooms);
            permissions.Add(ManageRooms);

            return permissions;
        }

        // â”€â”€ Rooms â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€
        public const string ViewRooms    = "Permissions.Rooms.View";
        public const string ManageRooms  = "Permissions.Rooms.Manage";

        /// <summary>
        /// Returns permissions grouped by module for UI display.
        /// </summary>
        public static Dictionary<string, List<string>> GetGroupedPermissions()
        {
            return new Dictionary<string, List<string>>
            {
                ["Ø§Ù„Ø·Ù„Ø§Ø¨"] = new() { ViewStudents, ManageStudents, DeleteStudents, ViewStudentPasswords },
                ["Ø§Ù„Ø­Ù„Ù‚Ø§Øª"] = new() { ViewGroups, ManageGroups, DeleteGroups },
                ["Ø§Ù„Ø­Ø¶ÙˆØ± ÙˆØ§Ù„Ø¬Ù„Ø³Ø§Øª"] = new() { ViewAttendance, ManageAttendance, ManageSessions },
                ["Ø§Ù„Ø§Ø®ØªØ¨Ø§Ø±Ø§Øª"] = new() { ViewExams, ManageExams },
                ["Ø§Ù„Ù…Ø³Ø§Ø¨Ù‚Ø§Øª"] = new() { ViewCompetitions, ManageCompetitions },
                ["Ø§Ù„Ø­ÙØ¸ ÙˆØ§Ù„Ù…Ø±Ø§Ø¬Ø¹Ø©"] = new() { ViewMemorization, ManageMemorization },
                ["Ø§Ù„Ø±Ø³ÙˆÙ… ÙˆØ§Ù„Ù…Ø§Ù„ÙŠØ©"] = new() { ViewFees, ManageFees, ExemptFees, ViewFinance, ManageFinance, ViewGroupFees, ManageGroupFees },
                ["Ø®Ø·Ø· Ø§Ù„Ø¯ÙØ¹"] = new() { ViewFeePlans, ManageFeePlans },
                ["Ø§Ù„Ù…ÙˆØ§Ø¹ÙŠØ¯"] = new() { ViewSchedules, ManageSchedules },
                ["Ø§Ù„Ø³Ù†ÙˆØ§Øª Ø§Ù„Ø¯Ø±Ø§Ø³ÙŠØ©"] = new() { ViewAcademicYears, ManageAcademicYears },
                ["Ø­Ø¶ÙˆØ± Ø§Ù„Ù…Ø¹Ù„Ù…ÙŠÙ†"] = new() { ViewTeacherAttendance, ManageTeacherAttendance, BypassAttendanceSessionRequirement },
                ["Ø§Ù„ØªÙ‚Ø§Ø±ÙŠØ± ÙˆØ§Ù„ØªØµØ¯ÙŠØ±"] = new() { ViewReports, ExportData, ImportData },
                ["Ø§Ù„Ù…Ø³ØªØ®Ø¯Ù…ÙˆÙ†"] = new() { ViewUsers, ManageUsers },
                ["Ø§Ù„Ø£Ø¯ÙˆØ§Ø±"] = new() { ViewRoles, ManageRoles },
                ["Ø§Ù„ØºØ±Ù"] = new() { ViewRooms, ManageRooms },
                ["Ù„ÙˆØ­Ø© ØªØ­ÙƒÙ… Ø§Ù„Ù…Ø¹Ù„Ù…"] = new() { ViewTeacherDashboard },
                ["المحادثات"] = new() { ViewStaffChat, ViewStudentChats },
                ["الإنذارات"] = new() { ViewWarnings, ManageWarnings }
            };
        }
    }
}



