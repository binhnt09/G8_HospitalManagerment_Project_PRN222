using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.Repository
{
    public interface ILabOrderRepository
    {
        IQueryable<LabOrder> GetAllWithRelations();
        Task<LabOrder> GetByIdAsync(int id);
        //Task<LabOrder> GetLabOrderWithDetailsAsync(int id);
        Task AddAsync(LabOrder labOrder);
        Task UpdateAsync(LabOrder labOrder);
        Task DeleteAsync(LabOrder labOrder);
        bool Exists(int id);

        // Lấy data cho Dropdown list
        IQueryable<Doctor> GetDoctors();
        IQueryable<MedicalRecord> GetMedicalRecords();
        IQueryable<Patient> GetPatients();
    }
}
