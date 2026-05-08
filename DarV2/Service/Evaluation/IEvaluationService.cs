using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IEvaluationService
    {
        Task SaveBatchAsync(EvaluationBatchDTO batch);
    }
}
