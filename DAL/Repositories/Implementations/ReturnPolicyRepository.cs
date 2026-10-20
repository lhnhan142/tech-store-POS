using DAL.Context;
using DAL.Entities;
using DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DAL.Repositories.Implementations
{
    public class ReturnPolicyRepository : Repository<ReturnPolicy>, IReturnPolicyRepository
    {
        public ReturnPolicyRepository(AppDbContext context) : base(context)
        {
        }

        public async Task<ReturnPolicy?> GetActivePolicyAsync()
        {
            return await _context.ReturnPolicies
                .Include(p => p.UpdatedByUser)
                .Where(p => p.IsActive)
                .OrderByDescending(p => p.UpdatedAt)
                .FirstOrDefaultAsync();
        }
    }
}
