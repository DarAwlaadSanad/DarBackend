using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;

namespace DarV2.Service
{
    public class AcademicYearService : IAcademicYearService
    {
        private readonly IUnitOfWork _uow;

        public AcademicYearService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<IEnumerable<AcademicYearViewDTO>> GetAllAsync()
        {
            var items = await _uow.AcademicYears.GetAllAsync();
            return items.Select(i => new AcademicYearViewDTO { Id = i.Id, Name = i.Name, TypeSchool = (int)i.TypeSchool }).ToList();
        }

        public async Task<AcademicYearViewDTO> AddAsync(AcademicYearAddDTO dto)
        {
            var ay = new AcademicYear { Name = dto.Name, TypeSchool = (TypeSchool)dto.TypeSchool };
            await _uow.AcademicYears.AddAsync(ay);
            await _uow.SaveAsync();
            return new AcademicYearViewDTO { Id = ay.Id, Name = ay.Name, TypeSchool = (int)ay.TypeSchool };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var ay = await _uow.AcademicYears.GetByIdAsync(id);
            if (ay == null) return false;
            _uow.AcademicYears.Remove(ay);
            await _uow.SaveAsync();
            return true;
        }
    }
}
