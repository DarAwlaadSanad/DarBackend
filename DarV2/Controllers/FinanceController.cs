using DarV2.Models;
using DarV2.DTOs.Finance;
using DarV2.Service.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class FinanceController : ControllerBase
    {
        private readonly IFinanceService _financeService;

        public FinanceController(IFinanceService financeService)
        {
            _financeService = financeService;
        }

        [HttpGet("settings")]
        [Authorize(Policy = Permissions.ViewFinance)]
        public async Task<IActionResult> GetSettings()
        {
            var settings = await _financeService.GetSettingsAsync();
            return Ok(settings);
        }

        [HttpPost("settings")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> UpdateSettings(FinancialSettingDTO dto)
        {
            await _financeService.UpdateSettingsAsync(dto);
            return Ok(new { message = "Settings updated successfully" });
        }

        [HttpGet("contracts")]
        [Authorize(Policy = Permissions.ViewFinance)]
        public async Task<IActionResult> GetAllContracts()
        {
            var contracts = await _financeService.GetAllContractsAsync();
            return Ok(contracts);
        }

        [HttpGet("contracts/{userId}")]
        [Authorize(Policy = Permissions.ViewFinance)]
        public async Task<IActionResult> GetContract(string userId)
        {
            var contract = await _financeService.GetContractAsync(userId);
            if (contract == null) return NotFound();
            return Ok(contract);
        }

        [HttpPost("contracts")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> SetContract(SetUserContractDTO dto)
        {
            await _financeService.SetContractAsync(dto);
            return Ok(new { message = "Contract updated successfully" });
        }

        [HttpDelete("contracts/{userId}")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> DeleteContract(string userId)
        {
            var result = await _financeService.DeleteContractAsync(userId);
            if (!result) return NotFound(new { message = "العقد غير موجود" });
            return Ok(new { message = "تم حذف العقد والراتب بنجاح" });
        }

        [HttpGet("transactions")]
        [Authorize(Policy = Permissions.ViewFinance)]
        public async Task<IActionResult> GetTransactions([FromQuery] string userId, [FromQuery] int month, [FromQuery] int year)
        {
            var transactions = await _financeService.GetTransactionsAsync(userId, month, year);
            return Ok(transactions);
        }

        [HttpPost("transactions")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> AddTransaction(AddFinancialTransactionDTO dto)
        {
            await _financeService.AddTransactionAsync(dto);
            return Ok(new { message = "Transaction added successfully" });
        }

        [HttpDelete("transactions/{id}")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> DeleteTransaction(int id)
        {
            var result = await _financeService.DeleteTransactionAsync(id);
            if (!result) return NotFound();
            return Ok(new { message = "Transaction deleted successfully" });
        }

        [HttpGet("payroll/monthly")]
        [Authorize(Policy = Permissions.ViewFinance)]
        public async Task<IActionResult> GetMonthlyPayroll([FromQuery] int month, [FromQuery] int year)
        {
            var reports = await _financeService.GenerateMonthlyPayrollAsync(month, year);
            return Ok(reports);
        }

        [HttpGet("payroll/monthly/{userId}")]
        public async Task<IActionResult> GetUserMonthlyPayroll(string userId, [FromQuery] int month, [FromQuery] int year)
        {
            // Security check: only allow if Admin OR if the current user is requesting their own
            var currentUserId = User.Claims.FirstOrDefault(c => c.Type == "uid")?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && currentUserId != userId)
            {
                return Forbid();
            }

            var report = await _financeService.GenerateUserMonthlyPayrollAsync(userId, month, year);
            if (report == null) return NotFound();

            return Ok(report);
        }

        [HttpPost("payroll/toggle-payment")]
        [Authorize(Policy = Permissions.ManageFinance)]
        public async Task<IActionResult> ToggleSalaryPayment(TogglePaymentDTO dto)
        {
            var isPaid = await _financeService.ToggleSalaryPaymentAsync(dto.UserId, dto.Month, dto.Year, dto.Amount);
            return Ok(new { isPaid, message = isPaid ? "تم تسليم المرتب بنجاح" : "تم إلغاء تسليم المرتب بنجاح" });
        }
    }
}
