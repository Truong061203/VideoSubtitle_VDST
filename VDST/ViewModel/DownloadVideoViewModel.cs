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
        private readonly string _url;
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

        private readonly HttpClient _httpClient = new HttpClient();
        private readonly YoutubeModel Youtube = new YoutubeModel();


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
            //DownloadVideoCommand = new VfxCommand(OnDownload, () => !IsBusy && !string.IsNullOrEmpty(TitleVideo));
            _ = LoadInfoAsync();
        }



        private async Task LoadInfoAsync()
        {
            try
            {
                IsBusy = true;

                var id = YoutubeRepository.Instance.RegexYoutubeId(_url);
                if (string.IsNullOrEmpty(id))
                {
                    MessageBox.Show("Link youtube không hợp lệ.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                var exactLink = $"https://www.youtube.com/watch?v={id}";
                var detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(exactLink);
                if (detail == null)
                {
                    MessageBox.Show("Không lấy được thông tin video.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                Youtube.VideoId= detail.Id;
                Youtube.Duration = TimeSpan.FromSeconds(detail.Duration);
                Youtube.Title = $"{detail.Title} [{Youtube.Duration:mm\\:ss}]";

                if (!string.IsNullOrWhiteSpace(detail.Thumbnail))
                {
                    var bytes = await _httpClient.GetByteArrayAsync(detail.Thumbnail);
                    using var ms = new MemoryStream(bytes);
                    var bmp = new BitmapImage();
                    bmp.BeginInit();
                    bmp.StreamSource = ms;
                    bmp.CacheOption = BitmapCacheOption.OnLoad;
                    bmp.EndInit();
                    bmp.Freeze();
                    Youtube.Thumbnail = bmp;
                }

                Youtube.Subtitles.Clear();
                Youtube.SubFilters.Clear();

                var original = detail.OriginSubs;
                var auto = detail.AutoSubs;

                if (original != null && original.Length > 0)
                {
                    var unique = original
                        .GroupBy(x => x.LanguageCode)
                        .Select(g => g.FirstOrDefault(s => s.Ext == "srt") ?? g.First())
                        .ToList();

                    foreach (var item in unique.Where(x => x != null))
                    {
                        Youtube.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-subs"
                        });
                    }
                }

                if (Youtube.Subtitles.Count == 0 && auto != null && auto.Length > 0)
                {
                    var unique = auto
                        .GroupBy(x => x.LanguageCode)
                        .Select(g => g.FirstOrDefault(s => s.Ext == "srt") ?? g.First())
                        .ToList();

                    foreach (var item in unique.Where(x => x != null))
                    {
                        Youtube.Subtitles.Add(new SubtitleFormatModel
                        {
                            Name = item.Name,
                            Language = item.LanguageCode,
                            Ext = item.Ext,
                            Url = item.Url,
                            Source = "--write-auto-subs"
                        });
                    }
                }

                foreach (var s in Youtube.Subtitles)
                    Youtube.SubFilters.Add(s);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "LoadVideoInfoAsync error");
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                RaisePropertyChanged(nameof(Youtube));
            }
        }
        private async void OnCheck(object obj)
        {
          
        }

        //private async void OnDownload(object obj)
        //{
        //    IsBusy = true;
        //    TitleVideo = "Đang tải video về máy...";

        //    try
        //    {
        //        // 2. Gọi Repository tải video thật
        //        string path = await YoutubeRepository.Instance.DownloadVideoAsync(Url);

        //        if (!string.IsNullOrEmpty(path))
        //        {
        //            DownloadedVideoPath = path;
        //            MessageBox.Show("Tải thành công!");

        //            // Đóng cửa sổ và trả về OK
        //            if (obj is Window window)
        //            {
        //                window.DialogResult = true;
        //                window.Close();
        //            }
        //        }
        //        else
        //        {
        //            MessageBox.Show("Tải thất bại. Vui lòng thử lại.");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        MessageBox.Show("Lỗi tải: " + ex.Message);
        //    }
        //    finally
        //    {
        //        IsBusy = false;
        //        TitleVideo = "Đã hoàn tất.";
        //    }
        //}
    }
}

