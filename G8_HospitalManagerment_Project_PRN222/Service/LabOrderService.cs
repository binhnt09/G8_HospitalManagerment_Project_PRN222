using G8_HospitalManagerment_Project_PRN222.DTOs;
using G8_HospitalManagerment_Project_PRN222.Models;
using G8_HospitalManagerment_Project_PRN222.Repository;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222.Service
{
    public class LabOrderService : ILabOrderService
    {
        private readonly ILabOrderRepository _repository;

        public LabOrderService(ILabOrderRepository repository)
        {
            _repository = repository;
        }

        public async Task<LabOrderDTO> GetIndexDataAsync(string searchString, string sortOrder, int pageIndex, int pageSize)
        {
            var query = _repository.GetAllWithRelations();

            // 1. Tính toán thống kê
            var result = new LabOrderDTO
            {
                TotalOrders = await query.CountAsync(),
                CompletedCount = await query.CountAsync(l => l.Status == "Completed"),
                PendingCount = await query.CountAsync(l => l.Status == "Pending"),
                ActiveCount = await query.CountAsync(l => l.IsDeleted == false)
            };

            // 2. Xử lý Tìm kiếm
            if (!string.IsNullOrEmpty(searchString))
            {
                searchString = searchString.ToLower();
                query = query.Where(l =>
                    l.Reason != null && l.Reason.ToLower().Contains(searchString) ||
                    l.DoctorId.ToString().Contains(searchString) ||
                    l.PatientId.ToString().Contains(searchString));
            }

            // 3. Xử lý Sắp xếp
            query = sortOrder switch
            {
                "date_desc" => query.OrderByDescending(l => l.OrderDate),
                _ => query.OrderBy(l => l.OrderDate),
            };

            // 4. Xử lý Phân trang
            result.TotalPages = (int)Math.Ceiling(result.TotalOrders / (double)pageSize);
            result.ItemStart = (pageIndex - 1) * pageSize + 1;
            result.ItemEnd = Math.Min(pageIndex * pageSize, result.TotalOrders);

            result.PagedData = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return result;
        }

        public async Task<LabOrder> GetLabOrderDetailsAsync(int id) => await _repository.GetByIdAsync(id);

        public async Task CreateLabOrderAsync(LabOrder labOrder) => await _repository.AddAsync(labOrder);

        public async Task UpdateLabOrderAsync(LabOrder labOrder) => await _repository.UpdateAsync(labOrder);

        public async Task DeleteLabOrderAsync(int id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order != null)
            {
                await _repository.DeleteAsync(order);
            }
        }

        public bool CheckLabOrderExists(int id) => _repository.Exists(id);

        // Trả về dữ liệu cho các SelectList (Dropdown)
        public async Task<dynamic> GetDropdownDataAsync()
        {
            return new
            {
                Doctors = await _repository.GetDoctors().ToListAsync(),
                MedicalRecords = await _repository.GetMedicalRecords().ToListAsync(),
                Patients = await _repository.GetPatients().ToListAsync()
            };
        }
    }
}
