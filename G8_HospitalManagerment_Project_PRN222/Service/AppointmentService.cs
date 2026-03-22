using G8_HospitalManagerment_Project_PRN222.Models;
public class AppointmentService
{
    private readonly AppointmentRepository _repo;

    public AppointmentService(AppointmentRepository repo)
    {
        _repo = repo;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _repo.GetAllAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _repo.GetByIdAsync(id);
    }

    public async Task CreateAsync(Appointment appointment)
    {
        // You can add business logic here later
        appointment.CreatedAt = DateTime.Now;

        await _repo.AddAsync(appointment);
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.Now;

        await _repo.UpdateAsync(appointment);
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
    }
}