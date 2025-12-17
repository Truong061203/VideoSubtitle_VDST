using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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

        public string GetProjectFolderPath(ProjectInfoModel project)
        {
            return project.GetFullPath();
        }
      
        public string CreateProjectFolder(ProjectInfoModel project)
        {

            string workDir = project.GetFullPath();
            Log.Information("Tạo thư mục dự án tại: {WorkDir}", workDir);

            // 2. Tạo thư mục (idempotent, nếu đã tồn tại cũng không lỗi) 
            try
            {
                Directory.CreateDirectory(workDir);
                Directory.CreateDirectory(Path.Combine(workDir, "Input"));
                Directory.CreateDirectory(Path.Combine(workDir, "Videos"));
                Directory.CreateDirectory(Path.Combine(workDir, "Temp"));
                // 3. Tạo đường dẫn file .VDST trong thư mục đó
                string projectFilePath = Path.Combine(workDir, project.FolderName + ".VDST");

                // 4. Lưu cấu hình dự án vào file .VDST
                SaveProject(project, projectFilePath);

                return projectFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tạo thư mục dự án: " + ex.Message);
                throw;
            }
        }

        public void SaveProject(ProjectInfoModel project, string path)
        {
            // Dùng Newtonsoft.Json để lưu toàn bộ thông tin dự án xuống file
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);

            Log.Information($"Lưu dự án tại: {path}");
            Log.Information($"Nội dung dự án:\n{json}");
            File.WriteAllText(path, json);
        }



        // LƯU DỰ ÁN


        // ĐỌC DỰ ÁN
        public ProjectInfoModel LoadProject(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                // 1. Đọc file
                string json = File.ReadAllText(filePath);

                // 2. Chuyển đổi ngược lại thành Object
                var project = JsonConvert.DeserializeObject<ProjectInfoModel>(json);
                return project;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc dự án: {ex.Message}");
            }
        }
    }
}
