using AutoUpdaterDotNET;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VDST.ViewModel;

namespace VDST.Model
{
    public class ProjectInfo : ViewModelBase
    {
        private string _projectName;
        public string ProjectName
        {
            get => _projectName;
            set
            {
                if (_projectName != value)
                {
                    _projectName = value;
                    Log.Information($"Project name changed to: {ProjectName}");
                    RaisePropertyChanged(nameof(ProjectName));

                    if (IsProjectNameValid(value))
                    {
                        FolderName = value.Replace(" ", "");
                        Log.Information($"Folder name auto-generated as: {FolderName}");
                    }
                    else
                    {
                        FolderName = string.Empty;
                        Log.Warning("Invalid project name entered; Folder name cleared.");
                    }
                }
            }
        }
        public static bool IsProjectNameValid(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
                return false;
            var regex = new Regex("^[A-Za-z0-9 _-]+$");
           Log.Information($"Regex: {regex}");

            bool isValid = regex.IsMatch(name);
            Log.Information($"Is project name '{name}' valid? {isValid}");
            return isValid;
        }


        /// <summary>
        ///  chứa tên thư mục được tạo tự động từ ProjectName, không có khoảng trắng.
        /// </summary>
        private string _folderName;
        public string FolderName
        {
            get => _folderName;
            set
            {
                _folderName = value;
                Log.Information($"Folder name set to: {FolderName}");
                RaisePropertyChanged(nameof(FolderName));
            }
        }

        private string _saveLocation;
        public string SaveLocation
        {
            get => _saveLocation;
            set
            {
                _saveLocation = value;
                Log.Information($"Save location set to: {SaveLocation}");
                RaisePropertyChanged(nameof(SaveLocation));
            }
        }


        private LanguageOption _sourceLanguage;
        public LanguageOption SourceLanguage
        {
            get => _sourceLanguage;
            set
            {
                _sourceLanguage = value;
                Log.Information($"Source language set to: {SourceLanguage.NameLanguage}");
                RaisePropertyChanged(nameof(SourceLanguage));
            }
        }

        private LanguageOption _targetLanguage;
        public LanguageOption TargetLanguage
        {
            get => _targetLanguage;
            set
            {
                _targetLanguage = value;
                Log.Information($"Target language set to: {TargetLanguage.NameLanguage}");
                RaisePropertyChanged(nameof(TargetLanguage));
            }
        }


        public string GetFullPath()
        {
            if (string.IsNullOrWhiteSpace(SaveLocation) ||
                string.IsNullOrWhiteSpace(FolderName))
            {
                Log.Warning("Cannot get full path: SaveLocation or FolderName is empty.");
                return string.Empty;
            }

            string fullPath = Path.Combine(SaveLocation, FolderName);
            Log.Information($"Full project path: {fullPath}");
            return fullPath;
        }
    }
}


public class LanguageOption
{
    /// <summary>Mã code dùng cho API, ví dụ: "en", "vi", "auto".</summary>
    public string Code { get; set; }

    /// <summary>Tên hiển thị trên UI, ví dụ: "Tiếng Anh (English)".</summary>
    public string NameLanguage { get; set; }

    /// <summary>Có phải là lựa chọn "Auto Detect" hay không.</summary>
    public bool IsAuto { get; set; }
    public override string ToString()
    {
        return NameLanguage; // ComboBox sẽ tự dùng cái này để hiển thị
    }
}
