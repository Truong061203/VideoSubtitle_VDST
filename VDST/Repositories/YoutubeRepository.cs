using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VDST.Model.Youtubes;

namespace VDST.Repositories
{
    public class YoutubeRepository
    {
        private static YoutubeRepository _instance;
        public static YoutubeRepository Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new YoutubeRepository();
                }
                return _instance;
            }
        }

        public string RootDir { get; private set; }
        public string ExePath { get; private set; }

        private readonly string _exeName = "yt-dlp.exe";

        private YoutubeRepository()
        {
            // Tạo thư mục chứa tool: bin/Debug/net6.0/Youtubes/yt-dlp.exe
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            Log.Information("Current Directory: {CurrentDir}", currentDir);
            RootDir = Path.Combine(currentDir, "Youtubes");
            if (!Directory.Exists(RootDir)) Directory.CreateDirectory(RootDir);

            ExePath = Path.Combine(RootDir, _exeName);
        }

        // 1. Hàm kiểm tra xem đã có tool chưa
        public bool IsToolExist() => File.Exists(ExePath);

        // 2. Hàm lấy thông tin Video (Chỉ lấy JSON, không tải)
        public async Task<YoutubeDetailDTO> GetYoutubeDetailAsync(string url)
        {
            // Lệnh: yt-dlp.exe -j "url"
            // -j nghĩa là dump json (trả về thông tin dạng chữ)
            var cmd = $"{_exeName} -j \"{url}\"";
            var output = await RunCMDAsync(cmd);

            try
            {
                return JsonConvert.DeserializeObject<YoutubeDetailDTO>(output);
            }
            catch
            {
                return null; // Link sai hoặc không lấy được
            }
        }

        // 3. Hàm tải Video thật
        public async Task<string> DownloadVideoAsync(string url)
        {
            // Tạo tên file output: ID.mp4
            // Lệnh: yt-dlp.exe -f "bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4" -o "Folder/%(id)s.%(ext)s" "url"
            var cmd = $"{_exeName} -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{RootDir}\\%(id)s.%(ext)s\" \"{url}\"";

            var output = await RunCMDAsync(cmd);

            // (Logic tìm file sau khi tải - Giống Senior)
            // Đơn giản là mình tìm trong folder xem file nào mới được tạo ra
            // Ở đây trả về output để ViewModel xử lý hoặc tìm file dựa trên ID
            return output;
        }

        // Hàm chạy lệnh CMD ngầm (Core)
        private Task<string> RunCMDAsync(string cmd)
        {
            if (!IsToolExist()) throw new Exception($"Không tìm thấy file {_exeName}");

            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {cmd}";
            process.StartInfo.WorkingDirectory = RootDir; // Chạy tại thư mục chứa exe
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            process.Start();

            // Đọc kết quả trả về
            return Task.Run(() => process.StandardOutput.ReadToEnd());
        }
    }
}
