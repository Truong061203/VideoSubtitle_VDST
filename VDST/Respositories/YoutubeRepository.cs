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
        /// 
        /// </summary>
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

        /// <summary>
        /// Download Tool yt-dlp.exe từ GitHub
        /// </summary>
        public async Task DownloadYtbAsync()
        {
            // nếu có roothPath rồi thì thôi
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
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;
            return process;
        }

        /// <summary>
        /// Tạo process chạy yt-dlp để tải video về thư mục RootDir,
        /// với tên file dạng "%(id)s.%(ext)s"
        /// </summary>
        // Trong YoutubeRepository.cs

        public Process DownloadVideoAsyncWithProgress(string url)
        {

            var outTemplate = Path.Combine(TempDownloadDir, "%(id)s.%(ext)s");


            string ffmpegLocation = FFmpegRepositories.Instance.FfmpegDir;


            var cmd = $"{_fileName} -f \"bv*+ba/b\" --merge-output-format mp4 " +
                      $"--ffmpeg-location \"{ffmpegLocation}\" " +
                      $"--no-warnings -o \"{outTemplate}\" \"{url}\"";

            Log.Information("Running download command: {cmd}", cmd);
            var process = RunCMDWithProgress(cmd);
            return process;
        }


        /// <summary>
        /// Phân tích output của yt-dlp để tìm tên file .mp4 đã tải về trong thư mục RootDir
        /// </summary>
        public string ProcessOutputDownload(string output, string id)
        {
            // 1. Ưu tiên file có tên [id].mp4 vì đã cấu hình outTemplate như vậy
            var finalPath = Path.Combine(TempDownloadDir, $"{id}.mp4");
            if (File.Exists(finalPath))
                return finalPath;

            // 2. Nếu vì lý do nào đó tên không đúng id.mp4, thử parse log để lấy file mp4 bất kỳ
            var lines = output?.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
                        ?? Array.Empty<string>();
            var regex = new Regex(@".{1,}""(.{1,})""");

            foreach (var line in lines)
            {
                var match = regex.Match(line);
                if (!match.Success)
                    continue;

                var fileName = match.Groups[1].Value?.Trim();
                if (string.IsNullOrEmpty(fileName))
                    continue;

                if (!fileName.EndsWith(".mp4", StringComparison.OrdinalIgnoreCase))
                    continue;

                string path;
                if (Path.IsPathRooted(fileName))
                {

                    path = fileName;
                }
                else
                {
                    // chỉ in file name → ghép vào TempDownloadDir
                    path = Path.Combine(TempDownloadDir, fileName);
                }

                if (File.Exists(path))
                    return path;
            }

            // 3. Fallback cuối: tìm file *.mp4 nào chứa id trong tên
            var files = Directory.GetFiles(TempDownloadDir, $"*{id}*.mp4");
            if (files.Length > 0)
                return files[0];

            return string.Empty;
        }


        /// <summary>
        /// Run CMD bất đồng bộ và trả về stdout
        /// </summary>
        private async Task<string> RunCMDAsync(string cmd)
        {
            // 1: Cấu hình process để chạy "cmd /c [lệnh]"
            var process = new Process();
            process.StartInfo.FileName = "cmd.exe";
            process.StartInfo.Arguments = $"/c {cmd}";
            process.StartInfo.WorkingDirectory = RootDir;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardError = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.StandardOutputEncoding = Encoding.UTF8;

            //2: Chạy và đọc output + error
            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var err = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            // B3: Nếu lỗi → ném exception, ngược lại trả về stdout (JSON)
            if (process.ExitCode != 0)
                throw new Exception($"yt-dlp error: {err}");

            return output;
        }



        /// <summary>
        /// Get Youtube Detail từ URL video
        /// </summary>
        public async Task<YoutubeDetailDTO> GetYoutubeDetailAsync(string url)
        {
            //1. check xem đã có yt-dlp.exe chưa và tải về nếu chưa có
            await DownloadYtbAsync();

            // 2. Gọi yt-dlp để lấy JSON
            var cmd = $"{_fileName} -j \"{url}\"";
            Log.Information("Running command: {cmd}", cmd);

            var output = await RunCMDAsync(cmd);
            if (string.IsNullOrWhiteSpace(output))
                return null;

            // 3. Parse JSON thành DTO
            var dto = JsonConvert.DeserializeObject<Model.Youtubes.YoutubeDetailDTO>(output);

            if (dto == null)
                return null;

            // 4. Lấy subtitles & auto-subtitles
            var subtiles = GetSubtitles(output, "subtitles");
            Log.Information("Original Subtitles: {subtiles}", subtiles);
            if (subtiles != null)
                dto.OriginSubs = subtiles.ToArray();

            //5: Lấy subtitles gốc(subtitles) và gán vào DTO
            var autoSubtiles = GetSubtitles(output, "automatic_captions");
            Log.Information("Automatic Subtitles: {autoSubtiles}", autoSubtiles.ToString());
            if (autoSubtiles != null)
                dto.AutoSubs = autoSubtiles.ToArray();

            return dto;

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
