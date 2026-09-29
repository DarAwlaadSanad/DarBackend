using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DarV2.Context;
using DarV2.DTOs.AttendanceLocation;
using DarV2.Models.AttendanceLocation;

namespace DarV2.Service.AttendanceLocation
{
    public class AttendanceLocationService : IAttendanceLocationService
    {
        private readonly DarContext _context;

        public AttendanceLocationService(DarContext context)
        {
            _context = context;
        }

        public async Task<List<AttendanceLocationDTO>> GetAllAsync()
        {
            var list = await _context.AttendanceLocations
                .OrderByDescending(a => a.CreatedAt)
                .ToListAsync();

            return list.Select(MapToDTO).ToList();
        }

        public async Task<List<AttendanceLocationDTO>> GetActiveLocationsAsync()
        {
            var list = await _context.AttendanceLocations
                .Where(a => a.IsActive)
                .OrderBy(a => a.Name)
                .ToListAsync();

            return list.Select(MapToDTO).ToList();
        }

        public async Task<AttendanceLocationDTO?> GetByIdAsync(int id)
        {
            var loc = await _context.AttendanceLocations.FindAsync(id);
            return loc != null ? MapToDTO(loc) : null;
        }

        public async Task<AttendanceLocationDTO> CreateAsync(CreateAttendanceLocationDTO dto)
        {
            var entity = new Models.AttendanceLocation.AttendanceLocation
            {
                Name = dto.Name.Trim(),
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                RadiusInMeters = dto.RadiusInMeters > 0 ? dto.RadiusInMeters : 50.0,
                IsActive = dto.IsActive,
                Address = dto.Address?.Trim()
            };

            _context.AttendanceLocations.Add(entity);
            await _context.SaveChangesAsync();

            return MapToDTO(entity);
        }

        public async Task<AttendanceLocationDTO?> UpdateAsync(UpdateAttendanceLocationDTO dto)
        {
            var loc = await _context.AttendanceLocations.FindAsync(dto.Id);
            if (loc == null) return null;

            loc.Name = dto.Name.Trim();
            loc.Latitude = dto.Latitude;
            loc.Longitude = dto.Longitude;
            loc.RadiusInMeters = dto.RadiusInMeters > 0 ? dto.RadiusInMeters : 50.0;
            loc.IsActive = dto.IsActive;
            loc.Address = dto.Address?.Trim();

            await _context.SaveChangesAsync();
            return MapToDTO(loc);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var loc = await _context.AttendanceLocations.FindAsync(id);
            if (loc == null) return false;

            _context.AttendanceLocations.Remove(loc);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ToggleActiveAsync(int id)
        {
            var loc = await _context.AttendanceLocations.FindAsync(id);
            if (loc == null) return false;

            loc.IsActive = !loc.IsActive;
            await _context.SaveChangesAsync();
            return true;
        }

        private static AttendanceLocationDTO MapToDTO(Models.AttendanceLocation.AttendanceLocation loc)
        {
            return new AttendanceLocationDTO
            {
                Id = loc.Id,
                Name = loc.Name,
                Latitude = loc.Latitude,
                Longitude = loc.Longitude,
                RadiusInMeters = loc.RadiusInMeters,
                IsActive = loc.IsActive,
                Address = loc.Address,
                CreatedAt = loc.CreatedAt
            };
        }
    }
}
