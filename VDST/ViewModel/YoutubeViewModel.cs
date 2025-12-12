using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VDST.Commands;
using VDST.Model.Youtubes;
using VDST.Repositories;
using VDST.Respositories;

namespace VDST.ViewModel
{
    public class YoutubeViewModel : ViewModelBase
    {
        private YoutubeModel _youtubeModel = new YoutubeModel();
        public YoutubeModel youtubeModel
        {
            get => _youtubeModel;
            set
            {
                _youtubeModel = value;
                RaisePropertyChanged(nameof(youtubeModel));
            }
        }

        private bool _isLoading = false;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                Log.Information("Isbusy :" + value);
                RaisePropertyChanged(nameof(IsLoading));

                (CheckCommand as VfxCommand)?.RaiseCanExecuteChanged();
                (DownloadVideoCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        private SubtitleDTO _selectedSubtitle;
        public SubtitleDTO SelectedSubtitle
        {
            get => _selectedSubtitle;
            set
            {
                _selectedSubtitle = value;
                RaisePropertyChanged(nameof(SelectedSubtitle));
                (DownloadSubtitleFileCommand as VfxCommand)?.RaiseCanExecuteChanged();
                (ExportSubtitleTextCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }


        private string _inputUrl;
        public string InputUrl
        {
            get => _inputUrl;
            set
            {
                if (_inputUrl != value)
                {
                    _inputUrl = value;
                    Log.Information("Input Url :" + InputUrl);
                    RaisePropertyChanged(nameof(InputUrl));
                    (CheckCommand as VfxCommand)?.RaiseCanExecuteChanged();
                }
            }
        } 

        private string _searchText; 
        public string SearchText 
        {
            get => _searchText; 
            set
            {
                if (_searchText != value)
                {
                    _searchText = value; 
                    RaisePropertyChanged(nameof(SearchText));
                }
            }
        }

        private string _currentTempFilePath;
        private Process _currentDownloadProcess;


        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        //public string DownloadedVideoPath { get; private set; }

        private readonly HttpClient _httpClient = new HttpClient();

        public ICommand CheckCommand { get; set; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand DownloadSubtitleFileCommand { get; }
        public ICommand ExportSubtitleTextCommand { get; }

        public YoutubeViewModel(string _url)
        {
            DownloadVideoCommand = new VfxCommand(OnDownloadVideo, CanDownloadVideo);
            CheckCommand = new VfxCommand(OnCheck, CanCheck);
            InputUrl = _url;

            if (_url != null)
            {
                LoadInfoAsync(_url);
            }
        }


        public void OnWindowClosing()
        {
            try
            {
                // Nếu process đang chạy → kill
                if (_currentDownloadProcess != null &&
                    !_currentDownloadProcess.HasExited)
                {
                    _currentDownloadProcess.Kill();
                }

                // Nếu có file tạm → xoá
                if (!string.IsNullOrEmpty(_currentTempFilePath) &&
                    File.Exists(_currentTempFilePath))
                {
                    File.Delete(_currentTempFilePath);
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error while cleaning temp on window closing");
            }
        }

        private bool CanDownloadVideo()
        {
            if (youtubeModel.Thumbnail != null && !IsLoading)
                return true;
            return false;
        }



        /// <summary>
        /// Download video from youtube
        /// </summary>
        private async void OnDownloadVideo(object obj)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Lưu video",
                Filter = "Video files (*.mp4)|*.mp4",
                RestoreDirectory = true,
            };

            if (dlg.ShowDialog() != true)
                return;

            var finalPath = dlg.FileName;
            IsLoading = true;
            _currentTempFilePath = null;
            _currentDownloadProcess = null;
            try
            {
                await FFmpegRepositories.Instance.EnsureFfmpegAsync();         // ffmpeg

                var process = YoutubeRepository.Instance.DownloadVideoAsyncWithProgress(youtubeModel.Url);
                _currentDownloadProcess = process;
                string output = string.Empty;
                await Task.Run(() =>
                {
                    try
                    {
                        process.OutputDataReceived += (sender, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                            {
                                output += e.Data + Environment.NewLine;
                            }
                        };

                        process.Start();
                        process.BeginOutputReadLine();  // đọc stdout async
                        process.WaitForExit();
                    }
                    finally
                    {
                        process.Close();
                        process.Dispose();
                    }
                });

                _currentDownloadProcess = null;

                // 4. Tìm file tạm (ở RootDir hoặc TempDownloadDir tuỳ bạn impl)
                var tempPath = YoutubeRepository.Instance.ProcessOutputDownload(output, youtubeModel.VideoId);

                if (string.IsNullOrEmpty(tempPath) || !File.Exists(tempPath))
                    throw new Exception("Không tìm được file video sau khi tải.");

                _currentTempFilePath = tempPath;

                // 5. Dùng FFmpeg check codec, convert nếu cần, rồi move sang finalPath
                var mediaInfo = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(tempPath);
                if (mediaInfo == null || mediaInfo.VideoStreams == null || !mediaInfo.VideoStreams.Any())
                {
                    throw new Exception("File tải xuống không có stream video");

                }
                var codec = mediaInfo.VideoStreams.FirstOrDefault().Codec;
                string encoder = "h264";
                string[] listSupportMp = { "h264", "wmv1", "wmv2", "wmv3", "mp1", "mp2", "mp3" };

                if (listSupportMp.Contains(codec))
                {
                    File.Move(tempPath, finalPath);
                }
                else
                {
                    var conversion = Xabe.FFmpeg.FFmpeg.Conversions.New();
                    string cmd = $" -y -i \"{tempPath}\" -c:v h264 -c:a aac \"{finalPath}\" ";

                    await conversion.Start(cmd);

                    File.Delete(tempPath);
                }

                // 6. Đã xong → không còn file tạm nữa
                _currentTempFilePath = null;

                System.Windows.MessageBox.Show("Tải video thành công!", "Information",
                                               MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Lỗi tải video",
                                               MessageBoxButton.OK, MessageBoxImage.Error);

                try
                {
                    if (!string.IsNullOrEmpty(_currentTempFilePath) &&
                        File.Exists(_currentTempFilePath))
                    {
                        File.Delete(_currentTempFilePath);
                    }
                }
                catch (Exception Ex)
                {
                    Log.Error(Ex, "Lỗi khi xoá file tạm sau khi tải lỗi");
                }
            }
            finally
            {
                IsLoading = false;
                _currentDownloadProcess = null;
            }
        }


        private bool CanCheck()
        {
            if (string.IsNullOrWhiteSpace(InputUrl) != null && !IsLoading)
                return true;
            return false;
        }


        /// <summary>
        /// Load Info with url
        /// </summary>
        private async Task LoadInfoAsync(string url)
        {
            try
            {
                IsLoading = true;

                // 1. Clear UI cũ cho đỡ “ảo”
                youtubeModel.Title = "Đang tải thông tin video...";
                youtubeModel.Thumbnail = null;
                youtubeModel.Subtitles.Clear();
                youtubeModel.SubFilters.Clear();
                youtubeModel.Url = url;

                // 2. Gọi repo lấy detail
                var detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(url);
                if (detail == null)
                {
                    System.Windows.MessageBox.Show("Không lấy được thông tin video.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                youtubeModel.VideoId = detail.Id;
                youtubeModel.Duration = TimeSpan.FromSeconds(detail.Duration);
                youtubeModel.Title = $"{detail.Title} [{youtubeModel.Duration:mm\\:ss}]";

                // 3. Download thumbnail
                using (var client = new HttpClient { Timeout = TimeSpan.FromSeconds(90) })
                {
                    byte[] imageData = await client.GetByteArrayAsync(detail.Thumbnail);
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    bitmap.Freeze();

                    youtubeModel.Thumbnail = bitmap;
                }

                // 4. Build subtitles
                var original = detail.OriginSubs;
                Log.Information("OriginSub :" + original.ToString());
                var auto = detail.AutoSubs;
                Log.Information("Auto :" + auto.ToString());

                if (original != null && original.Length > 0)
                {
                    var unique = original
                        .GroupBy(x => x.LanguageCode)
                        .Select(g => g.FirstOrDefault(s => s.Ext == "srt") ?? g.First())
                        .ToList();

                    foreach (var item in unique.Where(x => x != null))
                    {
                        youtubeModel.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-subs"
                        });
                    }
                }

                if (youtubeModel.Subtitles.Count == 0 && auto != null && auto.Length > 0)
                {
                    var unique = auto
                        .GroupBy(x => x.LanguageCode)
                        .Select(g => g.FirstOrDefault(s => s.Ext == "srt") ?? g.First())
                        .ToList();

                    foreach (var item in unique.Where(x => x != null))
                    {
                        youtubeModel.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-auto-subs"
                        });
                    }
                }

                foreach (var s in youtubeModel.Subtitles)
                    youtubeModel.SubFilters.Add(s);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// check url và load info
        /// </summary>
        /// <param name="obj"></param>
        private async void OnCheck(object obj)
        {
            try
            {
                var rawUrl = InputUrl?.Trim();
                if (string.IsNullOrEmpty(rawUrl))
                {
                    System.Windows.MessageBox.Show("Bạn chưa nhập link youtube.", "Link", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var id = YoutubeRepository.Instance.RegexYoutubeId(rawUrl);
                if (string.IsNullOrEmpty(id))
                {
                    System.Windows.MessageBox.Show("Link youtube không đúng format.\r\nVí dụ:\r\n" +
                                    "https://www.youtube.com/watch?v=[id]\r\n" +
                                    "https://www.youtube.com/shorts/[id]",
                                    "Link youtube", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var exactUrl = $"https://www.youtube.com/watch?v={id}";

                // Gán URL chuẩn cho model (để sau này DownloadVideo / DownloadSub dùng)
                youtubeModel.Url = exactUrl;

                await LoadInfoAsync(exactUrl);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnCheck error with url: {Url}", InputUrl);
                System.Windows.MessageBox.Show("Lỗi Url link{Url} :" + InputUrl, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

