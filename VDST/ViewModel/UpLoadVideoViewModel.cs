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
using VDST.Model.Youtubes;
using VDST.Repositories;
using VDST.View.Windows;

namespace VDST.ViewModel
{
    public class UpLoadVideoViewModel : ViewModelBase, IViewModel
    {
        #region
        private ProjectInfoModel _currentProject;

        public ProjectInfoModel CurrentProject
        {
            get => _currentProject;
            set
            {
                _currentProject = value;
                Log.Information("Current project set to: {CurrentProject}", CurrentProject?.ProjectName);
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
                Log.Information("Title App set to: {TitleApp}", TitleApp);
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
                Log.Information("YTB URL :" + YoutubeUrl);
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
                Log.Information("Project file path set to: {ProjectFilePath}", ProjectFilePath);
                RaisePropertyChanged(nameof(ProjectFilePath));
            }
        }

        private string _selectedVideoPath;
        public string SelectedVideoPath
        {
            get => _selectedVideoPath;
            set
            {
                _selectedVideoPath = value;
                Log.Information("Selected video path set to: {SelectedVideoPath}", SelectedVideoPath);
                RaisePropertyChanged(nameof(SelectedVideoPath));
            }
        }

        #endregion



        public ICommand CheckUrlCommand { get; set; }
        public ICommand UploadCommand { get; set; }
        public UpLoadVideoViewModel(string projectPath)
        {
            ProjectFilePath = projectPath;
            LoadProjectData();

        }
        public UpLoadVideoViewModel()
        {
            Init_Data();
        }


        public void Init_Data()
        {
            CheckUrlCommand = new VfxCommand(OnCheck, CanCheck);
            UploadCommand = new VfxCommand(OnUpload, () => true);
            TitleApp = "VDST - Upload Video";
        }
        /// <summary>
        /// Load project data from file path
        /// </summary>
        private void LoadProjectData()
        {
            if (string.IsNullOrWhiteSpace(ProjectFilePath))
            {
                System.Windows.MessageBox.Show("Đường dẫn file dự án đang trống.", "Lỗi dự án", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            if (!File.Exists(ProjectFilePath))
            {
                System.Windows.MessageBox.Show("Không tìm thấy file dự án:\n" + ProjectFilePath,
                                "Lỗi dự án", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            try
            {
                var project = ProjectRepositories.Instance.LoadProject(ProjectFilePath);
                Log.Information("Loaded project from {ProjectFilePath}", ProjectFilePath);

                if (project == null)
                {
                    System.Windows.MessageBox.Show("Không đọc được dữ liệu dự án từ file:\n" + ProjectFilePath,
                                    "Lỗi dự án", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                CurrentProject = project;
                TitleApp = "VDST " + project.ProjectName;
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Lỗi khi đọc file dự án:\n" + ex.Message,
                                "Lỗi dự án", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }




        //private async Task<ProjectInfoModel> ImportLocalVideo(ProjectInfoModel projectInfo, string sourceVideoPath)
        //{
        //    string workDir = projectInfo.GetFullPath();
        //    string inputDir = Path.Combine(workDir, "Input");
        //    string tempDir = Path.Combine(workDir, "Temp");
        //    string fileName = Path.GetFileName(sourceVideoPath);
        //    string destVideoPath = Path.Combine(inputDir, fileName);
        //}

        /// <summary>
        /// Upload video file mp4
        /// </summary>
        private void OnUpload(object obj)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = "Select Video File to Upload";
            openFileDialog.Filter = "Video Files|*.mp4;*.mov;*.avi;*.mkv;*.flv;*.wmv|All Files|*.*";
            if (openFileDialog.ShowDialog() == DialogResult.No)
            {
                return;
            }
            SelectedVideoPath = openFileDialog.FileName;
            var projectPath = Path.GetFileName(SelectedVideoPath);

        }


        private bool CanCheck()
        {
            if (string.IsNullOrEmpty(YoutubeUrl))
                return false;
            return true;
        }


        /// <summary>
        /// Check link youtube 
        /// </summary>
        private void OnCheck(object obj)
        {
            try
            {

                // 1. Kiểm tra định dạng của đường dẫn và lấy ra id 
                var id = YoutubeRepository.Instance.RegexYoutubeId(YoutubeUrl);
                if (string.IsNullOrEmpty(id))
                {
                    System.Windows.MessageBox.Show("Link youtube không đúng format.\r\nVí dụ:\r\n" +
                                    "https://www.youtube.com/watch?v=[id]\r\n" +
                                    "https://www.youtube.com/shorts/[id]",
                                    "Link youtube", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var urlLink = $"https://www.youtube.com/watch?v={id}";

                // gán nó cho datacontext của downloadVideoVM
                var ytbVM = new YoutubeViewModel(urlLink);

                //gán vm cho view 
                var ytbWindow = new YoutubeWindow();
                ytbWindow.DataContext = ytbVM;
                if (ytbWindow.ShowDialog() == true)
                {
                    Log.Information("YouTube URL checked successfully: {YoutubeUrl}", YoutubeUrl);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error checking YouTube URL: {YoutubeUrl}", YoutubeUrl);
                System.Windows.MessageBox.Show("Đã có lỗi xảy ra khi kiểm tra link youtube.\r\n" +
                                    "Vui lòng thử lại sau.",
                                    "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


    }
}
