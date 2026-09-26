using DarV2.Context;
using DarV2.DTOs.Finance;
using DarV2.Enum;
using DarV2.Models.Finance;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service.Finance
{
    public class FinanceService : IFinanceService
    {
        private readonly DarContext _context;

        public FinanceService(DarContext context)
        {
            _context = context;
        }

        public async Task<FinancialSettingDTO> GetSettingsAsync()
        {
            var settings = await _context.FinancialSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                return new FinancialSettingDTO { AbsenceSessionDeduction = 0, DelayDeductionAmount = 0 };
            }
            return new FinancialSettingDTO
            {
                AbsenceSessionDeduction = settings.AbsenceSessionDeduction,
                DelayDeductionAmount = settings.DelayDeductionAmount
            };
        }

        public async Task<bool> UpdateSettingsAsync(FinancialSettingDTO dto)
        {
            var settings = await _context.FinancialSettings.FirstOrDefaultAsync();
            if (settings == null)
            {
                settings = new FinancialSetting();
                _context.FinancialSettings.Add(settings);
            }
            settings.AbsenceSessionDeduction = dto.AbsenceSessionDeduction;
            settings.DelayDeductionAmount = dto.DelayDeductionAmount;
            
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<UserContractDTO>> GetAllContractsAsync()
        {
            return await _context.UserContracts
                .Include(c => c.User)
                .Select(c => new UserContractDTO
                {
                    Id = c.Id,
                    UserId = c.UserId,
                    UserName = c.User != null ? c.User.FullName : "Unknown",
                    Amount = c.Amount,
                    SalaryType = c.SalaryType
                }).ToListAsync();
        }

        public async Task<UserContractDTO?> GetContractAsync(string userId)
        {
            var contract = await _context.UserContracts
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            
            if (contract == null) return null;

            return new UserContractDTO
            {
                Id = contract.Id,
                UserId = contract.UserId,
                UserName = contract.User != null ? contract.User.FullName : "Unknown",
                Amount = contract.Amount,
                SalaryType = contract.SalaryType
            };
        }

        public async Task<bool> SetContractAsync(SetUserContractDTO dto)
        {
            var contract = await _context.UserContracts.FirstOrDefaultAsync(c => c.UserId == dto.UserId);
            if (contract == null)
            {
                contract = new UserContract { UserId = dto.UserId };
                _context.UserContracts.Add(contract);
            }
            contract.Amount = dto.Amount;
            contract.SalaryType = dto.SalaryType;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteContractAsync(string userId)
        {
            var contract = await _context.UserContracts.FirstOrDefaultAsync(c => c.UserId == userId);
            if (contract == null) return false;

            _context.UserContracts.Remove(contract);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<FinancialTransactionDTO>> GetTransactionsAsync(string userId, int month, int year)
        {
            return await _context.FinancialTransactions
                .Where(t => t.UserId == userId && t.TransactionDate.Month == month && t.TransactionDate.Year == year)
                .Select(t => new FinancialTransactionDTO
                {
                    Id = t.Id,
                    UserId = t.UserId,
                    Amount = t.Amount,
                    Reason = t.Reason,
                    TransactionDate = t.TransactionDate,
                    Type = t.Type
                }).ToListAsync();
        }

        public async Task<bool> AddTransactionAsync(AddFinancialTransactionDTO dto)
        {
            var transaction = new FinancialTransaction
            {
                UserId = dto.UserId,
                Amount = dto.Amount,
                Type = dto.Type,
                Reason = dto.Reason,
                TransactionDate = dto.TransactionDate
            };
            _context.FinancialTransactions.Add(transaction);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteTransactionAsync(int id)
        {
            var t = await _context.FinancialTransactions.FindAsync(id);
            if (t == null) return false;
            _context.FinancialTransactions.Remove(t);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<MonthlyPayrollReportDTO>> GenerateMonthlyPayrollAsync(int month, int year)
        {
            var usersWithContracts = await _context.UserContracts.Include(c => c.User).AsNoTracking().ToListAsync();
            var userIds = usersWithContracts.Select(c => c.UserId).ToList();

            var settings = await GetSettingsAsync();

            var transactions = await _context.FinancialTransactions
                .Where(t => userIds.Contains(t.UserId) && t.TransactionDate.Month == month && t.TransactionDate.Year == year)
                .AsNoTracking()
                .ToListAsync();

            var groupsCount = await _context.Groups
                .Where(g => g.TeacherId != null && userIds.Contains(g.TeacherId))
                .GroupBy(g => g.TeacherId)
                .Select(g => new { TeacherId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.TeacherId!, g => g.Count);

            var attendances = await _context.TeacherAttendances
                .Where(a => userIds.Contains(a.TeacherId) && a.Date.Month == month && a.Date.Year == year)
                .AsNoTracking()
                .ToListAsync();

            var substitutions = await _context.Sessions
                .Where(s => s.SubstituteTeacherId != null && userIds.Contains(s.SubstituteTeacherId) && s.SessionDate.Month == month && s.SessionDate.Year == year)
                .GroupBy(s => s.SubstituteTeacherId)
                .Select(g => new { TeacherId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.TeacherId!, g => g.Count);

            var payments = await _context.SalaryPayments
                .Where(p => p.Month == month && p.Year == year)
                .ToListAsync();
            var paymentsDict = payments.ToDictionary(p => p.UserId, p => true);

            var reports = new List<MonthlyPayrollReportDTO>();

            foreach(var contract in usersWithContracts)
            {
                if (contract.User == null) continue;

                var userId = contract.UserId;
                var report = new MonthlyPayrollReportDTO
                {
                    UserId = userId,
                    UserName = contract.User.FullName,
                    SalaryType = contract.SalaryType
                };

                var userTransactions = transactions.Where(t => t.UserId == userId).ToList();
                report.ManualAdditions = userTransactions.Where(t => t.Type == TransactionType.ManualBonus).Sum(t => t.Amount);
                report.ManualDeductions = userTransactions.Where(t => t.Type == TransactionType.ManualDeduction).Sum(t => Math.Abs(t.Amount));

                if (contract.SalaryType == SalaryType.FixedMonthly)
                {
                    report.BaseSalary = contract.Amount;

                    report.Substitutions = substitutions.GetValueOrDefault(userId, 0);
                    var subBonus = userTransactions.Where(t => t.Type == TransactionType.SubstituteBonus).Sum(t => t.Amount);
                    report.AutomaticAdditions = subBonus > 0 ? subBonus : (report.Substitutions * settings.AbsenceSessionDeduction);

                    report.Absences = attendances.Count(a => a.TeacherId == userId && a.IsAbsent);
                    var absenceDeductions = userTransactions.Where(t => t.Type == TransactionType.AbsenceDeduction).Sum(t => Math.Abs(t.Amount));
                    report.AutomaticDeductions = absenceDeductions > 0 ? absenceDeductions : (report.Absences * settings.AbsenceSessionDeduction);

                    report.DelayIncidents = attendances.Count(a => a.TeacherId == userId && a.DelayMinutes > 0);
                    report.AutomaticDeductions += report.DelayIncidents * settings.DelayDeductionAmount;

                    report.ManualAdditions = userTransactions.Where(t => t.Type == TransactionType.ManualBonus).Sum(t => t.Amount);
                    report.ManualDeductions = userTransactions.Where(t => t.Type == TransactionType.ManualDeduction).Sum(t => Math.Abs(t.Amount));

                    report.NetSalary = report.BaseSalary 
                                        + report.AutomaticAdditions 
                                        + report.ManualAdditions 
                                        - report.AutomaticDeductions 
                                        - report.ManualDeductions;
                    report.IsPaid = paymentsDict.GetValueOrDefault(userId, false);
                    reports.Add(report);
                    continue;
                }

                report.NumberOfGroups = groupsCount.GetValueOrDefault(userId, 0);
                report.BaseSalary = report.NumberOfGroups * contract.Amount;

                var userAttendances = attendances.Where(a => a.TeacherId == userId).ToList();
                report.DelayIncidents = userAttendances.Count(a => a.DelayMinutes > 0);
                report.AutomaticDeductions += report.DelayIncidents * settings.DelayDeductionAmount;

                report.Absences = userAttendances.Count(a => a.IsAbsent);
                report.AutomaticDeductions += report.Absences * settings.AbsenceSessionDeduction;

                report.Substitutions = substitutions.GetValueOrDefault(userId, 0);
                report.AutomaticAdditions += report.Substitutions * settings.AbsenceSessionDeduction;

                report.NetSalary = report.BaseSalary 
                                    + report.AutomaticAdditions 
                                    + report.ManualAdditions 
                                    - report.AutomaticDeductions 
                                    - report.ManualDeductions;
                                    
                report.IsPaid = paymentsDict.GetValueOrDefault(userId, false);

                reports.Add(report);
            }

            return reports;
        }

        public async Task<MonthlyPayrollReportDTO?> GenerateUserMonthlyPayrollAsync(string userId, int month, int year)
        {
            var contract = await _context.UserContracts.Include(c => c.User).FirstOrDefaultAsync(c => c.UserId == userId);
            if (contract == null || contract.User == null) return null;

            var settings = await GetSettingsAsync();
            var report = new MonthlyPayrollReportDTO
            {
                UserId = userId,
                UserName = contract.User.FullName,
                SalaryType = contract.SalaryType
            };

            // Transactions for this month
            var transactions = await _context.FinancialTransactions
                .Where(t => t.UserId == userId && t.TransactionDate.Month == month && t.TransactionDate.Year == year)
                .ToListAsync();

            report.ManualAdditions = transactions.Where(t => t.Type == TransactionType.ManualBonus).Sum(t => t.Amount);
            report.ManualDeductions = transactions.Where(t => t.Type == TransactionType.ManualDeduction).Sum(t => Math.Abs(t.Amount));

            if (contract.SalaryType == SalaryType.FixedMonthly)
            {
                report.BaseSalary = contract.Amount;

                report.Substitutions = await _context.Sessions.CountAsync(s => s.SubstituteTeacherId == userId && s.SessionDate.Month == month && s.SessionDate.Year == year);
                var subBonus = transactions.Where(t => t.Type == TransactionType.SubstituteBonus).Sum(t => t.Amount);
                report.AutomaticAdditions = subBonus > 0 ? subBonus : (report.Substitutions * settings.AbsenceSessionDeduction);

                report.Absences = await _context.TeacherAttendances.CountAsync(a => a.TeacherId == userId && a.IsAbsent && a.Date.Month == month && a.Date.Year == year);
                var absenceDeductions = transactions.Where(t => t.Type == TransactionType.AbsenceDeduction).Sum(t => Math.Abs(t.Amount));
                report.AutomaticDeductions = absenceDeductions > 0 ? absenceDeductions : (report.Absences * settings.AbsenceSessionDeduction);

                report.DelayIncidents = await _context.TeacherAttendances.CountAsync(a => a.TeacherId == userId && a.DelayMinutes > 0 && a.Date.Month == month && a.Date.Year == year);
                report.AutomaticDeductions += report.DelayIncidents * settings.DelayDeductionAmount;

                report.ManualAdditions = transactions.Where(t => t.Type == TransactionType.ManualBonus).Sum(t => t.Amount);
                report.ManualDeductions = transactions.Where(t => t.Type == TransactionType.ManualDeduction).Sum(t => Math.Abs(t.Amount));

                report.NetSalary = report.BaseSalary 
                                    + report.AutomaticAdditions 
                                    + report.ManualAdditions 
                                    - report.AutomaticDeductions 
                                    - report.ManualDeductions;
                return report;
            }

            // For Teachers
            report.NumberOfGroups = await _context.Groups.CountAsync(g => g.TeacherId == userId);
            report.BaseSalary = report.NumberOfGroups * contract.Amount;

            // Delays
            var attendancesThisMonth = await _context.TeacherAttendances
                .Where(a => a.TeacherId == userId && a.Date.Month == month && a.Date.Year == year)
                .ToListAsync();

            report.DelayIncidents = attendancesThisMonth.Count(a => a.DelayMinutes > 0);
            report.AutomaticDeductions += report.DelayIncidents * settings.DelayDeductionAmount;

            // Absences
            report.Absences = attendancesThisMonth.Count(a => a.IsAbsent);
            report.AutomaticDeductions += report.Absences * settings.AbsenceSessionDeduction;

            // Substitutions (sessions where they are SubstituteTeacher)
            var substitutionSessions = await _context.Sessions
                .Where(s => s.SubstituteTeacherId == userId && s.SessionDate.Month == month && s.SessionDate.Year == year)
                .CountAsync();
            
            report.Substitutions = substitutionSessions;
            report.AutomaticAdditions += report.Substitutions * settings.AbsenceSessionDeduction;

            report.NetSalary = report.BaseSalary 
                                + report.AutomaticAdditions 
                                + report.ManualAdditions 
                                - report.AutomaticDeductions 
                                - report.ManualDeductions;

            report.IsPaid = await _context.SalaryPayments.AnyAsync(p => p.UserId == userId && p.Month == month && p.Year == year);

            return report;
        }

        public async Task<bool> ToggleSalaryPaymentAsync(string userId, int month, int year, decimal amount)
        {
            var existingPayment = await _context.SalaryPayments
                .FirstOrDefaultAsync(p => p.UserId == userId && p.Month == month && p.Year == year);

            if (existingPayment != null)
            {
                // Mark as unpaid
                _context.SalaryPayments.Remove(existingPayment);
                await _context.SaveChangesAsync();
                return false; // Result is unpaid
            }
            else
            {
                // Mark as paid
                var payment = new SalaryPayment
                {
                    UserId = userId,
                    Month = month,
                    Year = year,
                    AmountPaid = amount,
                    PaidAt = DateTime.UtcNow
                };
                _context.SalaryPayments.Add(payment);
                await _context.SaveChangesAsync();
                return true; // Result is paid
            }
        }
    }
}
