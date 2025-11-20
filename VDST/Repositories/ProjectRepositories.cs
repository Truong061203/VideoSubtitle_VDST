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
    public class ProjectRepositories
    {
        private static ProjectRepositories _instance;
        public static ProjectRepositories Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new ProjectRepositories();
                }
                return _instance;
            }
        }

        private ProjectRepositories() 
        { 
        }

        // LƯU DỰ ÁN
        public void SaveProject(ProjectInfoConfig project)
        {
            try
            {
                // 1. Tạo đường dẫn file .VDST
                // Ví dụ: D:\MyProject\Demo1\Demo1.VDST
                string filePath = Path.Combine(project.GetFullPath(), project.FolderName + ".VDST");

                // 2. Chuyển đổi sang JSON
                string json = JsonConvert.SerializeObject(project, Formatting.Indented);

                // 3. Ghi đè file cũ
                File.WriteAllText(filePath, json);
            }
            catch (Exception ex)
            {
                // Log lỗi (hoặc throw để ViewModel xử lý)
                throw new Exception($"Lỗi lưu dự án: {ex.Message}");
            }
        }

        // ĐỌC DỰ ÁN
        public ProjectInfoConfig LoadProject(string filePath)
        {
            if (!File.Exists(filePath)) return null;

            try
            {
                // 1. Đọc file
                string json = File.ReadAllText(filePath);

                // 2. Chuyển đổi ngược lại thành Object
                var project = JsonConvert.DeserializeObject<ProjectInfoConfig>(json);
                return project;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc dự án: {ex.Message}");
            }
        }
    }
}
