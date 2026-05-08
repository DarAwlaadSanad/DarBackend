using DarV2.DTOs;

namespace DarV2.Service
{
    public interface IFeePlanService
    {
        Task<FeePlanViewDTO> AddAsync(FeePlanAddDTO dto);
        Task<List<FeePlanViewDTO>> GetAllPlans(int groupId);
        Task<bool> DeactivateAsync(int feePlanId);
    }
}
