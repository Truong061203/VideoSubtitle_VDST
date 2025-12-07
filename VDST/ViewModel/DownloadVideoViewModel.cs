using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using VDST.Commands;
using VDST.Model.Youtubes;
using VDST.Repositories;

namespace VDST.ViewModel
{
    public class DownloadVideoViewModel : ViewModelBase
    {
        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                RaisePropertyChanged(nameof(Url));
            }
        }

        // Dữ liệu hiển thị sau khi check
        private string _titleVideo;
        public string TitleVideo
        {
            get => _titleVideo;
            set { _titleVideo = value;
                RaisePropertyChanged(nameof(TitleVideo)); }
        }

        private string _thumbnailUrl;
        public string ThumbnailUrl
        {
            get => _thumbnailUrl;
            set { _thumbnailUrl = value;
                RaisePropertyChanged(nameof(ThumbnailUrl)); }
        }

        private BitmapImage _thumbnail;
        public BitmapImage Thumbnail
        {
            get => _thumbnail;
            set { _thumbnail = value; RaisePropertyChanged(nameof(Thumbnail)); }
        }

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
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



        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        public string DownloadedVideoPath { get; private set; }

        public ICommand CheckCommand { get; set; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand DownloadSubtitleFileCommand { get; }
        public ICommand ExportSubtitleTextCommand { get; }

        public DownloadVideoViewModel(string url)
        {
            CheckCommand = new VfxCommand(OnCheck, () => !IsBusy);

            // Chỉ cho tải khi đã Check xong (có Title)
            DownloadVideoCommand = new VfxCommand(OnDownload, () => !IsBusy && !string.IsNullOrEmpty(TitleVideo));
        }



        private async Task LoadInfoAsync()
        {
            try
            {
                IsBusy = true;
                Log.Information("Loading youtube info for {Url}", Url);

                var detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(Url);
                if (detail == null)
                {
                    MessageBox.Show("Không lấy được thông tin video YouTube.",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                TitleVideo = detail.Title;
                Duration = TimeSpan.FromSeconds(detail.Duration);

                // Thumbnail
                if (!string.IsNullOrWhiteSpace(detail.Thumbnail))
                {
                    try
                    {
                        var bytes = await _httpClient.GetByteArrayAsync(detail.Thumbnail);
                        using var ms = new MemoryStream(bytes);
                        var bmp = new BitmapImage();
                        bmp.BeginInit();
                        bmp.StreamSource = ms;
                        bmp.CacheOption = BitmapCacheOption.OnLoad;
                        bmp.EndInit();
                        bmp.Freeze();
                        Thumbnail = bmp;
                    }
                    catch (Exception ex)
                    {
                        Log.Warning(ex, "Load thumbnail fail");
                    }
                }

                // Subtitles
                Subtitles.Clear();
                var subs = (detail.OriginSubs != null && detail.OriginSubs.Length > 0)
                    ? detail.OriginSubs
                    : (detail.AutoSubs ?? Array.Empty<SubtitleDTO>());

                foreach (var s in subs)
                    Subtitles.Add(s);

                if (Subtitles.Count == 0)
                {
                    MessageBox.Show("Video không có subtitle (gốc hoặc auto).",
                                    "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error while loading youtube info");
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void OnCheck(object obj)
        {
            IsBusy = true;
            TitleVideo = "Đang kiểm tra và tải công cụ hỗ trợ (lần đầu)...";
            ThumbnailUrl = null;

            try
            {
                // 1. Gọi Repository lấy thông tin 
                // (Repository sẽ tự động tải tool nếu chưa có)
                var info = await YoutubeRepository.Instance.GetYoutubeDetailAsync(Url);

                if (info != null)
                {
                    TitleVideo = info.Title;
                    ThumbnailUrl = info.Thumbnail;
                }
                else
                {
                    TitleVideo = "Không tìm thấy video. Link sai hoặc video riêng tư.";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
                // Cập nhật lại nút Download (Sáng lên nếu check thành công)
                (DownloadCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        private async void OnDownload(object obj)
        {
            IsBusy = true;
            TitleVideo = "Đang tải video về máy...";

            try
            {
                // 2. Gọi Repository tải video thật
                string path = await YoutubeRepository.Instance.DownloadVideoAsync(Url);

                if (!string.IsNullOrEmpty(path))
                {
                    DownloadedVideoPath = path;
                    MessageBox.Show("Tải thành công!");

                    // Đóng cửa sổ và trả về OK
                    if (obj is Window window)
                    {
                        window.DialogResult = true;
                        window.Close();
                    }
                }
                else
                {
                    MessageBox.Show("Tải thất bại. Vui lòng thử lại.");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi tải: " + ex.Message);
            }
            finally
            {
                IsBusy = false;
                TitleVideo = "Đã hoàn tất.";
            }
        }
    }
}

