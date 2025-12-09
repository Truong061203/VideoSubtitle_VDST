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
    public class YoutubeViewModel : ViewModelBase
    {
        private YoutubeModel _youtubeModel = new YoutubeModel();
        public YoutubeModel YoutubeVModel
        {
            get => _youtubeModel;
            set
            {
                _youtubeModel = value;
                RaisePropertyChanged(nameof(YoutubeModel));
            }
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


        private readonly HttpClient _httpClient = new HttpClient();


        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                _url = value;
                Log.Information("YTB URL in ViewModel :" + Url);
                RaisePropertyChanged(nameof(Url));
            }
        }

        // BIẾN KẾT QUẢ (Để trả về cho màn hình cha)
        public string DownloadedVideoPath { get; private set; }

        public ICommand CheckCommand { get; set; }
        public ICommand DownloadVideoCommand { get; }
        public ICommand DownloadSubtitleFileCommand { get; }
        public ICommand ExportSubtitleTextCommand { get; }

        public YoutubeViewModel(string _url)
        {
            CheckCommand = new VfxCommand(OnCheck, () => !IsBusy);
            Url = _url;
            // Chỉ cho tải khi đã Check xong (có Title)
            //DownloadVideoCommand = new VfxCommand(OnDownload, () => !IsBusy && !string.IsNullOrEmpty(TitleVideo));
            LoadInfoAsync(this._url);
        }



        private async Task LoadInfoAsync(string url)
        {
            try
            {
                YoutubeVModel.Url = url;
                IsBusy = true;

                var id = YoutubeRepository.Instance.RegexYoutubeId(url);
                if (string.IsNullOrEmpty(id))
                {
                    System.Windows.MessageBox.Show("Link youtube không đúng format.\r\nVí dụ:\r\n" +
                                    "https://www.youtube.com/watch?v=[id]\r\n" +
                                    "https://www.youtube.com/shorts/[id]",
                                    "Link youtube", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var urlLink = $"https://www.youtube.com/watch?v={id}";

                var detail = await YoutubeRepository.Instance.GetYoutubeDetailAsync(urlLink);
                if (detail == null)
                {
                    MessageBox.Show("Không lấy được thông tin video.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                YoutubeVModel.VideoId = detail.Id;
                YoutubeVModel.Duration = TimeSpan.FromSeconds(detail.Duration);
                YoutubeVModel.Title = $"{detail.Title} [{YoutubeVModel.Duration:mm\\:ss}]";

                // download thumbnail
                using (var client = new System.Net.Http.HttpClient() { Timeout = TimeSpan.FromSeconds(90) })
                {
                    byte[] imageData = await client.GetByteArrayAsync(detail.Thumbnail);
                    BitmapImage bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.StreamSource = new MemoryStream(imageData);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad; // Important for memory management
                    bitmap.EndInit();
                    bitmap.Freeze();

                    YoutubeVModel.Thumbnail = bitmap;
                }

                // get subtitle
                YoutubeVModel.Subtitles.Clear();
                YoutubeVModel.SubFilters.Clear();

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
                MessageBox.Show(ex.Message, "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
            }
        }
        private async void OnCheck(object obj)
        {
            Url = YoutubeVModel.Url;
            if (string.IsNullOrEmpty(Url))
            {
                MessageBox.Show("Vui lòng điền link video youtube", "Link", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            await LoadInfoAsync(Url);

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

