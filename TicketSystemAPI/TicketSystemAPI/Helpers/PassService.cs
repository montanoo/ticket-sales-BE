using Microsoft.EntityFrameworkCore;
using TicketSystemAPI.Data;
using TicketSystemAPI.Models;
using TicketSystemAPI.DTO;

namespace TicketSystemAPI.Helpers
{
    public interface IPassService
    {
        Task<IEnumerable<PassDto>> GetAllAsync();
        Task<PassDto> GetByIdAsync(int id);
        Task<PassDto> CreateAsync(CreatePassDto dto);
        Task<bool> UpdateAsync(int id, UpdatePassDto dto);
        Task<bool> DeleteAsync(int id);
    }

    // Offers (ski passes) management, including getting current offers available, creation, update and deletion of offers
    public class PassService : IPassService
    {
        private readonly TicketSystemContext _context;

        public PassService(TicketSystemContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<PassDto>> GetAllAsync()
        {
            return await _context.Tickettypes
                .Select(p => new PassDto
                {
                    TypeId = p.TypeId,
                    Name = p.Name,
                    BaseDurationDays = p.BaseDurationDays,
                    BaseRideLimit = p.BaseRideLimit,
                    BasePrice = p.BasePrice
                }).ToListAsync();
        }

        public async Task<PassDto?> GetByIdAsync(int id)
        {
            var entity = await _context.Tickettypes.FindAsync(id);
            if (entity == null) return null;

            return new PassDto
            {
                TypeId = entity.TypeId,
                Name = entity.Name,
                BaseDurationDays = entity.BaseDurationDays,
                BaseRideLimit = entity.BaseRideLimit,
                BasePrice = entity.BasePrice
            };
        }

        public async Task<PassDto> CreateAsync(CreatePassDto dto)
        {
            var entity = new Tickettype
            {
                Name = dto.Name,
                BaseDurationDays = dto.BaseDurationDays,
                BaseRideLimit = dto.BaseRideLimit,
                BasePrice = dto.BasePrice
            };

            _context.Tickettypes.Add(entity);
            await _context.SaveChangesAsync();

            return new PassDto
            {
                TypeId = entity.TypeId,
                Name = entity.Name,
                BaseDurationDays = entity.BaseDurationDays,
                BaseRideLimit = entity.BaseRideLimit,
                BasePrice = entity.BasePrice
            };
        }

        public async Task<bool> UpdateAsync(int id, UpdatePassDto dto)
        {
            var entity = await _context.Tickettypes.FindAsync(id);
            if (entity == null) return false;

            entity.Name = dto.Name;
            entity.BaseDurationDays = dto.BaseDurationDays;
            entity.BaseRideLimit = dto.BaseRideLimit;
            entity.BasePrice = dto.BasePrice;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var entity = await _context.Tickettypes.FindAsync(id);
            if (entity == null) return false;

            _context.Tickettypes.Remove(entity);
            await _context.SaveChangesAsync();
            return true;
        }
    }

}
