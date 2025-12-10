using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using VDST.ViewModel;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace VDST.Respositories
{
    public class FFmpegRepositories : ViewModelBase
    {
        private static FFmpegRepositories _instance;
        public static FFmpegRepositories Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FFmpegRepositories();
                }
                return _instance;
            }
        }

        private bool _isInstalled;
        public bool IsInstalled
        {
            get => _isInstalled;
            private set
            {
                if (_isInstalled != value)
                {
                    _isInstalled = value;
                    Log.Information("FFmpeg installation status changed to: {IsInstalled}", _isInstalled);
                    RaisePropertyChanged(nameof(IsInstalled));
                }
            }
        }

        public string FfmpegDir { get; private set; }


        private FFmpegRepositories()
        {
            InitFfmpegPath();
        }
        /// <summary>
        /// Khởi tạo đường dẫn FFmpeg
        /// </summary>
        public void InitFfmpegPath()
        {
            var currentDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            FfmpegDir = Path.Combine(currentDir, "FFmpeg");

            if (!Directory.Exists(FfmpegDir))
            {
                Directory.CreateDirectory(FfmpegDir);
            }

            // Nói cho Xabe.FFmpeg biết ffmpeg.exe, ffprobe.exe sẽ nằm ở đây
            FFmpeg.SetExecutablesPath(FfmpegDir);
            IsInstalled = CheckFfmpegInstalled();
        }

        private bool CheckFfmpegInstalled()
        {
            var ffmpegPath = Path.Combine(FfmpegDir, "ffmpeg.exe");
            Log.Information("Checking FFmpeg at path: {FfmpegPath}", ffmpegPath);
            var ffprobePath = Path.Combine(FfmpegDir, "ffprobe.exe");
            Log.Information("Checking FFprobe at path: {FfprobePath}", ffprobePath);

            // Tối thiểu là phải có ffmpeg.exe
            return File.Exists(ffmpegPath);
            // Nếu muốn chặt chẽ hơn:
            // return File.Exists(ffmpegPath) && File.Exists(ffprobePath);
        }
       

        public async Task DownloadFfmpegAsync()
        {
            if (IsInstalled)
            {
                Log.Information("FFmpeg đã có sẵn trong thư mục: {FfmpegPath}", FfmpegDir);
                return;
            }

            try
            {
                Log.Information("Đang tải FFmpeg vào: {Folder}", FfmpegDir);
                await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, FfmpegDir);

                IsInstalled = CheckFfmpegInstalled();
                if (IsInstalled)
                {
                    Log.Information("FFmpeg sẵn sàng tại: {Path}", FfmpegDir);
                }
                else
                {
                    Log.Error("Tải FFmpeg xong nhưng không tìm thấy ffmpeg.exe");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi tải FFmpeg");
                throw; // cho ViewModel bắt và show MessageBox
            }
        }
        public async Task EnsureFfmpegAsync()
        {
            if (!IsInstalled)
            {
                await DownloadFfmpegAsync();
            }
        }
    }
}
