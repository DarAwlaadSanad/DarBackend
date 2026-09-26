using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;
using Microsoft.EntityFrameworkCore;

namespace DarV2.Service
{
    public class CompetitionService : ICompetitionService
    {
        private readonly IUnitOfWork _uow;

        public CompetitionService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<CompetitionDTO> CreateCompetitionAsync(CreateCompetitionDTO dto)
        {
            var comp = new Competition
            {
                Title = dto.Title,
                Date = dto.Date,
                Notes = dto.Notes
            };

            await _uow.Competitions.AddAsync(comp);
            await _uow.SaveAsync();

            return new CompetitionDTO
            {
                Id = comp.Id,
                Title = comp.Title,
                Date = comp.Date,
                Notes = comp.Notes
            };
        }

        public async Task<IEnumerable<CompetitionDTO>> GetAllCompetitionsAsync()
        {
            var comps = await _uow.Competitions.Query()
                .Include(c => c.Levels)
                .OrderByDescending(c => c.Date)
                .ToListAsync();

            return comps.Select(c => new CompetitionDTO
            {
                Id = c.Id,
                Title = c.Title,
                Date = c.Date,
                Notes = c.Notes,
                Levels = c.Levels.Select(l => new CompetitionLevelDTO
                {
                    Id = l.Id,
                    CompetitionId = l.CompetitionId,
                    Name = l.Name,
                    MaxScore = l.MaxScore
                }).ToList()
            });
        }

        public async Task<CompetitionDTO?> GetCompetitionByIdAsync(int id)
        {
            var c = await _uow.Competitions.Query()
                .Include(x => x.Levels)
                .ThenInclude(l => l.Results)
                .FirstOrDefaultAsync(x => x.Id == id);

            if (c == null) return null;

            return new CompetitionDTO
            {
                Id = c.Id,
                Title = c.Title,
                Date = c.Date,
                Notes = c.Notes,
                Levels = c.Levels.Select(l => new CompetitionLevelDTO
                {
                    Id = l.Id,
                    CompetitionId = l.CompetitionId,
                    Name = l.Name,
                    MaxScore = l.MaxScore,
                    RegisteredStudentsCount = l.Results.Count
                }).ToList()
            };
        }

        public async Task<bool> DeleteCompetitionAsync(int id)
        {
            var c = await _uow.Competitions.GetByIdAsync(id);
            if (c == null) return false;

            _uow.Competitions.Remove(c);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<CompetitionLevelDTO> CreateLevelAsync(CreateCompetitionLevelDTO dto)
        {
            var level = new CompetitionLevel
            {
                CompetitionId = dto.CompetitionId,
                Name = dto.Name,
                MaxScore = dto.MaxScore
            };

            await _uow.CompetitionLevels.AddAsync(level);
            await _uow.SaveAsync();

            return new CompetitionLevelDTO
            {
                Id = level.Id,
                CompetitionId = level.CompetitionId,
                Name = level.Name,
                MaxScore = level.MaxScore,
                RegisteredStudentsCount = 0
            };
        }

        public async Task<bool> DeleteLevelAsync(int levelId)
        {
            var l = await _uow.CompetitionLevels.GetByIdAsync(levelId);
            if (l == null) return false;

            _uow.CompetitionLevels.Remove(l);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> RegisterStudentToLevelAsync(RegisterStudentDTO dto)
        {
            var level = await _uow.CompetitionLevels.Query()
                .Include(l => l.Competition)
                .FirstOrDefaultAsync(l => l.Id == dto.CompetitionLevelId);
            if (level == null) return false;

            // Check if student is already registered in any level of this competition
            var alreadyRegistered = await _uow.CompetitionResults.Query()
                .AnyAsync(r => r.StudentId == dto.StudentId && r.CompetitionLevel.CompetitionId == level.CompetitionId);
            if (alreadyRegistered) return false;

            var result = new CompetitionResult
            {
                CompetitionLevelId = dto.CompetitionLevelId,
                StudentId = dto.StudentId,
                Score = null,
                Notes = null
            };

            await _uow.CompetitionResults.AddAsync(result);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<bool> UnregisterStudentFromLevelAsync(int levelId, int studentId)
        {
            var result = await _uow.CompetitionResults.Query()
                .FirstOrDefaultAsync(r => r.CompetitionLevelId == levelId && r.StudentId == studentId);

            if (result == null) return false;

            _uow.CompetitionResults.Remove(result);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<CompetitionResultDTO>> GetLevelResultsAsync(int levelId)
        {
            var results = await _uow.CompetitionResults.Query()
                .Include(r => r.Student)
                .Include(r => r.CompetitionLevel)
                .Where(r => r.CompetitionLevelId == levelId)
                .ToListAsync();

            return results.Select(r => new CompetitionResultDTO
            {
                Id = r.Id,
                CompetitionLevelId = r.CompetitionLevelId,
                LevelName = r.CompetitionLevel.Name,
                MaxScore = r.CompetitionLevel.MaxScore,
                StudentId = r.StudentId,
                StudentName = r.Student.FullName,
                StudentCode = r.Student.Code,
                Score = r.Score,
                Notes = r.Notes
            });
        }

        public async Task<bool> SaveLevelResultsAsync(int levelId, SaveCompetitionResultsDTO dto)
        {
            var level = await _uow.CompetitionLevels.GetByIdAsync(levelId);
            if (level == null) return false;

            var results = await _uow.CompetitionResults.Query()
                .Where(r => r.CompetitionLevelId == levelId)
                .ToListAsync();

            foreach (var r in dto.Results)
            {
                var target = results.FirstOrDefault(x => x.StudentId == r.StudentId);
                if (target != null)
                {
                    // Enforce that score does not exceed level max score
                    if (r.Score.HasValue && r.Score.Value > level.MaxScore)
                    {
                        throw new InvalidOperationException($"الدرجة المسجلة للطالب لا يمكن أن تتجاوز الدرجة القصوى للمستوى ({level.MaxScore})");
                    }
                    target.Score = r.Score;
                    target.Notes = r.Notes;
                    _uow.CompetitionResults.Update(target);
                }
            }

            await _uow.SaveAsync();
            return true;
        }

        public async Task<IEnumerable<StudentCompetitionResultDTO>> GetStudentCompetitionsAsync(int studentId)
        {
            var results = await _uow.CompetitionResults.Query()
                .Include(r => r.CompetitionLevel)
                .ThenInclude(l => l.Competition)
                .Where(r => r.StudentId == studentId)
                .ToListAsync();

            return results.Select(r => new StudentCompetitionResultDTO
            {
                Id = r.Id,
                Title = r.CompetitionLevel.Competition.Title,
                Date = r.CompetitionLevel.Competition.Date,
                LevelName = r.CompetitionLevel.Name,
                MaxScore = r.CompetitionLevel.MaxScore,
                Score = r.Score,
                Notes = r.Notes
            }).OrderByDescending(x => x.Date).ToList();
        }
    }
}
