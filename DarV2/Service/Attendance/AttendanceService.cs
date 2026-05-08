using DarV2.DTOs;
using DarV2.UnitofWork;
using DarV2.Models;

namespace DarV2.Service
{
    public class AttendanceService : IAttendanceService
    {
        private readonly IUnitOfWork _uow;

        public AttendanceService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task SaveBatchAsync(AttendanceBatchDTO batch)
        {
            if (batch == null) throw new ArgumentNullException(nameof(batch));

            // Validate session exists
            var session = await _uow.Sessions.GetByIdAsync(batch.SessionId);
            if (session == null) throw new InvalidOperationException("Session not found");

            // For each entry: upsert attendance record
            foreach (var entry in batch.Entries)
            {
                var existing = await _uow.Attendances.FirstOrDefaultAsync(a => a.SessionId == batch.SessionId && a.StudentId == entry.StudentId);
                if (existing != null)
                {
                    existing.Status = entry.Status;
                    existing.Notes = entry.Notes;
                    _uow.Attendances.Update(existing);
                }
                else
                {
                    var att = new Attendance
                    {
                        SessionId = batch.SessionId,
                        StudentId = entry.StudentId,
                        Status = entry.Status,
                        Notes = entry.Notes
                    };
                    await _uow.Attendances.AddAsync(att);
                }
            }

            await _uow.SaveAsync();
        }
    }
}
