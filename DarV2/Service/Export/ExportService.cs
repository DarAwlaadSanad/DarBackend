using ClosedXML.Excel;
using DarV2.Models;
using DarV2.Service.Finance;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Linq;

namespace DarV2.Service.Export
{
    public class ExportService : IExportService
    {
        private readonly IUnitOfWork _uow;
        private readonly ITeacherAttendanceService _teacherAttendanceService;
        private readonly IFinanceService _financeService;
        private readonly IGroupService _groupService;
        private readonly ICenterFinanceService _centerFinanceService;
        private readonly Microsoft.Extensions.Configuration.IConfiguration _config;

        public ExportService(IUnitOfWork uow, ITeacherAttendanceService teacherAttendanceService, IFinanceService financeService, IGroupService groupService, ICenterFinanceService centerFinanceService, Microsoft.Extensions.Configuration.IConfiguration config)
        {
            _uow = uow;
            _teacherAttendanceService = teacherAttendanceService;
            _financeService = financeService;
            _groupService = groupService;
            _centerFinanceService = centerFinanceService;
            _config = config;
        }

        private void FormatWorksheet(IXLWorksheet ws, string title)
        {
            ws.RightToLeft = true;
            int colCount = ws.LastColumnUsed().ColumnNumber();
            int rowCount = ws.LastRowUsed().RowNumber();

            ws.Row(1).InsertRowsAbove(2);
            
            var titleRange = ws.Range(1, 1, 2, colCount);
            titleRange.Merge();
            titleRange.Value = title;
            titleRange.Style.Font.Bold = true;
            titleRange.Style.Font.FontSize = 18;
            titleRange.Style.Font.FontColor = XLColor.White;
            titleRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#2c3e50");
            titleRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            titleRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            titleRange.Style.Border.OutsideBorder = XLBorderStyleValues.Medium;

            var headerRange = ws.Range(3, 1, 3, colCount);
            headerRange.Style.Font.Bold = true;
            headerRange.Style.Font.FontSize = 12;
            headerRange.Style.Font.FontColor = XLColor.White;
            headerRange.Style.Fill.BackgroundColor = XLColor.FromHtml("#34495e");
            headerRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
            headerRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
            headerRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
            headerRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

            int newRowCount = rowCount + 2;
            if (newRowCount >= 4) 
            {
                var dataRange = ws.Range(4, 1, newRowCount, colCount);
                dataRange.Style.Font.FontSize = 11;
                dataRange.Style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
                dataRange.Style.Alignment.Vertical = XLAlignmentVerticalValues.Center;
                dataRange.Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
                dataRange.Style.Border.InsideBorder = XLBorderStyleValues.Thin;

                for (int i = 4; i <= newRowCount; i++)
                {
                    if (i % 2 == 0)
                        ws.Row(i).Style.Fill.BackgroundColor = XLColor.FromHtml("#f8f9fa");
                    else
                        ws.Row(i).Style.Fill.BackgroundColor = XLColor.White;
                }
            }
            
            ws.Columns().AdjustToContents();
            foreach(var col in ws.Columns(1, colCount))
            {
                col.Width += 2;
            }
        }

