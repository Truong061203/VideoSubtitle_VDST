using Microsoft.Web.WebView2.Core;
using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VDST.Utils;
using VDST.View;
using VDST.View.Windows;
using Xceed.Wpf.Toolkit;

namespace VDST.ViewModel
{
    public class ProjectConfigViewModel : ViewModelBase, IViewModel
    {
        public ProjectInfoModel projectInfo { get; set; }
        public ObservableCollection<LanguageOption> SourceLanguages { get; set; }
        public ObservableCollection<LanguageOption> TargetLanguages { get; set; }
        public ICommand ChooseCommand { get; set; }
        public ICommand OkCommand { get; set; }

        private string titleNewProject;
        public string TitleNewProject
        {
            get => titleNewProject;
            set
            {
                titleNewProject = value;
                Log.Information("Title New Project :" + titleNewProject);
                RaisePropertyChanged(nameof(TitleNewProject));
            }
        }

        public ProjectConfigViewModel()
        {
            Init_Data();
        }
        public void Init_Data()
        {
            ChooseCommand = new VfxCommand(OnChoose, () => true);
            OkCommand = new VfxCommand(OnOk, CanOk);
            projectInfo = new ProjectInfoModel();
            SourceLanguages = new ObservableCollection<LanguageOption>
            {

                new LanguageOption { Code = "vi", NameLanguage = "Tiếng Việt (Vietnamese)" , IsAuto =true },
                new LanguageOption { Code = "en", NameLanguage = "Tiếng Anh (English)" },
                new LanguageOption { Code = "ja", NameLanguage = "Tiếng Nhật (Japanese)" },
                new LanguageOption { Code = "ko", NameLanguage = "Tiếng Hàn (Korean)" },
            };

            TargetLanguages = new ObservableCollection<LanguageOption>
            {
                new LanguageOption { Code = "vi", NameLanguage = "Tiếng Việt (Vietnamese)" },
                new LanguageOption { Code = "en", NameLanguage = "Tiếng Anh (English)" , IsAuto = true },
                new LanguageOption { Code = "ja", NameLanguage = "Tiếng Nhật (Japanese)" },
                new LanguageOption { Code = "ko", NameLanguage = "Tiếng Hàn (Korean)" },
            };

            projectInfo.SourceLanguage = SourceLanguages[0];
            projectInfo.TargetLanguage = TargetLanguages[1];

        }

        public void OnProjectPath(string pathVDST)
        {
            var uploadVM = new UpLoadVideoViewModel();

            // 2. NẠP DỮ LIỆU VÀO (Truyền cái path vừa tạo sang)
            uploadVM.LoadProjectData(pathVDST);

            // 3. Gán vào View và Hiển thị
            var uploadWindow = new UploadVideoWindow();
            uploadWindow.DataContext = uploadVM; // Gán cái VM "đã có dữ liệu" vào
            uploadWindow.Show(); 
        }




        private void OnOk(object obj)
        {
            try
            {
                var result = System.Windows.MessageBox.Show(
                    "Bạn có muốn tạo dự án với: \n Tên : " + projectInfo.ProjectName
                    + $"\n Ngôn Ngữ Gốc : {projectInfo.SourceLanguage}"
                    + $"\n Ngôn Dịch : {projectInfo.TargetLanguage}",
                    "Xác nhận",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes)
                    return;

                // 1. Kiểm tra ngôn ngữ
                if (!projectInfo.SourceLanguage.IsAuto &&
                    projectInfo.TargetLanguage.Code == projectInfo.SourceLanguage.Code)
                {
                    System.Windows.MessageBox.Show(
                        "Ngôn ngữ đích phải khác ngôn ngữ gốc.\nVí dụ: Source = English → Target = Vietnamese.",
                        "Xung đột ngôn ngữ",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 2. Validate tên dự án
                if (!ProjectInfoModel.IsProjectNameValid(projectInfo.ProjectName))
                {
                    System.Windows.MessageBox.Show(
                        "Tên dự án không hợp lệ (Không dấu, không ký tự đặc biệt)!",
                        "Lỗi",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }

                // 3. Tạo thư mục dự án
                string fullPath = projectInfo.GetFullPath();

                if (Directory.Exists(fullPath))
                {
                    System.Windows.MessageBox.Show(
                        "Thư mục dự án này đã tồn tại.\n" +
                        "Hãy đặt tên khác hoặc sử dụng chức năng 'Mở dự án' nếu muốn dùng lại.",
                        "Thư mục đã tồn tại",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }



                // 4. Tạo folder + file .vdst
                string projectFilePath = ProjectRepositories.Instance.CreateProjectFolder(projectInfo);
                Log.Information("Đã tạo dự án mới tại: {ProjectFilePath}", projectFilePath);

                TitleNewProject = projectFilePath; 

                // 5. Đóng window với DialogResult = true
              
                if (obj is Window window)
                {
                    OnProjectPath(projectFilePath);
                    window.Close();
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(
                    $"Lỗi khi tạo dự án: {ex.Message}",
                    "Lỗi",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private bool CanOk()
        {
            if (string.IsNullOrWhiteSpace(projectInfo.ProjectName) ||
                string.IsNullOrWhiteSpace(projectInfo.SaveLocation))
                return false;
            return true;
        }

        private void OnChoose(object obj)
        {
            using (var dialog = new System.Windows.Forms.FolderBrowserDialog())
            {
                dialog.SelectedPath = projectInfo.SaveLocation;
                if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                {
                    projectInfo.SaveLocation = dialog.SelectedPath;
                    (OkCommand as VfxCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
