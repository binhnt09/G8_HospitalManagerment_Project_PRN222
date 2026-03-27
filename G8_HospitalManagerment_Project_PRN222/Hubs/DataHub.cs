using Microsoft.AspNetCore.SignalR;

namespace G8_HospitalManagerment_Project_PRN222.Hubs
{
    public class DataHub : Hub
    {
        // Hiện tại chúng ta không cần viết hàm gì ở đây cả. 
        // Vì Server chỉ làm nhiệm vụ "phát" (Broadcast) tín hiệu một chiều xuống Client.
    }
}