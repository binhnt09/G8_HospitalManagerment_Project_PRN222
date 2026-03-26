using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Repository
{
    public interface ItestRepository
    {
        IQueryable<Test> GetAllWithRelations();
        Task<Test> GetByIdAsync(int id);
        Task AddAsync(Test test);
        Task UpdateAsync(Test test);
        Task DeleteAsync(Test test);
        bool Exists(int id);
    }
}
