using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDST.ViewModel;

namespace VDST.Model
{
    public class RecentProjectModel : ViewModelBase
    {
        private static RecentProjectModel _instance;
        public static RecentProjectModel Instance => _instance ??= new RecentProjectModel();

        // Danh sách các dự án
        public List<RecentProjectPlusModel> Recents { get; set; } = new List<RecentProjectPlusModel>();
    }

    /// <summary>
    /// dòng lịch sử của dự án gần đây
    /// </summary>
    public class RecentProjectPlusModel
    {
        public string Path { get; set; }       // Đường dẫn file .VDST
        public string Name { get; set; }       // Tên dự án
        public DateTime LastUsed { get; set; } // Thời gian mở cuối cùng

        // (Có thể thêm các thông tin phụ để hiển thị đẹp hơn)
        public string SizeDisplay { get; set; }
    }
}
