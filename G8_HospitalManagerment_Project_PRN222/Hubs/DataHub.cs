using Microsoft.AspNetCore.SignalR;

namespace G8_HospitalManagerment_Project_PRN222.Hubs
{
    public class DataHub : Hub
    {
        // Hiện tại chúng ta không cần viết hàm gì ở đây cả. 
        // Vì Server chỉ làm nhiệm vụ "phát" (Broadcast) tín hiệu một chiều xuống Client.
        //public async Task NotifyLabOrderChanged(string patientName)
        //{
        //    // Truyền thêm patientName vào tin nhắn gửi đi
        //    await Clients.All.SendAsync("ReceiveLabOrderUpdate", patientName);
        //}
        public async Task NotifyLabOrderChanged()
        {
            // Truyền thêm patientName vào tin nhắn gửi đi
            await Clients.All.SendAsync("ReceiveLabOrderUpdate");
        }

        // 2. Dành cho Chẩn đoán hình ảnh (Imaging)
        public async Task NotifyImagingOrderChanged(string patientName)
        {
            // Tách riêng tên sự kiện và truyền patientName
            await Clients.All.SendAsync("ReceiveImagingOrderUpdate", patientName);
        }

        // Hàm này sẽ được Javascript ở Client gọi khi tải trang xong
        public async Task JoinGroup(string groupName)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task LeaveGroup(string groupName)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }
    }
}