using G8_HospitalManagerment_Project_PRN222.DTOs;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Service
{
    public interface ItestService
    {
        Task<IndexDTO> GetIndexDataAsync(string search, string sort, int pageIndex, int pageSize);
        Task<Test> GetTestDetailsAsync(int id);
        Task CreateTestAsync(Test test);
        Task UpdateTestAsync(Test test);
        Task DeleteTestAsync(int id);
        bool CheckTestExists(int id);
        // Hỗ trợ Dropdown
        //Task<dynamic> GetDropdownDataAsync();
    }
}
