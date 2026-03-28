using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.EntityFrameworkCore;

namespace G8_HospitalManagerment_Project_PRN222.Repository
{
    public class LabOrderRepository : ILabOrderRepository
    {
        private readonly DbHospitalManagementContext _context;

        public LabOrderRepository(DbHospitalManagementContext context)
        {
            _context = context;
        }

        public IQueryable<LabOrder> GetAllWithRelations()
        {
            return _context.LabOrders
                .Include(l => l.Doctor)
                .Include(l => l.MedicalRecord)
                .Include(l => l.Patient)
                .AsQueryable();
        }

        public async Task<LabOrder> GetByIdAsync(int id)
        {
            return await _context.LabOrders
                    .Include(l => l.Patient)
                        .ThenInclude(p => p.User)

                    .Include(l => l.Doctor)
                        .ThenInclude(d => d.Employee)
                            .ThenInclude(e => e.User)

                    // Kéo danh sách Test (Quan trọng nhất để không bị lỗi "Không tìm thấy danh sách test")
                    .Include(l => l.LabOrderItems)
                        .ThenInclude(li => li.Test)

                    .FirstOrDefaultAsync(l => l.OrderId == id);
        }

        public async Task AddAsync(LabOrder labOrder)
        {
            _context.Add(labOrder);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(LabOrder labOrder)
        {
            _context.Update(labOrder);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(LabOrder labOrder)
        {
            _context.LabOrders.Remove(labOrder);
            await _context.SaveChangesAsync();
        }

        public bool Exists(int id) => _context.LabOrders.Any(e => e.OrderId == id);

        // Hỗ trợ lấy dữ liệu cho View Create/Edit
        public IQueryable<Doctor> GetDoctors() => _context.Doctors;
        public IQueryable<MedicalRecord> GetMedicalRecords() => _context.MedicalRecords;
        public IQueryable<Patient> GetPatients() => _context.Patients;
    }
}
