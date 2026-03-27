using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.AspNetCore.SignalR;
using G8_HospitalManagerment_Project_PRN222.Hubs;

public class AppointmentService
{
    private readonly AppointmentRepository _repo;
    private readonly IHubContext<AppointmentHub> _hubContext;

    public AppointmentService(AppointmentRepository repo, IHubContext<AppointmentHub> hubContext)
    {
        _repo = repo;
        _hubContext = hubContext;
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
        await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate");
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        appointment.UpdatedAt = DateTime.Now;

        await _repo.UpdateAsync(appointment);
        await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate");
    }

    public async Task<List<Appointment>> GetPagedAsync(string searchString, string sortOrder, int pageNumber, int pageSize)
    {
        return await _repo.GetPagedAsync(searchString, sortOrder, pageNumber, pageSize);
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _repo.GetTotalCountAsync();
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        return await _repo.GetCountByStatusAsync(status);
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _repo.GetActiveCountAsync();
    }

    public async Task DeleteAsync(int id)
    {
        await _repo.DeleteAsync(id);
        await _hubContext.Clients.All.SendAsync("ReceiveAppointmentUpdate");
    }
}