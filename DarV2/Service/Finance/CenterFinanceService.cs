using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DarV2.Context;
using DarV2.DTOs.Finance;
using DarV2.Models.Finance;

namespace DarV2.Service.Finance
{
    public class CenterFinanceService : ICenterFinanceService
    {
        private readonly DarContext _context;
        private readonly IFinanceService _financeService;

        public CenterFinanceService(DarContext context, IFinanceService financeService)
        {
            _context = context;
            _financeService = financeService;
        }

        public async Task<List<CenterExpenseDTO>> GetExpensesAsync(int month, int year)
        {
            var expenses = await _context.CenterExpenses
                .Where(e => e.Date.Month == month && e.Date.Year == year)
                .OrderByDescending(e => e.Date)
                .ToListAsync();

            return expenses.Select(e => new CenterExpenseDTO
            {
                Id = e.Id,
                Title = e.Title,
                Amount = e.Amount,
                Category = e.Category,
                Date = e.Date,
                Notes = e.Notes
            }).ToList();
        }

        public async Task<CenterExpenseDTO> AddExpenseAsync(CreateCenterExpenseDTO dto)
        {
            var expense = new CenterExpense
            {
                Title = dto.Title,
                Amount = dto.Amount,
                Category = dto.Category,
                Date = dto.Date,
                Notes = dto.Notes
            };

            _context.CenterExpenses.Add(expense);
            await _context.SaveChangesAsync();

            return new CenterExpenseDTO
            {
                Id = expense.Id,
                Title = expense.Title,
                Amount = expense.Amount,
                Category = expense.Category,
                Date = expense.Date,
                Notes = expense.Notes
            };
        }

        public async Task DeleteExpenseAsync(int id)
        {
            var expense = await _context.CenterExpenses.FindAsync(id);
            if (expense != null)
            {
                _context.CenterExpenses.Remove(expense);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<List<CenterIncomeDTO>> GetIncomesAsync(int month, int year)
        {
            var incomes = await _context.CenterIncomes
                .Where(i => i.Date.Month == month && i.Date.Year == year)
                .OrderByDescending(i => i.Date)
                .ToListAsync();

            return incomes.Select(i => new CenterIncomeDTO
            {
                Id = i.Id,
                Title = i.Title,
                Amount = i.Amount,
                Category = i.Category,
                Date = i.Date,
                Notes = i.Notes
            }).ToList();
        }

        public async Task<CenterIncomeDTO> AddIncomeAsync(CreateCenterIncomeDTO dto)
        {
            var income = new CenterIncome
            {
                Title = dto.Title,
                Amount = dto.Amount,
                Category = dto.Category,
                Date = dto.Date,
                Notes = dto.Notes
            };

            _context.CenterIncomes.Add(income);
            await _context.SaveChangesAsync();

            return new CenterIncomeDTO
            {
                Id = income.Id,
                Title = income.Title,
                Amount = income.Amount,
                Category = income.Category,
                Date = income.Date,
                Notes = income.Notes
            };
        }

        public async Task DeleteIncomeAsync(int id)
        {
            var income = await _context.CenterIncomes.FindAsync(id);
            if (income != null)
            {
                _context.CenterIncomes.Remove(income);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<MonthlyFinancialSummaryDTO> GetMonthlySummaryAsync(int month, int year)
        {
            // 1. Get Expenses
            var expenses = await GetExpensesAsync(month, year);
            var totalExpenses = expenses.Sum(e => e.Amount);

            // 2. Get Incomes (Other)
            var incomes = await GetIncomesAsync(month, year);
            var totalOtherIncomes = incomes.Sum(i => i.Amount);

            // 3. Get Student Fees (Only paid ones for that month)
            // Wait, should we check PaymentDate or just Month/Year field in StudentFee? 
            // Usually, payment date signifies when the money came in.
            var studentFees = await _context.StudentFees
                .Where(f => f.PaymentDate.HasValue && f.PaymentDate.Value.Month == month && f.PaymentDate.Value.Year == year)
                .SumAsync(f => f.AmountPaid);

            // 4. Get Salaries
            var payroll = await _financeService.GenerateMonthlyPayrollAsync(month, year);
            var totalSalaries = payroll.Sum(p => p.NetSalary);

            return new MonthlyFinancialSummaryDTO
            {
                Month = month,
                Year = year,
                TotalStudentFees = studentFees,
                TotalOtherIncomes = totalOtherIncomes,
                TotalCenterExpenses = totalExpenses,
                TotalSalaries = totalSalaries,
                ExpensesBreakdown = expenses,
                IncomesBreakdown = incomes
            };
        }
    }
}
