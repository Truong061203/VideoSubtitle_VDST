using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
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
using VDST.Respositories;

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
        public string RootPathExe { get; private set; }

        private readonly string _fileName = "yt-dlp.exe";
        private string _url = "https://github.com/yt-dlp/yt-dlp/releases/download/2025.11.12/yt-dlp.exe";

        public string TempDownloadDir { get; private set; }
        private YoutubeRepository()
        {
            GetRootPath();
        }


        /// <summary>
        /// get Root Path lưu trữ Tool yt-dlp.exe
        /// </summary>
        private void GetRootPath()
        {
            // lấy đường dẫn trong app và tạo 1 folder Youtubes để lưu trữ
            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            RootDir = Path.Combine(currentDir, "Youtubes");
            if (!Directory.Exists(RootDir))
                Directory.CreateDirectory(RootDir);

            // khởi tạo biến tạm thời cho root path của yt-dlp.exe
            RootPathExe = Path.Combine(RootDir, _fileName);
            Log.Information("Youtube Root Path: {RootPath}", RootDir);

            // Thư mục TEMP riêng cho video tạm
            TempDownloadDir = Path.Combine(Path.GetTempPath(), "VDST", "YoutubeDownloads");
            if (!Directory.Exists(TempDownloadDir))
                Directory.CreateDirectory(TempDownloadDir);
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
        /// Download Tool yt-dlp.exe từ GitHub
        /// </summary>
        public async Task DownloadYtbAsync()
        {

            if (File.Exists(RootPathExe))
                return;
            using (var client = new HttpClient())
            {
                var response = await client.GetAsync(_url);
                if (response.IsSuccessStatusCode)
                {
                    var fileBytes = await response.Content.ReadAsByteArrayAsync();
                    File.WriteAllBytes(RootPathExe, fileBytes);
                }
                else
                {
                    throw new Exception("Failed to download yt-dlp.exe");
                }
            }
        }
        private Process RunCMDWithProgress(string cmd)
        {
            if (!File.Exists(RootPathExe))
                throw new Exception($"{_fileName} not found");

            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {cmd}";
            process.StartInfo.WorkingDirectory = RootDir;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;   // đảm bảo đọc được stderr
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.StartInfo.StandardErrorEncoding = Encoding.UTF8;
            return process;
        }


        /// <summary>
        /// Tạo process chạy yt-dlp để tải video về thư mục RootDir,
        /// với tên file dạng "%(id)s.%(ext)s"
        /// </summary>
        // Trong YoutubeRepository.cs
        // Trong YoutubeRepository.cs

        public Process DownloadVideoAsyncWithProgress(string url)
        {
            var outTemplate = Path.Combine(TempDownloadDir, "%(id)s.%(ext)s");
            string ffmpegDir = FFmpegRepositories.Instance.FfmpegDir;

            if (!string.IsNullOrEmpty(ffmpegDir) && Path.HasExtension(ffmpegDir))
            {
                ffmpegDir = Path.GetDirectoryName(ffmpegDir);
            }

            string randomUA = VDST.Helpers.UserAgentHelper.GetRandomUserAgent();

            var cmd = $"{_fileName} " +
              $"-f \"bv*[ext=mp4][vcodec^=avc1]+ba[ext=m4a]/bv*[vcodec^=avc1]+ba/b[ext=mp4]/b\" " +
              $"--merge-output-format mp4 " +
              $"--ffmpeg-location \"{ffmpegDir}\" " +
              $"--no-warnings " +
              $"--user-agent \"{randomUA}\" " +
              $"-o \"{outTemplate}\" " +
              $"\"{url}\"";

            Log.Information("Running optimized download: {cmd}", cmd);
            return RunCMDWithProgress(cmd);
        }

        /// <summary>
        /// Phân tích output của yt-dlp để tìm tên file .mp4 đã tải về trong thư mục RootDir
        /// </summary>
        public string ProcessOutputDownload(string output, string videoId)
        {

            var expectedPath = Path.Combine(TempDownloadDir, $"{videoId}.mp4");
            if (File.Exists(expectedPath))
                return expectedPath;

            if (Directory.Exists(TempDownloadDir))
            {
                var files = Directory.GetFiles(TempDownloadDir, $"*{videoId}*");
                // Ưu tiên mp4
                var mp4 = files.FirstOrDefault(f => f.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase));
                if (mp4 != null)
                    return mp4;

                // Lấy file bất kỳ nếu không có mp4 (mkv, webm...)
                if (files.Length > 0)
                    return files[0];
            }
            return string.Empty;
        }


        /// <summary>
        /// Run CMD bất đồng bộ và trả về stdout
        /// </summary>
        private async Task<string> RunCMDAsync(string cmd)
        {

            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {cmd}";
            process.StartInfo.WorkingDirectory = RootDir;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var err = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp error: {err}");
            return output;
        }

        /// <summary>
        /// Get Youtube Detail từ URL video
        /// </summary>
        // Trong YoutubeRepository.cs

        public async Task<YoutubeDetailDTO> GetYoutubeDetailAsync(string url)
        {
            // 1. Đảm bảo tool đã có
            await DownloadYtbAsync();

            // 2. Cấu hình Retry (Thử lại tối đa 3 lần nếu thất bại)
            int maxRetries = 3;
            int currentRetry = 0;
            string output = string.Empty;

            while (currentRetry < maxRetries)
            {
                try
                {
                    currentRetry++;

                    string randomUA = VDST.Helpers.UserAgentHelper.GetRandomUserAgent();

                    var cmd = $"{_fileName} " +
                              $"--dump-json " +
                              $"--no-warnings " +
                              $"--socket-timeout 10 " +
                              $"--user-agent \"{randomUA}\" " +
                              $"\"{url}\"";

                    Log.Information($"[Attempt {currentRetry}] Getting Info: {url}");

                    // Gọi lệnh
                    output = await RunCMDAsync(cmd);

                    // Nếu lấy được dữ liệu -> Thoát vòng lặp ngay
                    if (!string.IsNullOrWhiteSpace(output))
                    {
                        break;
                    }
                }
                catch (Exception ex)
                {
                    Log.Warning($"Lần thử {currentRetry} thất bại: {ex.Message}");

                    if (currentRetry >= maxRetries) throw;

                    await Task.Delay(1500);
                }
            }

            if (string.IsNullOrWhiteSpace(output)) return null;

            try
            {
                var dto = JsonConvert.DeserializeObject<Model.Youtubes.YoutubeDetailDTO>(output);
                if (dto == null) return null;

                // Xử lý Subtitle (Code cũ của bạn giữ nguyên đoạn này ok)
                var subtiles = GetSubtitles(output, "subtitles");
                if (subtiles != null) dto.OriginSubs = subtiles.ToArray();

                var autoSubtiles = GetSubtitles(output, "automatic_captions");
                if (autoSubtiles != null) dto.AutoSubs = autoSubtiles.ToArray();

                return dto;
            }
            catch (JsonException jsonEx)
            {
                Log.Error(jsonEx, "Lỗi Parse JSON từ yt-dlp");
                return null;
            }
        }

        /// <summary>
        /// Get Subtitles từ JSON output của yt-dlp
        /// </summary>
        /// <returns></returns>
        private List<Model.Youtubes.SubtitleDTO> GetSubtitles(string output, string key)
        {
            var root = JObject.Parse(output);

            var subsObj = (JObject)root[key];
            if (subsObj == null)
                return null;

            var list = new List<SubtitleDTO>();

            //3: Duyệt từng ngôn ngữ ("en", "vi")
            foreach (var lang in subsObj)
            {
                var langCode = lang.Key;
                var arr = (JArray)lang.Value;

                //4: Duyệt từng subtitle trong mảng
                foreach (var item in arr)
                {
                    list.Add(new SubtitleDTO
                    {
                        LanguageCode = langCode,
                        Name = (string)item["name"],
                        Ext = (string)item["ext"],
                        Url = (string)item["url"]
                    });
                }
            }
            return list;
        }
    }
}
