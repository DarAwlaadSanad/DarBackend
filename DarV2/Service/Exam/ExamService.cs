using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class ExamService : IExamService
    {
        private readonly IUnitOfWork _uow;

        public ExamService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<ExamDTO> CreateExamAsync(CreateExamDTO dto)
        {
            var exam = new Exam
            {
                Title = dto.Title,
                Date = dto.Date,
                MaxScore = dto.MaxScore,
                GroupId = dto.GroupId,
                Notes = dto.Notes
            };

            await _uow.Exams.AddAsync(exam);
            await _uow.SaveAsync();

            // When an exam is created, create empty results for all students in the group
            var students = await _uow.Students.GetStudentsGroupAsync(dto.GroupId);
            foreach (var student in students)
            {
                var result = new ExamResult
                {
                    ExamId = exam.Id,
                    StudentId = student.Id,
                    Score = null,
                    Notes = null
                };
                await _uow.ExamResults.AddAsync(result);
            }
            await _uow.SaveAsync();

            return new ExamDTO
            {
                Id = exam.Id,
                Title = exam.Title,
                Date = exam.Date,
                MaxScore = exam.MaxScore,
                GroupId = exam.GroupId,
                Notes = exam.Notes
            };
        }

        public async Task<IEnumerable<ExamDTO>> GetExamsByGroupAsync(int groupId)
        {
            return await _uow.Exams.Query()
                .Where(e => e.GroupId == groupId)
                .OrderByDescending(e => e.Date)
                .Select(e => new ExamDTO
                {
                    Id = e.Id,
                    Title = e.Title,
                    Date = e.Date,
                    MaxScore = e.MaxScore,
                    GroupId = e.GroupId,
                    Notes = e.Notes
                }).ToListAsync();
        }

        public async Task<ExamDTO?> GetExamByIdAsync(int id)
        {
            var e = await _uow.Exams.GetByIdAsync(id);
            if (e == null) return null;
            return new ExamDTO
            {
                Id = e.Id,
                Title = e.Title,
                Date = e.Date,
                MaxScore = e.MaxScore,
                GroupId = e.GroupId,
                Notes = e.Notes
            };
        }

        public async Task<bool> DeleteExamAsync(int id)
        {
            var e = await _uow.Exams.GetByIdAsync(id);
            if (e == null) return false;

            _uow.Exams.Remove(e);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<ExamResultDTO>> GetExamResultsAsync(int examId)
        {
            return await _uow.ExamResults.Query()
                .Include(er => er.Student)
                .Where(er => er.ExamId == examId)
                .Select(er => new ExamResultDTO
                {
                    Id = er.Id,
                    ExamId = er.ExamId,
                    StudentId = er.StudentId,
                    StudentName = er.Student.FullName,
                    Score = er.Score,
                    Notes = er.Notes
                }).ToListAsync();
        }

        public async Task<bool> SaveExamResultsAsync(int examId, SaveExamResultsDTO dto)
        {
            var results = await _uow.ExamResults.Query().Where(er => er.ExamId == examId).ToListAsync();
            
            foreach(var r in dto.Results)
            {
                var target = results.FirstOrDefault(x => x.StudentId == r.StudentId);
                if (target != null)
                {
                    target.Score = r.Score;
                    target.Notes = r.Notes;
                    _uow.ExamResults.Update(target);
                }
            }
            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<ExamResultDTO>> GetStudentExamResultsAsync(int studentId)
        {
            return await _uow.ExamResults.Query()
                .Include(er => er.Exam)
                .Where(er => er.StudentId == studentId && er.Score != null)
                .OrderByDescending(er => er.Exam.Date)
                .Select(er => new ExamResultDTO
                {
                    Id = er.Id,
                    ExamId = er.ExamId,
                    StudentId = er.StudentId,
                    StudentName = er.Student.FullName,
                    ExamTitle = er.Exam.Title,
                    ExamDate = er.Exam.Date,
                    MaxScore = er.Exam.MaxScore,
                    Score = er.Score,
                    Notes = er.Notes
                }).ToListAsync();
        }
    }
}
