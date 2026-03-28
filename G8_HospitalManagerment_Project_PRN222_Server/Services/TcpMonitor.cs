using System;
using System.Collections.Generic;

namespace G8_HospitalManagerment_Project_PRN222_Server.Services
{
    public static class TcpMonitor
    {
        // Danh sách lưu trữ các dòng Log kết nối mạng
        public static List<string> Logs = new List<string>();

        public static void AddLog(string message)
        {
            string logEntry = $"[{DateTime.Now:HH:mm:ss}] {message}";

            // Thêm vào đầu danh sách để cái mới nhất hiện lên trên
            Logs.Insert(0, logEntry);

            // Giới hạn tối đa 20 dòng để tránh tràn bộ nhớ RAM
            if (Logs.Count > 20)
            {
                Logs.RemoveAt(20);
            }
        }
    }
}