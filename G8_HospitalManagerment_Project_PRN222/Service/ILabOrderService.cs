using G8_HospitalManagerment_Project_PRN222.DTOs;
using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Service
{
    public interface ILabOrderService
    {
        Task<LabOrderDTO> GetIndexDataAsync(string searchString, string sortOrder, int pageIndex, int pageSize);
        Task<LabOrder> GetLabOrderDetailsAsync(int id);
        Task CreateLabOrderAsync(LabOrder labOrder);
        Task UpdateLabOrderAsync(LabOrder labOrder);
        Task DeleteLabOrderAsync(int id);
        bool CheckLabOrderExists(int id);
        // Hỗ trợ Dropdown
        Task<dynamic> GetDropdownDataAsync();
    }
}
