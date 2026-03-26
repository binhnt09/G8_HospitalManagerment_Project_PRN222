using G8_HospitalManagerment_Project_PRN222.Repository;
using G8_HospitalManagerment_Project_PRN222.DTOs;
using Microsoft.EntityFrameworkCore;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Service
{
    public class TestService : ItestService
    {
        private readonly ItestRepository _testRepository;

        public TestService(ItestRepository testRepository)
        {
            _testRepository = testRepository;
        }
        public async Task<IndexDTO> GetIndexDataAsync(string search, string sort, int pageIndex, int pageSize)
        {
            var query = _testRepository.GetAllWithRelations();
            var result = new IndexDTO
            {
                TotalOrders = await query.CountAsync()
            };
            if (!string.IsNullOrEmpty(search))
            {
                search  =search.ToLower();
                query = query.Where(t =>
                    t.TestName != null && t.TestName.ToLower().Contains(search) ||
                    t.Category != null && t.Category.ToLower().Contains(search) ||
                    t.ReferenceRange != null && t.ReferenceRange.ToLower().Contains(search));
            }
            query = sort switch
            {
                "cost_desc" => query.OrderByDescending(t => t.Cost),
                _ => query.OrderBy(t => t.Cost)
            };
            result.TotalPages = (int)Math.Ceiling(result.TotalOrders / (double)pageSize);
            result.ItemStart = (pageIndex - 1) * pageSize + 1;
            result.ItemEnd = Math.Min(pageIndex * pageSize, result.TotalOrders);

            result.TestPagedData = await query.Skip((pageIndex - 1) * pageSize).Take(pageSize).ToListAsync();

            return result;
        }
        public async Task<Test> GetTestDetailsAsync(int id) => await _testRepository.GetByIdAsync(id);
        public async Task CreateTestAsync(Test test) => await _testRepository.AddAsync(test);
        public async Task UpdateTestAsync(Test test) => await _testRepository.UpdateAsync(test);
        public async Task DeleteTestAsync(int id)
        {
            var order = await _testRepository.GetByIdAsync(id);
            if(order != null)
            {
                await _testRepository.DeleteAsync(order);
            }
        }
        public bool CheckTestExists(int id) => _testRepository.Exists(id);
    }
}
