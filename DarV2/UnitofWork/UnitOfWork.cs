using DarV2.Context;
using DarV2.Models;
using DarV2.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace DarV2.UnitofWork
{
    
    public class UnitOfWork : IUnitOfWork
    {
        private readonly DarContext _context;
        private IDbContextTransaction? _transaction;

        // Lazy-loaded repositories
        private IStudentRepository? _studentRepository;
        private IGroupRepository? _group_repository;
        private IGenericRepository<Attendance>? _attendance_repository;
        private IGenericRepository<Evaluation>? _evaluation_repository;
        private ISessionRepository? _sessionRepository;
        private IGenericRepository<StudentGroup>? _studentGroupRepository;
        private IGenericRepository<GroupSchedule>? _group_schedule_repository;
        private IGenericRepository<Phone>? _phone_repository;
        private IGenericRepository<Image>? _image_repository;
        private IGenericRepository<FeePlan>? _feePlanRepository;
        private IGenericRepository<StudentFee>? _studentFeeRepository;
        private IGenericRepository<AcademicYear>? _academicYearRepository;
        private IGenericRepository<MemorizationRecord>? _memorizationRepository;

        public UnitOfWork(DarContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        
        public IStudentRepository Students
            => _studentRepository ??= new StudentRepository(_context);

        
        public IGroupRepository Groups
            => _group_repository ??= new GroupRepository(_context);

        
        public IGenericRepository<Attendance> Attendances
            => _attendance_repository ??= new GenericRepository<Attendance>(_context);

        
        public IGenericRepository<Evaluation> Evaluations
            => _evaluation_repository ??= new GenericRepository<Evaluation>(_context);

        
        public ISessionRepository Sessions
            => _sessionRepository ??= new SessionRepository(_context);

        
        public IGenericRepository<StudentGroup> StudentGroups
            => _studentGroupRepository ??= new GenericRepository<StudentGroup>(_context);

        
        public IGenericRepository<GroupSchedule> GroupSchedules
            => _group_schedule_repository ??= new GenericRepository<GroupSchedule>(_context);

        
        public IGenericRepository<Phone> Phones
            => _phone_repository ??= new GenericRepository<Phone>(_context);

        
        public IGenericRepository<Image> Images
            => _image_repository ??= new GenericRepository<Image>(_context);

        public IGenericRepository<FeePlan> FeePlans
            => _feePlanRepository ??= new GenericRepository<FeePlan>(_context);

        public IGenericRepository<StudentFee> StudentFees
            => _studentFeeRepository ??= new GenericRepository<StudentFee>(_context);

        public IGenericRepository<AcademicYear> AcademicYears
            => _academicYearRepository ??= new GenericRepository<AcademicYear>(_context);

        public IGenericRepository<MemorizationRecord> MemorizationRecords
            => _memorizationRepository ??= new GenericRepository<MemorizationRecord>(_context);

        
        public async Task<int> SaveAsync()
        {
            try
            {
                return await _context.SaveChangesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        
        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        
        public async Task CommitAsync()
        {
            try
            {
                await SaveAsync();
                if (_transaction != null)
                {
                    await _transaction.CommitAsync();
                }
            }
            catch
            {
                await RollbackAsync();
                throw;
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        
        public async Task RollbackAsync()
        {
            try
            {
                if (_transaction != null)
                {
                    await _transaction.RollbackAsync();
                }
            }
            finally
            {
                if (_transaction != null)
                {
                    await _transaction.DisposeAsync();
                    _transaction = null;
                }
            }
        }

        
        public async ValueTask DisposeAsync()
        {
            if (_transaction != null)
            {
                await _transaction.DisposeAsync();
            }
            await _context.DisposeAsync();
            GC.SuppressFinalize(this);
        }
    }
}
