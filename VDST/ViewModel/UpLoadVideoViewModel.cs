using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;
using VDST.Commands;
using VDST.Model;
using VDST.Model.Interfaces;
using VDST.Repositories;
using VDST.View.Windows;

namespace VDST.ViewModel
{
    public class UpLoadVideoViewModel : ViewModelBase
    {
        private ProjectInfoModel _currentProject;

        public ProjectInfoModel CurrentProject
        {
            get => _currentProject;
            set
            {
                _currentProject = value;
                RaisePropertyChanged(nameof(CurrentProject));
            }
        }

        private string titleApp;
        public string TitleApp
        {
            get => titleApp;
            set
            {
                titleApp = value;
                RaisePropertyChanged(nameof(TitleApp));
            }
        }


        private string _youtubeUrl;
        public string YoutubeUrl
        {
            get => _youtubeUrl;
            set
            {
                _youtubeUrl = value;
                Log.Information("YTB URL :"+ YoutubeUrl);

                RaisePropertyChanged(nameof(YoutubeUrl));
                (CheckUrlCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _projectFilePath;
        public string ProjectFilePath
        {
            get => _projectFilePath;
            private set
            {
                _projectFilePath = value;
                RaisePropertyChanged(nameof(ProjectFilePath));
            }
        }

        public void LoadProjectData(string projectFilePath)
        {
            if (File.Exists(projectFilePath))
            {
                var project = ProjectRepositories.Instance.LoadProject(projectFilePath);
                Log.Information("Loaded project from {ProjectFilePath}", projectFilePath);

                if (project != null)
                {
                    CurrentProject = project;
                    TitleApp = "VDST " + project.ProjectName;
                    ProjectFilePath = projectFilePath; // GIỮ LUÔN ĐỂ SAU NÀY SAVE
                }
            }
        }

        public ICommand CheckUrlCommand { get; set; }
        public ICommand UploadCommand { get; set; }
        public UpLoadVideoViewModel()
        {
            CheckUrlCommand = new VfxCommand(OnCheck, CanCheck);
            UploadCommand = new VfxCommand(OnUpload, () => true);

        }

        private void OnUpload(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Video File to Upload";
            openFileDialog.Filter = "Video Files|*.mp4;*.mov;*.avi;*.mkv;*.flv;*.wmv|All Files|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.OK)
            {
                string selectedFilePath = openFileDialog.FileName;
                Log.Information("Selected file for upload: {SelectedFilePath}", selectedFilePath);
            }
        }

        private bool CanCheck()
        {
            if (string.IsNullOrEmpty(YoutubeUrl))
                return false;
            return true;
        }

        private void OnCheck(object obj)
        {
            var id = YoutubeRepository.Instance.RegexYoutubeId(YoutubeUrl);
            if (string.IsNullOrEmpty(id))
            {
                System.Windows.MessageBox.Show("Link youtube không đúng format.\r\nVí dụ:\r\n" +
                                "https://www.youtube.com/watch?v=[id]\r\n" +
                                "https://www.youtube.com/shorts/[id]",
                                "Link youtube", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var exactLink = $"https://www.youtube.com/watch?v={id}";

            var ytbVM = new DownloadVideoViewModel(exactLink);
            ytbVM.Url = YoutubeUrl;
            var ytbWindow = new DownloadVideoWindow();
            ytbWindow.DataContext = ytbVM;
            if (ytbWindow.ShowDialog() == true)
            {
                Log.Information("YouTube URL checked successfully: {YoutubeUrl}", YoutubeUrl);
            }
        }
    }

}
