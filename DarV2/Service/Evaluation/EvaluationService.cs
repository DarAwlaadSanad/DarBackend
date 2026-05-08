using DarV2.DTOs;
using DarV2.UnitofWork;
using DarV2.Models;

namespace DarV2.Service
{
    public class EvaluationService : IEvaluationService
    {
        private readonly IUnitOfWork _uow;

        public EvaluationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task SaveBatchAsync(EvaluationBatchDTO batch)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));

            var session = await _uow.Sessions.GetByIdAsync(batch.SessionId);
            if (session == null) throw new InvalidOperationException("Session not found");

            foreach (var entry in batch.Entries)
            {
                var existing = await _uow.Evaluations.FirstOrDefaultAsync(e => e.SessionId == batch.SessionId && e.StudentId == entry.StudentId);
                if (existing != null)
                {
                    existing.Score = entry.Score;
                    existing.Comment = entry.Comment;
                    _uow.Evaluations.Update(existing);
                }
                else
                {
                    var ev = new Evaluation
                    {
                        SessionId = batch.SessionId,
                        StudentId = entry.StudentId,
                        Score = entry.Score,
                        Comment = entry.Comment
                    };
                    await _uow.Evaluations.AddAsync(ev);
                }
            }

            await _uow.SaveAsync();
        }
    }
}
