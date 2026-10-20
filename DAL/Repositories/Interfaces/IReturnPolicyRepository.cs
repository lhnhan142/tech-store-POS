using DAL.Entities;

namespace DAL.Repositories.Interfaces
{
    public interface IReturnPolicyRepository : IRepository<ReturnPolicy>
    {
        Task<ReturnPolicy?> GetActivePolicyAsync();
    }
}
