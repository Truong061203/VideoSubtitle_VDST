using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                _projectName = value; 
                Log.Information($"ProjectName set to: {ProjectName}");
                RaisePropertyChanged(nameof(ProjectName)); 
                FolderName = value?.Replace(" ", "") ?? "";
            }
        }

        // Tên thư mục sẽ tạo (Read-only hoặc auto-gen)
        private string _folderName;
        public string FolderName
        {
            get => _folderName;
            set
            {
                _folderName = value;
                Log.Information($"FolderName set to: {FolderName}", value);
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
                Log.Information($"SaveLocation set to: {SaveLocation}", value);
                RaisePropertyChanged(nameof(SaveLocation));
            }
        }

        private string _sourceLang;
        public string SourceLang
        {
            get => _sourceLang;
            set
            {
                _sourceLang = value; 
                Log.Information($"SourceLang set to: {SourceLang}", value);
                RaisePropertyChanged(nameof(SourceLang));
            }
        }

        private string _targetLang;
        public string TargetLang
        {
            get => _targetLang;
            set
            {
                _targetLang = value;
                Log.Information($"TargetLang set to: {TargetLang}", value);
                RaisePropertyChanged(nameof(TargetLang));
            }
        }
    }
}

