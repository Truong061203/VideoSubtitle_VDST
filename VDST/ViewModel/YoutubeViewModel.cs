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
using Xabe.FFmpeg;

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
                (DownloadSubtitleFileCommand as VfxCommand)?.RaiseCanExecuteChanged();
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

        private string _searchSubText;
        public string SearchSubText
        {
            get => _searchSubText;
            set
            {
                if (_searchSubText != value)
                {
                    _searchSubText = value;
                    Log.Information("Search Text : " + SearchSubText);
                    FilterSubtitles();
                    RaisePropertyChanged(nameof(SearchSubText));
                }
            }
        }

        private string _currentTempFilePath;
        private Process _currentDownloadProcess;


        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        //public string DownloadedVideoPath { get; private set; }


        public ICommand CheckCommand { get; set; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand DownloadSubtitleFileCommand { get; }

        public YoutubeViewModel(string _url)
        {
            DownloadVideoCommand = new VfxCommand(OnDownloadVideo, CanDownloadVideo);
            DownloadSubtitleFileCommand = new VfxCommand(OnDownloadSubtitleFile, CanDownLoadSub);
            CheckCommand = new VfxCommand(OnCheck, CanCheck);
            InputUrl = _url;

            if (_url != null)
            {
                _ = LoadInfoAsync(_url);
            }
        }

        private bool CanDownLoadSub()
        {
            if (IsLoading)
                return false;
            return true;
        }


        /// <summary>
        /// Download file subtitle .srt
        /// </summary>
        private async void OnDownloadSubtitleFile(object obj)
        {

            if (obj is SubtitleFormatModel model)
            {
                var saveFileDialog = new Microsoft.Win32.SaveFileDialog()
                {
                    Title = "Lưu phụ đề",
                    Filter = "Subtitles files (*.srt)|*.srt",
                    RestoreDirectory = true,
                };

                if (saveFileDialog.ShowDialog() == true)
                {
                    var finalPath = saveFileDialog.FileName;


                    var tempFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid() + ".tmp");

                    try
                    {
                        IsLoading = true;
                        var httpClient = new HttpClient()
                        {
                            Timeout = TimeSpan.FromSeconds(60),
                        };
                        var response = await httpClient.GetAsync(model.Url);

                        if (response.IsSuccessStatusCode)
                        {
                            var content = await response.Content.ReadAsStringAsync();

                            if (string.IsNullOrWhiteSpace(content))
                                throw new Exception("Nội dung phụ đề rỗng.");


                            File.WriteAllText(tempFile, content);


                            if (File.Exists(finalPath))
                                File.Delete(finalPath);
                            File.Move(tempFile, finalPath);

                            // 5. Thông báo thành công & Mở thư mục
                            var result = System.Windows.MessageBox.Show("Tải file phụ đề thành công!\nBạn có muốn mở thư mục chứa file không?",
                                                         "Phụ đề", MessageBoxButton.YesNo, MessageBoxImage.Information);

                            if (result == MessageBoxResult.Yes)
                            {
                                // Mở Explorer và chọn sẵn file đó
                                Process.Start("explorer.exe", $"/select, \"{finalPath}\"");
                            }
                        }
                        else
                        {
                            throw new Exception($"Lỗi tải: {response.StatusCode}");
                        }
                    }
                    catch (Exception ex)
                    {
                        System.Windows.MessageBox.Show($"Lỗi: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        IsLoading = false;
                        if (File.Exists(tempFile))
                            File.Delete(tempFile);
                    }
                }
            }
        }

        /// <summary>
        /// Filter subtitle list
        /// </summary>
        public void FilterSubtitles()
        {
            if (youtubeModel.Subtitles == null || youtubeModel.Subtitles.Count == 0)
                return;

            youtubeModel.SubFilters.Clear();

            if (string.IsNullOrEmpty(SearchSubText))
            {
                foreach (var item in youtubeModel.Subtitles)
                {
                    youtubeModel.SubFilters.Add(item);
                }
            }
            else
            {
                // Trường hợp có tìm kiếm: Lọc từ GỐC -> Đổ vào HIỂN THỊ
                var keyword = SearchSubText.ToLower().Trim();

                var results = youtubeModel.Subtitles.Where(x =>
                    (x.Name != null && x.Name.ToLower().Contains(keyword)) ||
                    (x.Language != null && x.Language.ToLower().Contains(keyword))
                );

                foreach (var item in results)
                {
                    youtubeModel.SubFilters.Add(item);
                }
            }
        }

        /// <summary>
        /// close window cleanup
        /// </summary>
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
        // Trong YoutubeViewModel.cs

        private async void OnDownloadVideo(object obj)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Lưu video",
                Filter = "Video files (*.mp4)|*.mp4",
                FileName = $"Video.mp4"
            };

            if (dlg.ShowDialog() != true)
                return;

            var finalPath = dlg.FileName;
            IsLoading = true;
            _currentTempFilePath = null;
            _currentDownloadProcess = null;

            try
            {
                await FFmpegRepositories.Instance.EnsureFfmpegAsync();

                var process = YoutubeRepository.Instance.DownloadVideoAsyncWithProgress(youtubeModel.Url);
                _currentDownloadProcess = process;
                string outputLog = string.Empty;

                await Task.Run(() =>
                {
                    try
                    {
                        process.OutputDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                                outputLog += e.Data + "\n";
                        };
                        process.ErrorDataReceived += (s, e) =>
                        {
                            if (!string.IsNullOrEmpty(e.Data))
                                outputLog += e.Data + "\n";
                        };

                        process.Start();
                        process.BeginOutputReadLine();
                        process.BeginErrorReadLine();
                        process.WaitForExit();
                    }
                    finally
                    {
                        process.Dispose();
                    }
                });

                _currentDownloadProcess = null;

                // 4. Tìm file tạm
                var tempPath = YoutubeRepository.Instance.ProcessOutputDownload(outputLog, youtubeModel.VideoId);
                if (string.IsNullOrEmpty(tempPath) || !File.Exists(tempPath))
                    throw new Exception("Không tìm thấy file đã tải. Kiểm tra log yt-dlp.");

                _currentTempFilePath = tempPath;

                // 5. Phân tích codec (Video + Audio)
                var mediaInfo = await Xabe.FFmpeg.FFmpeg.GetMediaInfo(tempPath);
                var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                var audioStream = mediaInfo.AudioStreams.FirstOrDefault();

                if (videoStream == null)
                    throw new Exception("File không có video stream.");

                // Check codec
                string videoCodec = videoStream.Codec.ToLower();
                string audioCodec = audioStream?.Codec.ToLower() ?? "";

                bool isH264 = videoCodec.Contains("h264") || videoCodec.Contains("avc1");
                bool isAac = audioCodec.Contains("aac");
                bool hasAudio = audioStream != null;

                Log.Information($"Video Codec: {videoCodec} | Audio Codec: {audioCodec}");

                // ⚠️ Cảnh báo nếu không có audio
                if (!hasAudio)
                {
                    var confirm = System.Windows.MessageBox.Show(
                        "⚠️ Video KHÔNG CÓ TIẾNG!\nCó thể do:\n- Video gốc không có âm thanh\n- Lỗi merge FFmpeg\n\nBạn có muốn giữ lại?",
                        "Thiếu Audio Stream",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (confirm == MessageBoxResult.No)
                        throw new Exception("Đã hủy vì video không có tiếng.");
                }

                // 6. Quyết định xử lý (3 CASES)

                if (isH264 && (isAac || !hasAudio))
                {
                    Log.Information("✅ CASE A: File đã chuẩn (H264 + AAC). Move trực tiếp.");

                    if (File.Exists(finalPath))
                        File.Delete(finalPath);

                    File.Move(tempPath, finalPath);
                }
                else if (isH264 && !isAac && hasAudio)
                {
                    Log.Information($"⚡ CASE B: Video H264 OK, nhưng audio {audioCodec}. Remux audio sang AAC.");

                    var conversion = Xabe.FFmpeg.FFmpeg.Conversions.New();

                    string cmd = $"-y -i \"{tempPath}\" -c:v copy -c:a aac -b:a 192k \"{finalPath}\"";

                    await conversion.Start(cmd);
                    File.Delete(tempPath);
                }
                else
                {
                    Log.Information($"🔄 CASE C: Video {videoCodec} không phổ thông. Convert H264 + AAC.");

                    var conversion = Xabe.FFmpeg.FFmpeg.Conversions.New();

                    // Encode cả video và audio
                    string audioParams = hasAudio ? "-c:a aac -b:a 192k" : "-an";
                    string cmd = $"-y -i \"{tempPath}\" -c:v h264 -preset veryfast {audioParams} \"{finalPath}\"";

                    await conversion.Start(cmd);
                    File.Delete(tempPath);
                }

                // 7. Hoàn tất
                _currentTempFilePath = null;

                var res = System.Windows.MessageBox.Show(
                    $"✅ Tải thành công!\n📁 {Path.GetFileName(finalPath)}\n\nBạn có muốn mở file ngay?",
                    "Hoàn tất",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information);

                if (res == MessageBoxResult.Yes)
                {
                    Process.Start("explorer.exe", $"/select, \"{finalPath}\"");
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi tải video");
                System.Windows.MessageBox.Show(
                    $"❌ Lỗi: {ex.Message}\n\nKiểm tra log để biết chi tiết.",
                    "Error",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);

                if (!string.IsNullOrEmpty(_currentTempFilePath) && File.Exists(_currentTempFilePath))
                {

                    File.Delete(_currentTempFilePath);

                }
            }
            finally
            {
                // Đảm bảo cleanup
                if (!string.IsNullOrEmpty(_currentTempFilePath) && File.Exists(_currentTempFilePath))
                {
                    try { File.Delete(_currentTempFilePath); }
                    catch { }
                }

                IsLoading = false;
                _currentDownloadProcess = null;
            }
        }
        private bool CanCheck()
        {
            return !string.IsNullOrWhiteSpace(InputUrl) && !IsLoading;
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
                YoutubeDetailDTO detail = null;
                // 2. Gọi repo lấy detail
                detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(url);
                if (detail == null)
                {
                    System.Windows.MessageBox.Show("Không lấy được thông tin video.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                youtubeModel.VideoId = detail.Id;
                youtubeModel.Duration = TimeSpan.FromSeconds(detail.Duration);
                youtubeModel.Title = $"{detail.Title} [{youtubeModel.Duration:mm\\:ss}]";
                try
                {
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
                }
                catch (Exception ex)
                {
                    Log.Warning("Không tải được thumbnail: " + ex.Message);
                }

                // 4. Build subtitles
                var original = detail.OriginSubs;
                var autoSubtitle = detail.AutoSubs;


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

                if (youtubeModel.Subtitles.Count == 0 && autoSubtitle != null && autoSubtitle.Length > 0)
                {
                    var unique = autoSubtitle
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
        public async void OnCheck(object obj)
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

