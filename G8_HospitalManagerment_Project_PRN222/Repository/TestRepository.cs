using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222.Repository
{
    public class TestRepository : ItestRepository
    {
        private DbHospitalManagementContext _context;

        public TestRepository(DbHospitalManagementContext context)
        {
            _context = context;
        }
        public IQueryable<Test> GetAllWithRelations()
        {
            return _context.Tests.AsQueryable();
        }
        public async Task<Test> GetByIdAsync(int id)
        {
            return await GetAllWithRelations().FirstOrDefaultAsync(m => m.TestId == id);
        }
        public async Task AddAsync(Test test)
        {
            _context.Add(test);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateAsync(Test test)
        {
            _context.Update(test);
            await _context.SaveChangesAsync();
        }
        public async Task DeleteAsync(Test test)
        {
            _context.Tests.Remove(test);
            await _context.SaveChangesAsync();
        }
        public bool Exists(int id) => _context.Tests.Any(e => e.TestId == id);
    }
}
