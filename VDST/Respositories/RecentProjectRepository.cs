using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDST.Model;

namespace VDST.Repositories
{
    public static class RecentProjectRepository
    {
        // HÀM 1: THÊM VÀO DANH SÁCH (Trong RAM)
        public static void OnAddRecentPath(RecentProjectModel model, string projectPath, int maxLimit = 20)
        {
            // 1. Kiểm tra file dự án có thật sự tồn tại không?
            if (!File.Exists(projectPath)) return;

            // 2. Tìm xem dự án này đã có trong list chưa?
            var existingItem = model.Recents.FirstOrDefault(x => x.Path == projectPath);

            if (existingItem != null)
            {
                // CÓ RỒI -> Chỉ cần cập nhật thời gian mới nhất
                //existingItem.LastUsed = DateTime.Now;

                // (Mẹo: Muốn đưa lên đầu thì xóa đi add lại hoặc sort lại sau)
                model.Recents.Remove(existingItem);
                model.Recents.Insert(0, existingItem);
            }
            else
            {
                // CHƯA CÓ -> Tạo mới
                var newItem = new RecentProjectPlusModel
                {
                    Path = projectPath,
                    Name = Path.GetFileNameWithoutExtension(projectPath), // Lấy tên file làm tên dự án
                    //LastUsed = DateTime.Now
                };

                // Thêm vào đầu danh sách
                model.Recents.Insert(0, newItem);
            }

            // 3. Kiểm tra giới hạn (Ví dụ chỉ nhớ 20 cái)
            if (model.Recents.Count > maxLimit)
            {
                // Xóa bớt mấy cái cũ nhất ở dưới đáy
                model.Recents.RemoveRange(maxLimit, model.Recents.Count - maxLimit);
            }
        }

        // HÀM 2: LƯU XUỐNG Ổ CỨNG
        public static void OnSave(RecentProjectModel model, string savePath)
        {
            try
            {
                string json = JsonConvert.SerializeObject(model, Formatting.Indented);

                File.WriteAllText(savePath, json);
            }
            catch (Exception ex)
            {
                // Log lỗi nếu không lưu được (ví dụ ổ cứng đầy, không có quyền)
                // Log.Error("Lỗi lưu lịch sử: " + ex.Message);
            }
        }

        // HÀM 3: ĐỌC TỪ Ổ CỨNG LÊN (Dùng lúc mở App)
        public static RecentProjectModel OnLoad(string loadPath)
        {
            if (!File.Exists(loadPath)) return new RecentProjectModel();

            try
            {
                string json = File.ReadAllText(loadPath);
                var data = JsonConvert.DeserializeObject<RecentProjectModel>(json);

                // Validate lại: Chỉ giữ lại những file còn tồn tại trên ổ cứng
                // (Tránh trường hợp user đã xóa file thủ công)
                if (data != null)
                {
                    data.Recents = data.Recents.Where(x => File.Exists(x.Path)).ToList();
                }

                return data ?? new RecentProjectModel();
            }
            catch
            {
                return new RecentProjectModel();
            }
        }
    }
}
