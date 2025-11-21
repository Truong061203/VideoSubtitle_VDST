using Newtonsoft.Json;
using Serilog;
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

        public void CreateProjectFolder(ProjectInfo project)
        {
            //    string workDir = project.GetFullPath();

            //    // Tạo các thư mục
            //    //Directory.CreateDirectory(workDir);
            //    //Directory.CreateDirectory(Path.Combine(workDir, "input"));
            //    //Directory.CreateDirectory(Path.Combine(workDir, "output"));
            //    //Directory.CreateDirectory(Path.Combine(workDir, "audio"));
            //    //Directory.CreateDirectory(Path.Combine(workDir, "temp"));

            string workDir = project.GetFullPath();

            if (!Directory.Exists(workDir))
            {
                Directory.CreateDirectory(workDir); // Chỉ tạo đúng 1 folder này
            }

            // 2. Tạo đường dẫn file .VDST
            string configPath = Path.Combine(workDir, project.FolderName + ".VDST");

            // 3. Lưu file
            SaveProject(project, configPath);
           
        }

        public void SaveProject(ProjectInfo project, string path)
        {
            // Dùng Newtonsoft.Json để lưu toàn bộ thông tin dự án xuống file
            var json = JsonConvert.SerializeObject(project, Formatting.Indented);

            Log.Information($"Lưu dự án tại: {path}");
            Log.Information($"Nội dung dự án:\n{json}");
            File.WriteAllText(path, json);
        }



        // LƯU DỰ ÁN


        // ĐỌC DỰ ÁN
        public ProjectInfo LoadProject(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            try
            {
                // 1. Đọc file
                string json = File.ReadAllText(filePath);

                // 2. Chuyển đổi ngược lại thành Object
                var project = JsonConvert.DeserializeObject<ProjectInfo>(json);
                return project;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc dự án: {ex.Message}");
            }
        }
    }
}
