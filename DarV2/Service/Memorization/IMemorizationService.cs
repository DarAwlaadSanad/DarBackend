using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IMemorizationService
    {
        Task<MemorizationRecordDTO> AddAsync(MemorizationRecordCreateDTO dto);
        Task<bool> UpdateAsync(int id, MemorizationRecordCreateDTO dto);
        Task<bool> DeleteAsync(int id);
    }
}
