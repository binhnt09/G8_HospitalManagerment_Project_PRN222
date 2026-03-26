using G8_HospitalManagerment_Project_PRN222.Models;

namespace G8_HospitalManagerment_Project_PRN222.DTOs
{
    public class IndexDTO
    {
        public List<LabOrder>? PagedData { get; set; }
        public List<Test>? TestPagedData { get; set; }

        // Dữ liệu thống kê
        public int TotalOrders { get; set; }
        public int CompletedCount { get; set; }
        public int PendingCount { get; set; }
        public int ActiveCount { get; set; }

        // Dữ liệu phân trang
        public int TotalPages { get; set; }
        public int ItemStart { get; set; }
        public int ItemEnd { get; set; }
    }
}
