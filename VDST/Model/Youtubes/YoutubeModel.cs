using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using VDST.ViewModel;

namespace VDST.Model.Youtubes
{
    public class YoutubeModel : ViewModelBase
    {
        public event EventHandler ThumbnailPathChanged;

        private string _url = string.Empty;
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    RaisePropertyChanged(nameof(Url));
                }
                // Bật nút download khi có thumbnail + url
                IsEnableDownload = Thumbnail != null && string.IsNullOrEmpty(Url) == false;
            }
        }

        private string _title = string.Empty;
        public string Title
        {
            get => _title;
            set
            {
                if (_title != value)
                {
                    _title = value;
                    RaisePropertyChanged(nameof(Title));
                }
            }
        }

        private TimeSpan _duration = TimeSpan.Zero;
        public TimeSpan Duration
        {
            get => _duration;
            set
            {
                if (_duration != value)
                {
                    _duration = value;
                    RaisePropertyChanged(nameof(Duration));
                }
            }
        }

        private string _videoPath = string.Empty;
        public string VideoPath
        {
            get => _videoPath;
            set
            {
                if (_videoPath != value)
                {
                    _videoPath = value;
                    RaisePropertyChanged(nameof(VideoPath));
                }
            }
        }

        private string _subtitlePath = string.Empty;
        public string SubtitlePath
        {
            get => _subtitlePath;
            set
            {
                if (_subtitlePath != value)
                {
                    _subtitlePath = value;
                    RaisePropertyChanged(nameof(SubtitlePath));
                }
            }
        }

        private string _thumbnailPath = string.Empty;
        public string ThumbnailPath
        {
            get => _thumbnailPath;
            set
            {
                if (_thumbnailPath != value)
                {
                    _thumbnailPath = value;
                    RaisePropertyChanged(nameof(ThumbnailPath));
                }
                ThumbnailPathChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        private ImageSource _thumbnail = null;
        public ImageSource Thumbnail
        {
            get => _thumbnail;
            set
            {
                if (_thumbnail != value)
                {
                    _thumbnail = value;
                    RaisePropertyChanged(nameof(Thumbnail));
                }
                // Bật/tắt nút Download theo Url + Thumbnail
                IsEnableDownload = Thumbnail != null && string.IsNullOrEmpty(Url) == false;
            }
        }

        private bool _isEnableDownload = false;
        public bool IsEnableDownload
        {
            get => _isEnableDownload;
            set
            {
                if (_isEnableDownload != value)
                {
                    _isEnableDownload = value;
                    RaisePropertyChanged(nameof(IsEnableDownload));
                }
            }
        }

        public ObservableCollection<SubtitleFormatModel> Subtitles { get; set; } = new ObservableCollection<SubtitleFormatModel>();

        public ObservableCollection<SubtitleFormatModel> SubFilters { get; set; } = new ObservableCollection<SubtitleFormatModel>();

        public string VideoId { get; set; }

        public YoutubeModel()
        {
        }
    }
    public class SubtitleFormatModel
    {
        public bool IsChecked { get; set; }
        public string Name { get; set; }        // "English"
        public string VietnameseName { get; set; } = "";  // nếu có map tên tiếng Việt
        public string Language { get; set; }        // "en", "vi"
        public string Ext { get; set; }        // "srt", "vtt"
        public string Url { get; set; }        // link phụ đề
        public string Source { get; set; }        // "--write-subs" / "--write-auto-subs"
    }
}
