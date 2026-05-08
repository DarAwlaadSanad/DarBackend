using DarV2.DTOs;
using DarV2.UnitofWork;

namespace DarV2.Service
{
    public class MemorizationService : IMemorizationService
    {
        private readonly IUnitOfWork _uow;

        public MemorizationService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<MemorizationRecordDTO> AddAsync(MemorizationRecordCreateDTO dto)
        {
            var student = await _uow.Students.GetByIdAsync(dto.StudentId);
            if (student == null) throw new InvalidOperationException("Student not found");

            var rec = new Models.MemorizationRecord
            {
                StudentId = dto.StudentId,
                FromSurahId = dto.FromSurahId,
                FromAyah = dto.FromAyah,
                ToSurahId = dto.ToSurahId,
                ToAyah = dto.ToAyah,
                Date = dto.Date,
                Notes = dto.Notes
            };

            await _uow.MemorizationRecords.AddAsync(rec);
            await _uow.SaveAsync();

            return new MemorizationRecordDTO
            {
                Id = rec.Id,
                StudentId = rec.StudentId,
                StudentName = student.FullName,
                FromSurahId = rec.FromSurahId,
                FromAyah = rec.FromAyah,
                ToSurahId = rec.ToSurahId,
                ToAyah = rec.ToAyah,
                Date = rec.Date,
                Notes = rec.Notes
            };
        }

        public async Task<bool> UpdateAsync(int id, MemorizationRecordCreateDTO dto)
        {
            var rec = await _uow.MemorizationRecords.GetByIdAsync(id);
            if (rec == null) return false;

            rec.FromSurahId = dto.FromSurahId;
            rec.FromAyah = dto.FromAyah;
            rec.ToSurahId = dto.ToSurahId;
            rec.ToAyah = dto.ToAyah;
            rec.Date = dto.Date;
            rec.Notes = dto.Notes;

            _uow.MemorizationRecords.Update(rec);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var rec = await _uow.MemorizationRecords.GetByIdAsync(id);
            if (rec == null) return false;
            _uow.MemorizationRecords.Remove(rec);
            await _uow.SaveAsync();
            return true;
        }
    }
}
