using DarV2.DTOs;
using DarV2.Models;
using DarV2.UnitofWork;

namespace DarV2.Service
{
    public class FeePlanService : IFeePlanService
    {
        private readonly IUnitOfWork _uow;

        public FeePlanService(IUnitOfWork uow)
        {
            _uow = uow;
        }

        public async Task<FeePlanViewDTO> AddAsync(FeePlanAddDTO dto)
        {
            // Deactivate any overlapping active feeplan for the group
            var existing = await _uow.FeePlans.FindAsync(fp => fp.GroupId == dto.GroupId && fp.IsActive);
            foreach (var e in existing)
            {
                e.IsActive = false;
                e.EffectiveTo = dto.EffectiveFrom.AddDays(-1);
                _uow.FeePlans.Update(e);
            }

            var fp = new FeePlan
            {
                GroupId = dto.GroupId,
                Amount = dto.Amount,
                EffectiveFrom = dto.EffectiveFrom,
                IsActive = true
            };

            await _uow.FeePlans.AddAsync(fp);
            await _uow.SaveAsync();

            return new FeePlanViewDTO
            {
                Id = fp.Id,
                GroupId = fp.GroupId,
                Amount = fp.Amount,
                EffectiveFrom = fp.EffectiveFrom,
                EffectiveTo = fp.EffectiveTo,
                IsActive = fp.IsActive
            };
        }

        public async Task<bool> DeactivateAsync(int feePlanId)
        {
            var fp = await _uow.FeePlans.GetByIdAsync(feePlanId);
            if (fp == null) return false;

            fp.IsActive = false;
            fp.EffectiveTo = DateOnly.FromDateTime(DateTime.Today);
            _uow.FeePlans.Update(fp);
            await _uow.SaveAsync();
            return true;
        }

        public async Task<List<FeePlanViewDTO>> GetAllPlans(int groupId)
        {
            var plans = await _uow.FeePlans.FindAsync(fp => fp.GroupId == groupId);
            if (plans == null || !plans.Any()) return null;
            var result = plans.Select(fp => new FeePlanViewDTO
            {
                Id = fp.Id,
                GroupId = fp.GroupId,
                Amount = fp.Amount,
                EffectiveFrom = fp.EffectiveFrom,
                EffectiveTo = fp.EffectiveTo,
                IsActive = fp.IsActive
            }).ToList();
            return result;

        }
    }
}