        public async Task<byte[]> ExportStudentsAsync(int? groupId = null)
        {
            var query = _uow.Students.Query().Include(s => s.AcademicYear).Include(s => s.Phones).AsQueryable();
            if (groupId.HasValue)
            {
                var groupStudents = await _uow.Students.GetStudentsGroupAsync(groupId.Value);
                var groupIds = groupStudents.Select(s => s.Id).ToList();
                query = query.Where(s => groupIds.Contains(s.Id));
            }
            
            var students = await query.ToListAsync();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("بيانات الطلاب");
            worksheet.RightToLeft = true;
            
            var headers = new[] { "الرقم", "الاسم", "الرقم القومي", "النوع", "السنة الدراسية", "رقم الهاتف الأساسي", "حالة الحساب" };
            for(int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            for (int i = 0; i < students.Count; i++)
            {
                var s = students[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = s.FullName;
                worksheet.Cell(i + 2, 3).Value = s.SSN;
                worksheet.Cell(i + 2, 4).Value = s.Gender == Gender.Male ? "ذَكَر" : (s.Gender == Gender.Female ? "أُنْثَى" : "غير محدد");
                worksheet.Cell(i + 2, 5).Value = s.AcademicYear?.Name ?? "غير محدد";
                worksheet.Cell(i + 2, 6).Value = s.Phones?.FirstOrDefault()?.Number ?? "";
                worksheet.Cell(i + 2, 7).Value = s.IsActive ? "نشط" : "غير نشط";
            }
            
            FormatWorksheet(worksheet, "بيانات الطلاب مسجلة بدار التحفيظ");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportGroupAttendanceAsync(int groupId, int month, int year)
        {
            var details = await _groupService.GetByIdAsync(groupId, month, year);
            if (details == null) return Array.Empty<byte>();

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("الغياب والتقييم");
            worksheet.RightToLeft = true;

            worksheet.Cell(1, 1).Value = "الرقم";
            worksheet.Cell(1, 2).Value = "اسم الطالب";
            
            int colIndex = 3;
            foreach(var session in details.Sessions)
            {
                worksheet.Cell(1, colIndex).Value = session.Date.ToString("yyyy/MM/dd");
                colIndex++;
            }
            worksheet.Cell(1, colIndex).Value = "إجمالي الحضور";

            for (int i = 0; i < details.Students.Count; i++)
            {
                var student = details.Students[i];
                worksheet.Cell(i + 2, 1).Value = i + 1;
                worksheet.Cell(i + 2, 2).Value = student.StudentName;

                int cIdx = 3;
                foreach(var session in details.Sessions)
                {
                    if (student.Records.TryGetValue(session.SessionId, out var record))
                    {
                        string status = record.Attendance switch
                        {
                            AttendanceStatus.Present => "حاضر",
                            AttendanceStatus.Absent => "غائب",
                            AttendanceStatus.Late => "متأخر",
                            AttendanceStatus.Excused => "مستأذن",
                            _ => ""
                        };
                        worksheet.Cell(i + 2, cIdx).Value = status;
                    }
                    cIdx++;
                }
                worksheet.Cell(i + 2, cIdx).Value = student.TotalPresent;
            }

            FormatWorksheet(worksheet, $"تقرير الغياب - {details.GroupName} - شهر {month}/{year}");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportTeacherAttendanceAsync(int month, int year)
        {
            var report = await _teacherAttendanceService.GetMonthlyReportAsync(year, month);
            
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("غياب المعلمين");
            worksheet.RightToLeft = true;

            var headers = new[] { "المعلم", "أيام الغياب", "دقائق التأخير", "الغياب (حصة)", "التأخير (حصة)" };
            for(int i = 0; i < headers.Length; i++)
            {
                worksheet.Cell(1, i + 1).Value = headers[i];
            }

            int row = 2;
            foreach (var item in report)
            {
                worksheet.Cell(row, 1).Value = item.TeacherName;
                worksheet.Cell(row, 2).Value = item.AbsentDays;
                worksheet.Cell(row, 3).Value = item.TotalLateMinutes;
                worksheet.Cell(row, 4).Value = item.AbsentSessions;
                worksheet.Cell(row, 5).Value = item.LateSessions;
                row++;
            }

            FormatWorksheet(worksheet, $"تقرير حضور وانصراف المعلمين - شهر {month}/{year}");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportMonthlyFinanceAsync(int month, int year)
        {
            var summary = await _centerFinanceService.GetMonthlySummaryAsync(month, year);
            var studentFees = await _uow.StudentFees.Query().Include(f => f.Student)
                .Where(f => f.PaymentDate.HasValue && f.PaymentDate.Value.Month == month && f.PaymentDate.Value.Year == year)
                .ToListAsync();
            var payroll = await _financeService.GenerateMonthlyPayrollAsync(month, year);

            using var workbook = new XLWorkbook();

            // 0. Summary Sheet
            var wsSummary = workbook.Worksheets.Add("الملخص");
            wsSummary.RightToLeft = true;
            wsSummary.Cell(1, 1).Value = "البند";
            wsSummary.Cell(1, 2).Value = "المبلغ";
            
            wsSummary.Cell(2, 1).Value = "إجمالي رسوم الطلاب";
            wsSummary.Cell(2, 2).Value = summary.TotalStudentFees;
            
            wsSummary.Cell(3, 1).Value = "إجمالي الإيرادات الأخرى (تبرعات)";
            wsSummary.Cell(3, 2).Value = summary.TotalOtherIncomes;
            
            wsSummary.Cell(4, 1).Value = "إجمالي الدخل";
            wsSummary.Cell(4, 2).Value = summary.TotalStudentFees + summary.TotalOtherIncomes;
            
            wsSummary.Cell(5, 1).Value = "إجمالي المصروفات العامة";
            wsSummary.Cell(5, 2).Value = summary.TotalCenterExpenses;
            
            wsSummary.Cell(6, 1).Value = "إجمالي رواتب الموظفين";
            wsSummary.Cell(6, 2).Value = summary.TotalSalaries;
            
            wsSummary.Cell(7, 1).Value = "صافي الربح / العجز";
            wsSummary.Cell(7, 2).Value = (summary.TotalStudentFees + summary.TotalOtherIncomes) - (summary.TotalCenterExpenses + summary.TotalSalaries);

            FormatWorksheet(wsSummary, $"ملخص التقرير المالي - شهر {month}/{year}");

            // 1. Student Fees Sheet
            var wsFees = workbook.Worksheets.Add("رسوم الطلاب");
            wsFees.RightToLeft = true;
            wsFees.Cell(1, 1).Value = "المبلغ المدفوع";
            wsFees.Cell(1, 2).Value = "الطالب";
            wsFees.Cell(1, 3).Value = "تاريخ الدفع";

            int row = 2;
            foreach(var f in studentFees)
            {
                wsFees.Cell(row, 1).Value = f.AmountPaid;
                wsFees.Cell(row, 2).Value = f.Student?.FullName;
                wsFees.Cell(row, 3).Value = f.PaymentDate?.ToString("yyyy/MM/dd");
                row++;
            }
            wsFees.Cell(row, 1).Value = studentFees.Sum(f => f.AmountPaid);
            wsFees.Cell(row, 2).Value = "الإجمالي";
            FormatWorksheet(wsFees, $"تقرير رسوم الطلاب - شهر {month}/{year}");

            // 2. Revenues Sheet (Donations/Other Incomes)
            var wsRevenues = workbook.Worksheets.Add("إيرادات وتبرعات أخرى");
            wsRevenues.RightToLeft = true;
            wsRevenues.Cell(1, 1).Value = "المبلغ";
            wsRevenues.Cell(1, 2).Value = "البيان";
            wsRevenues.Cell(1, 3).Value = "التاريخ";

            row = 2;
            foreach(var r in summary.IncomesBreakdown)
            {
                wsRevenues.Cell(row, 1).Value = r.Amount;
                wsRevenues.Cell(row, 2).Value = r.Title;
                wsRevenues.Cell(row, 3).Value = r.Date.ToString("yyyy/MM/dd");
                row++;
            }
            wsRevenues.Cell(row, 1).Value = summary.IncomesBreakdown.Sum(r => r.Amount);
            wsRevenues.Cell(row, 2).Value = "الإجمالي";
            FormatWorksheet(wsRevenues, $"تقرير إيرادات أخرى - شهر {month}/{year}");

            // 3. Expenses Sheet
            var wsExpenses = workbook.Worksheets.Add("المصروفات العامة");
            wsExpenses.RightToLeft = true;
            wsExpenses.Cell(1, 1).Value = "المبلغ";
            wsExpenses.Cell(1, 2).Value = "البيان";
            wsExpenses.Cell(1, 3).Value = "التاريخ";

            row = 2;
            foreach(var e in summary.ExpensesBreakdown)
            {
                wsExpenses.Cell(row, 1).Value = e.Amount;
                wsExpenses.Cell(row, 2).Value = e.Title;
                wsExpenses.Cell(row, 3).Value = e.Date.ToString("yyyy/MM/dd");
                row++;
            }
            wsExpenses.Cell(row, 1).Value = summary.ExpensesBreakdown.Sum(e => e.Amount);
            wsExpenses.Cell(row, 2).Value = "الإجمالي";
            FormatWorksheet(wsExpenses, $"تقرير مصروفات المركز - شهر {month}/{year}");

            // 4. Payroll Sheet
            var wsPayroll = workbook.Worksheets.Add("الرواتب");
            wsPayroll.RightToLeft = true;
            var prHeaders = new[] { "الاسم", "نوع الراتب", "الأساسي", "إضافات آلية", "خصومات آلية", "إضافات يدوية", "خصومات يدوية", "الصافي" };
            for(int i = 0; i < prHeaders.Length; i++)
            {
                wsPayroll.Cell(1, i + 1).Value = prHeaders[i];
            }

            row = 2;
            foreach(var p in payroll)
            {
                wsPayroll.Cell(row, 1).Value = p.UserName;
                wsPayroll.Cell(row, 2).Value = p.SalaryType == DarV2.Enum.SalaryType.FixedMonthly ? "ثابت" : "بالمجموعة";
                wsPayroll.Cell(row, 3).Value = p.BaseSalary;
                wsPayroll.Cell(row, 4).Value = p.AutomaticAdditions;
                wsPayroll.Cell(row, 5).Value = p.AutomaticDeductions;
                wsPayroll.Cell(row, 6).Value = p.ManualAdditions;
                wsPayroll.Cell(row, 7).Value = p.ManualDeductions;
                wsPayroll.Cell(row, 8).Value = p.NetSalary;
                row++;
            }
            wsPayroll.Cell(row, 1).Value = "الإجمالي";
            wsPayroll.Cell(row, 8).Value = payroll.Sum(p => p.NetSalary);
            FormatWorksheet(wsPayroll, $"تقرير رواتب الموظفين - شهر {month}/{year}");

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<byte[]> ExportEmptyTemplateAsync()
        {
            string connString = _config.GetConnectionString("DefaultConnection");
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Template.xlsx");

            var teachers = new System.Collections.Generic.List<dynamic>();
            var rooms = new System.Collections.Generic.List<dynamic>();
            var academicYears = new System.Collections.Generic.List<dynamic>();

            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connString))
            {
                await conn.OpenAsync();
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT u.Id, u.FullName FROM AspNetUsers u JOIN AspNetUserRoles ur ON u.Id = ur.UserId JOIN AspNetRoles r ON ur.RoleId = r.Id WHERE r.Name = 'Teacher'", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync()) teachers.Add(new { Id = reader.GetString(0), FullName = reader.IsDBNull(1) ? "" : reader.GetString(1) });

                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Name FROM Rooms", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync()) rooms.Add(new { Id = reader.GetInt32(0), Name = reader.IsDBNull(1) ? "" : reader.GetString(1) });

                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Name, TypeSchool FROM AcademicYears", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                    while (await reader.ReadAsync()) academicYears.Add(new { Id = reader.GetInt32(0), Name = reader.IsDBNull(1) ? "" : reader.GetString(1), TypeSchool = reader.GetInt32(2) });
            }

            using var workbook = new XLWorkbook(templatePath);
            var thabetSheet = workbook.Worksheet("ثابت");
            
            int row = 2;
            foreach (var t in teachers) { thabetSheet.Cell(row, 1).Value = $"{t.Id} - {t.FullName}"; row++; }

            row = 2;
            foreach (var r in rooms) { thabetSheet.Cell(row, 2).Value = $"{r.Id} - {r.Name}"; row++; }

            row = 2;
            foreach (var ay in academicYears) { 
                string tSchool = ay.TypeSchool == 0 ? "عام" : ay.TypeSchool == 1 ? "أزهري" : "أخرى";
                thabetSheet.Cell(row, 3).Value = $"{ay.Id} - {ay.Name} - {tSchool}"; row++; 
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }

        public async Task<byte[]> ExportGroupsDataAsync()
        {
            string connString = _config.GetConnectionString("DefaultConnection");
            string templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "Template.xlsx");

            var teachers = new System.Collections.Generic.List<dynamic>();
            var rooms = new System.Collections.Generic.List<dynamic>();
            var academicYears = new System.Collections.Generic.List<dynamic>();
            var groups = new System.Collections.Generic.List<dynamic>();
            var groupSchedules = new System.Collections.Generic.List<dynamic>();
            var studentGroups = new System.Collections.Generic.List<dynamic>();

            using (var conn = new Microsoft.Data.SqlClient.SqlConnection(connString))
            {
                await conn.OpenAsync();

                // 1. Teachers
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT u.Id, u.FullName FROM AspNetUsers u JOIN AspNetUserRoles ur ON u.Id = ur.UserId JOIN AspNetRoles r ON ur.RoleId = r.Id WHERE r.Name = 'Teacher'", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        teachers.Add(new { Id = reader.GetString(0), FullName = reader.IsDBNull(1) ? "" : reader.GetString(1) });
                }

                // 2. Rooms
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Name FROM Rooms", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        rooms.Add(new { Id = reader.GetInt32(0), Name = reader.IsDBNull(1) ? "" : reader.GetString(1) });
                }

                // 3. AcademicYears
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Name, TypeSchool FROM AcademicYears", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        academicYears.Add(new { Id = reader.GetInt32(0), Name = reader.IsDBNull(1) ? "" : reader.GetString(1), TypeSchool = reader.GetInt32(2) });
                }

                // 4. Groups
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT Id, Name, TeacherId, RoomId, Description FROM Groups", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        groups.Add(new { 
                            Id = reader.GetInt32(0), 
                            Name = reader.IsDBNull(1) ? "" : reader.GetString(1), 
                            TeacherId = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            RoomId = reader.IsDBNull(3) ? (int?)null : reader.GetInt32(3),
                            Description = reader.IsDBNull(4) ? "" : reader.GetString(4)
                        });
                }

