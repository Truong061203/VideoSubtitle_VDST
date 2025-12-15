using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDST.ViewModel;

namespace VDST.Model.Youtubes
{
    public class SubtitleFormatModel :ViewModelBase
    {
        private bool _isChecked;
        public bool IsChecked
        {
            get => _isChecked;
            set
            {
                if (_isChecked != value)
                {
                    _isChecked = value;
                    RaisePropertyChanged(nameof(IsChecked));
                }
            }
        }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    Log.Information("SubtitleFormatModel Name changed to: {Name}", _name);
                    RaisePropertyChanged(nameof(Name));
                }
            }
        }

        private string _vietnameseName;
        public string VietnameseName
        {
            get => _vietnameseName;
            set
            {
                if (_vietnameseName != value)
                {
                    _vietnameseName = value;
                    Log.Information("SubtitleFormatModel VietnameseName changed to: {VietnameseName}", _vietnameseName);
                    RaisePropertyChanged(nameof(VietnameseName));
                }
            }
        }

        private string _language;
        public string Language
        {
            get => _language;
            set
            {
                if (_language != value)
                {
                    _language = value;
                    Log.Information("SubtitleFormatModel Language changed to: {Language}", _language);
                    RaisePropertyChanged(nameof(Language));
                }
            }
        }

        // đuổi file của phụ đề
        private string _ext;
        public string Ext
        {
            get => _ext;
            set
            {
                if (_ext != value)
                {
                    _ext = value;
                    Log.Information("SubtitleFormatModel Ext changed to: {Ext}", _ext);
                    RaisePropertyChanged(nameof(Ext));
                }
            }
        }


        private string _url;
        public string Url
        {
            get => _url;
            set
            {
                if (_url != value)
                {
                    _url = value;
                    Log.Information("SubtitleFormatModel Url changed to: {Url}", _url);
                    RaisePropertyChanged(nameof(Url));
                }
            }
        }

        // sub gốc hoặc sub auto
        private string _source;
        public string Source
        {
            get => _source;
            set
            {
                if (_source != value)
                {
                    _source = value;
                    Log.Information("SubtitleFormatModel Source changed to: {Source}", _source);
                    RaisePropertyChanged(nameof(Source));
                }
            }
        }
    }
}
