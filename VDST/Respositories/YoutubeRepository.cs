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
        }

        /// <summary>
        /// Download Tool yt-dlp.exe từ GitHub
        /// </summary>
        public async Task DownloadAsync()
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
            await DownloadAsync();

            // 2. Gọi yt-dlp để lấy JSON
            var cmd = $"{_fileName} -j \"{url}\"";
            Log.Information("Running command: {cmd}", cmd);

            var output = await RunCMDAsync(cmd);
            if (string.IsNullOrWhiteSpace(output))
                return null;

            // 3. Parse JSON thành DTO
            var dto = JsonConvert.DeserializeObject<Model.Youtubes.YoutubeDetailDTO>(output);
            Log.Information("Youtube Detail DTO: {dto}", dto);
            if (dto == null)
                return null;

            // 4. Lấy subtitles & auto-subtitles
            var subtiles = GetSubtitles(output, "subtitles");
            Log.Information("Original Subtitles: {subtiles}", subtiles);
            if (subtiles != null)
                dto.OriginSubs = subtiles.ToArray();

            //5: Lấy subtitles gốc(subtitles) và gán vào DTO
            var autoSubtiles = GetSubtitles(output, "automatic_captions");
            Log.Information("Automatic Subtitles: {autoSubtiles}", autoSubtiles);
            if (autoSubtiles != null)
                dto.AutoSubs = autoSubtiles.ToArray();

            return dto;

        }


        public async Task DownloadVideoAsync(string url, string outputPath)
        {
            try
            {
                await DownloadAsync();
                var dir = Path.GetDirectoryName(outputPath);
                Log.Information("Download video Youtube từ {Url} về {OutputPath}", url, outputPath);
                if (!Directory.Exists(dir))
                    Directory.CreateDirectory(dir);

                // Nếu file đã tồn tại → xóa trước
                if (File.Exists(outputPath))
                    File.Delete(outputPath);

                // -f mp4: chọn mp4 tốt nhất, -o: output path
                var args = $"-f mp4 -o \"{outputPath}\" \"{url}\"";
                await RunCMDAsync(args);
            }
            catch (Exception ex)
            {
                Log.Error("Lỗi khi tải video Youtube: {Message}", ex.Message);
                throw;
            }
        }





        private List<Model.Youtubes.SubtitleDTO> GetSubtitles(string output, string key)
        {
            //1: Parse chuỗi JSON gốc
            var root = JObject.Parse(output);

            //2: Lấy object con theo key: "subtitles" hoặc "automatic_captions"
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
