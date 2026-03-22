using G8_HospitalManagerment_Project_PRN222.Models;
using Microsoft.EntityFrameworkCore;
public class AppointmentRepository
{
    private readonly DbHospitalManagementContext _context;

    public AppointmentRepository(DbHospitalManagementContext context)
    {
        _context = context;
    }

    public async Task<List<Appointment>> GetAllAsync()
    {
        return await _context.Appointments
            .Include(a => a.Department)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .ToListAsync();
    }

    public async Task<Appointment?> GetByIdAsync(int id)
    {
        return await _context.Appointments
            .Include(a => a.Department)
            .Include(a => a.Doctor)
            .Include(a => a.Patient)
            .FirstOrDefaultAsync(a => a.AppointmentId == id);
    }

    public async Task AddAsync(Appointment appointment)
    {
        _context.Appointments.Add(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Appointment appointment)
    {
        _context.Appointments.Update(appointment);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var appointment = await _context.Appointments.FindAsync(id);
        if (appointment != null)
        {
            _context.Appointments.Remove(appointment);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Appointment>> GetPagedAsync(string searchString, string sortOrder, int pageNumber, int pageSize)
    {
        var query = _context.Appointments
            .Include(a => a.Department)
            .Include(a => a.Doctor)
                .ThenInclude(d => d.Employee)
                    .ThenInclude(e => e.User)
            .Include(a => a.Patient)
                .ThenInclude(p => p.User)
            .AsQueryable();

        // Search
        if (!String.IsNullOrEmpty(searchString))
        {
            searchString = searchString.ToLower();
            query = query.Where(a =>
                a.Reason.ToLower().Contains(searchString) ||
                (a.Doctor.Employee.User.FirstName + " " + a.Doctor.Employee.User.LastName).ToLower().Contains(searchString) ||
                (a.Patient.User.FirstName + " " + a.Patient.User.LastName).ToLower().Contains(searchString));
        }

        // Sort
        switch (sortOrder)
        {
            case "date_desc":
                query = query.OrderByDescending(a => a.AppointmentDate);
                break;
            case "doctor_asc":
                query = query.OrderBy(a => a.Doctor.Employee.User.FirstName).ThenBy(a => a.Doctor.Employee.User.LastName);
                break;
            case "status_asc":
                query = query.OrderBy(a => a.Status);
                break;
            default:
                query = query.OrderBy(a => a.AppointmentDate);
                break;
        }

        return await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<int> GetTotalCountAsync()
    {
        return await _context.Appointments.CountAsync();
    }

    public async Task<int> GetCountByStatusAsync(string status)
    {
        return await _context.Appointments.CountAsync(a => a.Status == status);
    }

    public async Task<int> GetActiveCountAsync()
    {
        return await _context.Appointments.CountAsync(a => a.IsDeleted == false);
    }
}