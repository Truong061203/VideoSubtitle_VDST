using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
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

        public string RootPath { get; private set; }
        public string ExePath { get; private set; }

        private readonly string _fileName = "yt-dlp.exe";
        public string STABLE_VERSION => "2025.06.30";
        private string _url = string.Empty;

        private YoutubeRepository()
        {
            GetRootPath();
            _url = $"https://github.com/yt-dlp/yt-dlp/releases/download/{STABLE_VERSION}/yt-dlp.exe";
        }


        public string RegexYoutubeId(string url)
        {
            string pattern = "(youtu.*be.*)/(watch\\?v=|embed/|v|shorts|)(.*?((?=[&#?])|$))";
            var reg = Regex.Match(url, pattern);
            if (reg.Success)
                return reg.Groups[3].Value;
            return string.Empty;
        }


        /// <summary>
        /// get Root Path lưu trữ Tool yt-dlp.exe
        /// </summary>
        private void GetRootPath()
        {
            // lấy đường dẫn trong app và tạo 1 folder Youtubes để lưu trữ
            var currentDir = AppContext.BaseDirectory;         
            RootPath = Path.Combine(currentDir, "Youtubes");
            if (!Directory.Exists(RootPath))
                Directory.CreateDirectory(RootPath);

            // khởi tạo biến tạm thời cho root path của yt-dlp.exe
            RootPath = Path.Combine(RootPath, _fileName);
            Log.Information("Youtube Root Path: {RootPath}", RootPath);
        }

        /// </summary>
     

        /// <summary>
        /// Download Tool yt-dlp.exe từ GitHub
        /// </summary>
        public async Task DownloadAsync()
        {
            // nếu có roothPath rồi thì thôi
            if (File.Exists(RootPath))
                return;
            try
            {
               
                using (var client = new HttpClient())
                {
                    // Thêm timeout để tránh treo nếu mạng lag
                    client.Timeout = TimeSpan.FromMinutes(10);

                    var response = await client.GetAsync(_url);
                    if (response.IsSuccessStatusCode)
                    {
                        var fileBytes = await response.Content.ReadAsByteArrayAsync();
                        Log.Information("Read FileBytes :"+fileBytes);

                        // Xóa file cũ nếu có (Cập nhật)
                        if (File.Exists(RootPath))
                        {
                            File.Delete(RootPath);
                        }

                        // Ghi file mới
                        File.WriteAllBytes(RootPath, fileBytes);
                    }
                    else
                    {
                        throw new Exception($"Failed to download yt-dlp.exe. Status: {response.StatusCode}");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tải tool: {ex.Message}");
            }
        }

        /// <summary>
        /// Get Youtube Detail từ URL video
        /// </summary>
        public async Task<YoutubeDetailDTO> GetYoutubeDetailAsync(string url)
        {
            await DownloadAsync(); // Tự động tải nếu thiếu

            var cmd = $"{_fileName} -j \"{url}\""; 
            Log.Information("Get Youtube Detail CMD: {cmd}", cmd);
            var output = await RunCMDAsync(cmd); 
            Log.Information("Get Youtube Detail Output: {output}", output);
            try
            {
               return JsonConvert.DeserializeObject<YoutubeDetailDTO>(output);
            }
            catch 
            { 
                return null; 
            }
        }


        public async Task<string> DownloadVideoAsync(string url)
        {
            await DownloadAsync();

            var cmd = $"{_fileName} -f \"bestvideo[ext=mp4]+bestaudio[ext=m4a]/mp4\" --merge-output-format mp4 -o \"{RootPath}\\%(id)s.%(ext)s\" \"{url}\"";
            Log.Information("Download Video CMD: {cmd}", cmd);
            var output = await RunCMDAsync(cmd);

            // Hoặc dùng cách đơn giản: return ProcessOutputDownload(output, RegexYoutubeId(url));

            // Tạm thời trả về log để debug, bạn có thể áp dụng hàm ProcessOutputDownload sau
            return output;
        }


        private Task<string> RunCMDAsync(string cmd)
        {
            if (!File.Exists(RootPath))
            {
                // Nếu chưa có thì tải lại (Self-healing)
                DownloadAsync().Wait();
                if (!File.Exists(RootPath))
                    throw new Exception($"File {_fileName} không tìm thấy sau khi tải.");
            }
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {cmd}";
            process.StartInfo.WorkingDirectory = RootPath;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();
            return Task.Run(() => process.StandardOutput.ReadToEnd());
        }
        //private string ExtractVideoId(string url)
        //{
        //    // Logic đơn giản: Lấy sau dấu "=" hoặc "/" cuối cùng
        //    // (Bạn có thể dùng Regex xịn hơn nếu muốn)
        //    if (url.Contains("v=")) return url.Split(new[] { "v=" }, StringSplitOptions.None)[1].Split('&')[0];
        //    if (url.Contains("youtu.be/")) return url.Split(new[] { "youtu.be/" }, StringSplitOptions.None)[1].Split('?')[0];
        //    return "";
        //}
    }
}
