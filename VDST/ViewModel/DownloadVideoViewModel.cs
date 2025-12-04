using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VDST.Commands;
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

        private bool _isBusy;
        public bool IsBusy
        {
            get => _isBusy;
            set
            {
                _isBusy = value;
                RaisePropertyChanged(nameof(IsBusy));
           
                (CheckCommand as VfxCommand)?.RaiseCanExecuteChanged();
                (DownloadCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        public string DownloadedVideoPath { get; private set; }

        public ICommand CheckCommand { get; set; }
        public ICommand DownloadCommand { get; set; }

        public DownloadVideoViewModel()
        {
            CheckCommand = new VfxCommand(OnCheck, () => !IsBusy);

            // Chỉ cho tải khi đã Check xong (có Title)
            DownloadCommand = new VfxCommand(OnDownload, () => !IsBusy && !string.IsNullOrEmpty(TitleVideo));
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

