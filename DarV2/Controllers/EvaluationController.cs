using DarV2.DTOs;
using DarV2.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DarV2.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EvaluationController : ControllerBase
    {
        private readonly IEvaluationService _service;

        public EvaluationController(IEvaluationService service)
        {
            _service = service;
        }

        [HttpPost("batch")]
        [Authorize]
        public async Task<IActionResult> SaveBatch([FromBody] EvaluationBatchDTO batch)
        {
            await _service.SaveBatchAsync(batch);
            return NoContent();
        }
    }
}