                // 5. Schedules
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand("SELECT GroupId, DayOfWeek, StartTime, EndTime FROM GroupSchedules WHERE IsActive = 1", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                        groupSchedules.Add(new { 
                            GroupId = reader.GetInt32(0), 
                            DayOfWeek = reader.GetInt32(1), 
                            StartTime = reader.GetTimeSpan(2),
                            EndTime = reader.GetTimeSpan(3)
                        });
                }

                // 6. StudentGroups
                using (var cmd = new Microsoft.Data.SqlClient.SqlCommand(@"
                    SELECT sg.GroupId, s.Id, s.FullName, s.SSN, 
                           (SELECT TOP 1 p.Number FROM Phones p WHERE p.StudentId = s.Id) as Phone,
                           ay.Name as AcademicYearName, ay.TypeSchool
                    FROM StudentGroups sg
                    JOIN Students s ON sg.StudentId = s.Id
                    LEFT JOIN AcademicYears ay ON s.AcademicYearId = ay.Id
                ", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        string schoolType = "";
                        if (!reader.IsDBNull(6)) {
                            int t = reader.GetInt32(6);
                            schoolType = t == 0 ? "(عام)" : t == 1 ? "(أزهري)" : "(أخرى)";
                        }

                        studentGroups.Add(new { 
                            GroupId = reader.GetInt32(0), 
                            StudentId = reader.GetInt32(1),
                            FullName = reader.IsDBNull(2) ? "" : reader.GetString(2),
                            SSN = reader.IsDBNull(3) ? "" : reader.GetString(3),
                            Phone = reader.IsDBNull(4) ? "" : reader.GetString(4),
                            AcademicYear = (reader.IsDBNull(5) ? "" : reader.GetString(5)) + " " + schoolType
                        });
                    }
                }
            }

