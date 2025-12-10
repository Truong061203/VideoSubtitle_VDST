using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace VDST.ViewModel
{
    public class YoutubeViewModel : ViewModelBase
    {
        private YoutubeModel _youtubeModel = new YoutubeModel();
        public YoutubeModel YoutubeVModel
        {
            get => _youtubeModel;
            set
            {
                _youtubeModel = value;
                RaisePropertyChanged(nameof(YoutubeVModel));
            }
        }

        private bool _isBusy = false;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                Log.Information("Isbusy :" + value);
                RaisePropertyChanged(nameof(IsBusy));

                (CheckCommand as VfxCommand)?.RaiseCanExecuteChanged();
                (DownloadVideoCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }
        public ObservableCollection<SubtitleDTO> Subtitles { get; } = new();

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

        private string _downloadStatus;
        public string DownloadStatus
        {
            get => _downloadStatus;
            set
            {
                _downloadStatus = value;
                RaisePropertyChanged(nameof(DownloadStatus));
            }
        }

        private int _downloadPercent;
        public int DownloadPercent
        {
            get => _downloadPercent;
            set
            {
                _downloadPercent = value;
                RaisePropertyChanged(nameof(DownloadPercent));
            }
        }

        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        //public string DownloadedVideoPath { get; private set; }

        private readonly HttpClient _httpClient = new HttpClient();

        public ICommand CheckCommand { get; set; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand DownloadSubtitleFileCommand { get; }
        public ICommand ExportSubtitleTextCommand { get; }

        public YoutubeViewModel(string _url)
        {
            CheckCommand = new VfxCommand(OnCheck, CanCheck);
            InputUrl = _url;

            DownloadVideoCommand = new VfxCommand(OnDownloadVideo, CanDownloadVideo);
            if (_url != null)
            {
                OnCheck(null);
            }
        }

        private bool CanDownloadVideo()
        {
            if (YoutubeVModel.Thumbnail != null && !IsBusy)
                return true;
            return false;
        }

        private async void OnDownloadVideo(object obj)
        {
            var dlg = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Lưu video",
                Filter = "Video files (*.mp4)|*.mp4",
                RestoreDirectory = true,
                FileName = $"{YoutubeVModel.VideoId}.mp4"
            };

            if (dlg.ShowDialog() != true)
                return;



        }
        private bool CanCheck()
        {
            if (InputUrl != null || !IsBusy)
                return true;
            return false;
        }

        private async Task LoadInfoAsync(string url)
        {
            try
            {
                IsBusy = true;

                // 1. Clear UI cũ cho đỡ “ảo”
                YoutubeVModel.Title = "Đang tải thông tin video...";
                YoutubeVModel.Thumbnail = null;
                YoutubeVModel.Subtitles.Clear();
                YoutubeVModel.SubFilters.Clear();

                // (Không bắt buộc nhưng nên đồng bộ):
                YoutubeVModel.Url = url;

                // 2. Gọi repo lấy detail
                var detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(url);
                if (detail == null)
                {
                    System.Windows.MessageBox.Show("Không lấy được thông tin video.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                YoutubeVModel.VideoId = detail.Id;
                YoutubeVModel.Duration = TimeSpan.FromSeconds(detail.Duration);
                YoutubeVModel.Title = $"{detail.Title} [{YoutubeVModel.Duration:mm\\:ss}]";

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

                    YoutubeVModel.Thumbnail = bitmap;
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
                        YoutubeVModel.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-subs"
                        });
                    }
                }

                if (YoutubeVModel.Subtitles.Count == 0 && auto != null && auto.Length > 0)
                {
                    var unique = auto
                        .GroupBy(x => x.LanguageCode)
                        .Select(g => g.FirstOrDefault(s => s.Ext == "srt") ?? g.First())
                        .ToList();

                    foreach (var item in unique.Where(x => x != null))
                    {
                        YoutubeVModel.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-auto-subs"
                        });
                    }
                }

                foreach (var s in YoutubeVModel.Subtitles)
                    YoutubeVModel.SubFilters.Add(s);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadVideoInfoAsync error");
                System.Windows.MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }

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
                YoutubeVModel.Url = exactUrl;

                await LoadInfoAsync(exactUrl);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "OnCheck error with url: {Url}", InputUrl);
                System.Windows.MessageBox.Show("Lỗi Url link", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }


        }


    }
}

