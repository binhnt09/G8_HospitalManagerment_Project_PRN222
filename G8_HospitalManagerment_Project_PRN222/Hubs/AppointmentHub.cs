using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace G8_HospitalManagerment_Project_PRN222.Hubs
{
    public class AppointmentHub : Hub
    {
        // Hub methods can be added here if clients need to send messages to the server directly
        // Currently, we will just use the hub context to broadcast messages from controllers/services
    }
}