            using var workbook = new XLWorkbook(templatePath);
            var thabetSheet = workbook.Worksheet("ثابت");
            
            int row = 2;
            foreach (var t in teachers)
            {
                thabetSheet.Cell(row, 1).Value = $"{t.Id} - {t.FullName}";
                row++;
            }

            row = 2;
            foreach (var r in rooms)
            {
                thabetSheet.Cell(row, 2).Value = $"{r.Id} - {r.Name}";
                row++;
            }

            row = 2;
            foreach (var ay in academicYears)
            {
                string tSchool = ay.TypeSchool == 0 ? "عام" : ay.TypeSchool == 1 ? "أزهري" : "أخرى";
                thabetSheet.Cell(row, 3).Value = $"{ay.Id} - {ay.Name} - {tSchool}";
                row++;
            }

            var templateSheet = System.Linq.Enumerable.FirstOrDefault(workbook.Worksheets, w => w.Name != "ثابت");
            if (templateSheet == null) templateSheet = workbook.Worksheets.Add("ورقة1");

            string[] arabicDays = { "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

            foreach (var g in groups)
            {
                string safeName = g.Name.Replace("/", "_").Replace("\\", "_").Replace("?", "_").Replace("*", "_").Replace("[", "_").Replace("]", "_");
                if (safeName.Length > 31) safeName = safeName.Substring(0, 31);
                
                int sheetSuffix = 1;
                string finalName = safeName;
                while (workbook.TryGetWorksheet(finalName, out _))
                {
                    finalName = $"{safeName}_{sheetSuffix}";
                    sheetSuffix++;
                    if (finalName.Length > 31) finalName = finalName.Substring(0, 31);
                }

                var newSheet = templateSheet.CopyTo(finalName);
                
                var teacher = System.Linq.Enumerable.FirstOrDefault(teachers, t => t.Id == g.TeacherId);
                
                // Teacher
                newSheet.Cell("B1").Value = teacher != null ? teacher.FullName : "";
                newSheet.Cell("B1").Style.Font.FontColor = XLColor.Black;
                newSheet.Cell("C1").Value = teacher != null ? $"{teacher.Id} - {teacher.FullName}" : "";
                newSheet.Cell("C1").Style.Font.FontColor = XLColor.Black;

                // Group Name
                newSheet.Cell("E1").Value = g.Name;
                newSheet.Cell("E1").Style.Font.FontColor = XLColor.Black;

                // Room and Description (In Row 2)
                var room = System.Linq.Enumerable.FirstOrDefault(rooms, r => r.Id == g.RoomId);
                newSheet.Cell("I2").Value = room != null ? room.Name : "";
                newSheet.Cell("I2").Style.Font.FontColor = XLColor.Black;
                newSheet.Cell("J2").Value = g.Description;
                newSheet.Cell("J2").Style.Font.FontColor = XLColor.Black;

                // Make F2, G2, H2 Black (They already have "اليوم", "من", "الي")
                newSheet.Cell("F2").Style.Font.FontColor = XLColor.Black;
                newSheet.Cell("G2").Style.Font.FontColor = XLColor.Black;
                newSheet.Cell("H2").Style.Font.FontColor = XLColor.Black;

                var students = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(studentGroups, s => s.GroupId == g.Id));
                int rOut = 3;
                int stIndex = 1;
                foreach (var s in students)
                {
                    newSheet.Cell(rOut, 1).Value = stIndex++;
                    newSheet.Cell(rOut, 1).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 2).Value = s.FullName;
                    newSheet.Cell(rOut, 2).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 3).Value = "'" + s.SSN; 
                    newSheet.Cell(rOut, 3).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 4).Value = "'" + s.Phone;
                    newSheet.Cell(rOut, 4).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 5).Value = s.AcademicYear;
                    newSheet.Cell(rOut, 5).Style.Font.FontColor = XLColor.Black;
                    rOut++;
                }

                var schedules = System.Linq.Enumerable.ToList(System.Linq.Enumerable.Where(groupSchedules, s => s.GroupId == g.Id));
                rOut = 3;
                foreach (var s in schedules)
                {
                    newSheet.Cell(rOut, 6).Value = s.DayOfWeek >= 0 && s.DayOfWeek <= 6 ? arabicDays[s.DayOfWeek] : "";
                    newSheet.Cell(rOut, 6).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 7).Value = s.StartTime.ToString(@"hh\:mm");
                    newSheet.Cell(rOut, 7).Style.Font.FontColor = XLColor.Black;
                    
                    newSheet.Cell(rOut, 8).Value = s.EndTime.ToString(@"hh\:mm");
                    newSheet.Cell(rOut, 8).Style.Font.FontColor = XLColor.Black;
                    rOut++;
                }
            }

            templateSheet.Delete();

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            return stream.ToArray();
        }
        public async Task<bool> ImportGroupsAsync(Microsoft.AspNetCore.Http.IFormFile file)
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            using var workbook = new XLWorkbook(stream);

            var rooms = await _uow.Rooms.Query().ToListAsync();
            var academicYears = await _uow.AcademicYears.Query().ToListAsync();

            string[] arabicDays = { "الأحد", "الإثنين", "الثلاثاء", "الأربعاء", "الخميس", "الجمعة", "السبت" };

            await _uow.BeginTransactionAsync();
            try
            {
                foreach (var ws in workbook.Worksheets)
                {
                    if (ws.Name == "ثابت") continue;

                    // Teacher ID (Regex to find GUID from A1, B1 or C1)
                    string teacherCell = (ws.Cell("A1").GetString() + " " + ws.Cell("B1").GetString() + " " + ws.Cell("C1").GetString()).Trim();
                    string teacherId = null;
                    var guidMatch = System.Text.RegularExpressions.Regex.Match(teacherCell, @"[a-fA-F0-9]{8}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{4}-[a-fA-F0-9]{12}");
                    if (guidMatch.Success)
                    {
                        teacherId = guidMatch.Value;
                    }

                    // Group Name (From D1 or E1)
                    string groupName = ws.Cell("D1").GetString()?.Trim();
                    if (string.IsNullOrEmpty(groupName)) groupName = ws.Cell("E1").GetString()?.Trim();
                    if (string.IsNullOrEmpty(groupName)) groupName = ws.Name;

                    // Room ID (Extract number from I2)
                    string roomCell = ws.Cell("I2").GetString()?.Trim();
                    int? roomId = null;
                    if (!string.IsNullOrEmpty(roomCell))
                    {
                        var roomMatch = System.Text.RegularExpressions.Regex.Match(roomCell, @"^\d+");
                        if (roomMatch.Success && int.TryParse(roomMatch.Value, out int rId))
                        {
                            roomId = rId;
                        }
                        else
                        {
                            roomId = rooms.FirstOrDefault(r => r.Name == roomCell)?.Id;
                        }
                    }

                    string description = ws.Cell("J2").GetString()?.Trim();

                    var group = await _uow.Groups.Query()
                        .Include(g => g.StudentGroups)
                        .Include(g => g.Schedules)
                        .FirstOrDefaultAsync(g => g.Name == groupName && g.TeacherId == teacherId);

                    bool isNewGroup = false;
                    if (group == null)
                    {
                        isNewGroup = true;
                        group = new Group
                        {
                            Name = groupName,
                            TeacherId = teacherId,
                            RoomId = roomId,
                            Description = description,
                            IsOnline = roomId == null,
                            Schedules = new List<GroupSchedule>(),
                            StudentGroups = new List<StudentGroup>()
                        };
                        await _uow.Groups.AddAsync(group);
                    }
                    else
                    {
                        group.RoomId = roomId;
                        group.Description = description;
                        group.IsOnline = roomId == null;
                    }

                    // Schedules (From F2, G2, H2 onwards)
                    int r = 2;
                    while (!ws.Cell(r, 6).IsEmpty() || !ws.Cell(r, 7).IsEmpty())
                    {
                        string dayStr = ws.Cell(r, 6).GetString()?.Trim();
                        if (!string.IsNullOrEmpty(dayStr)) {
                            dayStr = dayStr.Replace("الاثنين", "الإثنين").Replace("الاربعاء", "الأربعاء").Replace("الاحد", "الأحد");
                        }
                        int dayIdx = !string.IsNullOrEmpty(dayStr) ? Array.IndexOf(arabicDays, dayStr) : -1;
                        
                        if (dayIdx >= 0)
                        {
                            string startStr = ws.Cell(r, 7).GetString()?.Trim();
                            string endStr = ws.Cell(r, 8).GetString()?.Trim();

                            TimeSpan start = TimeSpan.Zero, end = TimeSpan.Zero;
                            
                            bool startParsed = false;
                            if (double.TryParse(startStr, out double sh)) { start = TimeSpan.FromHours(sh); startParsed = true; }
                            else if (TimeSpan.TryParse(startStr, out start)) { startParsed = true; }

                            bool endParsed = false;
                            if (double.TryParse(endStr, out double eh)) { end = TimeSpan.FromHours(eh); endParsed = true; }
                            else if (TimeSpan.TryParse(endStr, out end)) { endParsed = true; }

                            if (startParsed && endParsed)
                            {
                                if (!group.Schedules.Any(s => s.DayOfWeek == (DarV2.Models.DayOfWeekAr)dayIdx && s.StartTime == start && s.EndTime == end))
                                {
                                    group.Schedules.Add(new GroupSchedule
                                    {
                                        DayOfWeek = (DarV2.Models.DayOfWeekAr)dayIdx,
                                        StartTime = start,
                                        EndTime = end,
                                        EffectiveFrom = DateOnly.FromDateTime(DateTime.Today),
                                        IsActive = true
                                    });
                                }
                            }
                        }
                        r++;
                    }

                    // Pre-fetch last code to generate new codes
                    var lastStudentCodeStr = await _uow.Students.Query()
                        .Where(s => s.Code != null && s.Code.StartsWith("STD-"))
                        .OrderByDescending(s => s.Id)
                        .Select(s => s.Code)
                        .FirstOrDefaultAsync();

                    int nextCodeNumber = 1;
                    if (lastStudentCodeStr != null && lastStudentCodeStr.Length >= 8)
                    {
                        if (int.TryParse(lastStudentCodeStr.Substring(4), out int lastNum))
                        {
                            nextCodeNumber = lastNum + 1;
                        }
                    }

                    // Students (From Row 3 onwards)
                    r = 3;
                    while (!ws.Cell(r, 2).IsEmpty())
                    {
                        string studentName = ws.Cell(r, 2).GetString()?.Trim();
                        string ssnStr = ws.Cell(r, 3).GetString()?.Replace("'", "")?.Trim();
                        string phoneStr = ws.Cell(r, 4).GetString()?.Replace("'", "")?.Trim();
                        string academicYearStr = ws.Cell(r, 5).GetString()?.Trim();

                        if (string.IsNullOrEmpty(studentName)) { r++; continue; }

                        int? acaYearId = null;
                        if (!string.IsNullOrEmpty(academicYearStr))
                        {
                            int typeSchool = 0;
                            if (academicYearStr.Contains("أزهري") || academicYearStr.Contains("ازهري")) typeSchool = 1;
                            else if (academicYearStr.Contains("أخرى") || academicYearStr.Contains("اخرى")) typeSchool = 2;

                            var match = academicYears.FirstOrDefault(a => a.Name == academicYearStr.Trim() && (int)a.TypeSchool == typeSchool);
                            if (match == null)
                            {
                                match = academicYears.FirstOrDefault(a => academicYearStr.Contains(a.Name) && (int)a.TypeSchool == typeSchool);
                            }
                            if (match == null)
                            {
                                match = academicYears.FirstOrDefault(a => academicYearStr.Contains(a.Name));
                            }
                            acaYearId = match?.Id;
                        }

                        var existingStudent = await _uow.Students.Query()
                            .Include(s => s.Phones)
                            .FirstOrDefaultAsync(s => 
                            (!string.IsNullOrEmpty(ssnStr) && s.SSN == ssnStr) || 
                            (!string.IsNullOrEmpty(studentName) && s.FullName == studentName)
                        );

                        var phoneList = new List<string>();
                        if (!string.IsNullOrEmpty(phoneStr))
                        {
                            var parts = phoneStr.Split(new[] { '/', '-', ',' }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (var p in parts)
                            {
                                string cleanPhone = p.Trim();
                                if (!string.IsNullOrEmpty(cleanPhone)) phoneList.Add(cleanPhone);
                            }
                        }

                        if (existingStudent == null)
                        {
                            string newCode = $"STD-{nextCodeNumber:D4}";
                            nextCodeNumber++;

                            existingStudent = new Student
                            {
                                FullName = studentName,
                                Code = newCode,
                                SSN = ssnStr,
                                AcademicYearId = acaYearId,
                                Phones = new List<Phone>()
                            };
                            
                            foreach (var p in phoneList)
                            {
                                existingStudent.Phones.Add(new Phone { Number = p });
                            }

                            await _uow.Students.AddAsync(existingStudent);
                            await _uow.SaveAsync(); 
                        }
                        else
                        {
                            bool phonesUpdated = false;
                            foreach (var p in phoneList)
                            {
                                if (!existingStudent.Phones.Any(x => x.Number == p))
                                {
                                    existingStudent.Phones.Add(new Phone { Number = p });
                                    phonesUpdated = true;
                                }
                            }
                            if (phonesUpdated) await _uow.SaveAsync();
                        }

                        if (!group.StudentGroups.Any(sg => sg.StudentId == existingStudent.Id))
                        {
                            group.StudentGroups.Add(new StudentGroup
                            {
                                StudentId = existingStudent.Id
                            });
                        }

                        r++;
                    }
                }
                await _uow.SaveAsync();
                await _uow.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                await _uow.RollbackAsync();
                
                string errorMsg = ex.Message;
                var inner = ex.InnerException;
                while (inner != null)
                {
                    errorMsg += " -> " + inner.Message;
                    inner = inner.InnerException;
                }
                
                Console.WriteLine(errorMsg);
                throw new Exception($"Error importing group from sheet. Detail: {errorMsg}", ex);
            }
        }
    }
}
