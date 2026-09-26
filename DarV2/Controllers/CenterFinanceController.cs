using DarV2.Models;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using DarV2.DTOs.Finance;
using DarV2.Service.Finance;

namespace DarV2.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CenterFinanceController : ControllerBase
    {
        private readonly ICenterFinanceService _centerFinanceService;

        public CenterFinanceController(ICenterFinanceService centerFinanceService)
        {
            _centerFinanceService = centerFinanceService;
        }

        [HttpGet("expenses")]
        public async Task<ActionResult<List<CenterExpenseDTO>>> GetExpenses(int month, int year)
        {
            var expenses = await _centerFinanceService.GetExpensesAsync(month, year);
            return Ok(expenses);
        }

        [HttpPost("expenses")]
        public async Task<ActionResult<CenterExpenseDTO>> AddExpense(CreateCenterExpenseDTO dto)
        {
            var expense = await _centerFinanceService.AddExpenseAsync(dto);
            return Ok(expense);
        }

        [HttpDelete("expenses/{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            await _centerFinanceService.DeleteExpenseAsync(id);
            return NoContent();
        }

        [HttpGet("incomes")]
        public async Task<ActionResult<List<CenterIncomeDTO>>> GetIncomes(int month, int year)
        {
            var incomes = await _centerFinanceService.GetIncomesAsync(month, year);
            return Ok(incomes);
        }

        [HttpPost("incomes")]
        public async Task<ActionResult<CenterIncomeDTO>> AddIncome(CreateCenterIncomeDTO dto)
        {
            var income = await _centerFinanceService.AddIncomeAsync(dto);
            return Ok(income);
        }

        [HttpDelete("incomes/{id}")]
        public async Task<IActionResult> DeleteIncome(int id)
        {
            await _centerFinanceService.DeleteIncomeAsync(id);
            return NoContent();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<MonthlyFinancialSummaryDTO>> GetSummary(int month, int year)
        {
            var summary = await _centerFinanceService.GetMonthlySummaryAsync(month, year);
            return Ok(summary);
        }
    }
}
