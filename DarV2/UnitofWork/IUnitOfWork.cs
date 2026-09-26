using DarV2.Models;
using DarV2.Repository;

namespace DarV2.UnitofWork
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        IStudentRepository Students { get; }
        IGenericRepository<Room> Rooms { get; }
        IGroupRepository Groups { get; }
        IGenericRepository<Attendance> Attendances { get; }
        IGenericRepository<Evaluation> Evaluations { get; }
        IGenericRepository<Exam> Exams { get; }
        IGenericRepository<ExamResult> ExamResults { get; }
        IGenericRepository<Competition> Competitions { get; }
        IGenericRepository<CompetitionLevel> CompetitionLevels { get; }
        IGenericRepository<CompetitionResult> CompetitionResults { get; }
        ISessionRepository Sessions { get; }
        IGenericRepository<StudentGroup> StudentGroups { get; }
        IGenericRepository<GroupSchedule> GroupSchedules { get; }
        IGenericRepository<Phone> Phones { get; }
        IGenericRepository<Image> Images { get; }
        IGenericRepository<FeePlan> FeePlans { get; }
        IGenericRepository<StudentFee> StudentFees { get; }
        IGenericRepository<AcademicYear> AcademicYears { get; }
        IGenericRepository<MemorizationRecord> MemorizationRecords { get; }
        IGenericRepository<StudentWarning> StudentWarnings { get; }
        Task<int> SaveAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
