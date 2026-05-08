using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IAcademicYearService
    {
        Task<IEnumerable<AcademicYearViewDTO>> GetAllAsync();
        Task<AcademicYearViewDTO> AddAsync(AcademicYearAddDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
