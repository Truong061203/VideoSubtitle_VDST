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
        public string CreatedProjectPath { get; private set; } // Thêm thuộc tính này
        public ObservableCollection<LanguageOption> SourceLanguages { get; set; }
        public ObservableCollection<LanguageOption> TargetLanguages { get; set; }
        public ICommand ChooseCommand { get; set; }
        public ICommand OkCommand { get; set; }


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


        // Hàm này được gọi khi bấm nút "Tạo Dự Án Mới" ở màn hình chào
        public void OnUploadVideo(string pathVDST)
        {
            var uploadWindow = new UpLoadVideo(); 
            var uploadVM = new UpVideoYouTubeVM();
            uploadVM.LoadProjectData(pathVDST);
            uploadWindow.DataContext = uploadVM;
            uploadWindow.ShowDialog();

        }
        private bool CanOk()
        {
            if (string.IsNullOrWhiteSpace(projectInfo.ProjectName) ||
                string.IsNullOrWhiteSpace(projectInfo.SaveLocation))
                return false;
            return true;
        }

        private void OnOk(object obj)
        {
            try
            {
                var result = System.Windows.MessageBox.Show("Bạn có muốn tạo dự án với: \n Tên : " + projectInfo.ProjectName
                                                            + $"\n Ngôn Ngữ Gốc : {projectInfo.SourceLanguage}"
                                                            + $"\n Ngôn Dịch : {projectInfo.TargetLanguage}"
                                                            , "Xác nhận", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result != MessageBoxResult.Yes)
                {
                    return; // Người dùng chọn No, thoát hàm
                }

                if (projectInfo.SourceLanguage.IsAuto = false && projectInfo.TargetLanguage.Code == projectInfo.SourceLanguage.Code)
                {
                    System.Windows.MessageBox.Show("Ngôn ngữ đích phải khác ngôn ngữ gốc.\nVí dụ: Source = English → Target = Vietnamese.",
                                                    "Xung đột ngôn ngữ",
                                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ProjectInfoModel.IsProjectNameValid(projectInfo.ProjectName))
                {
                    System.Windows.MessageBox.Show("Tên dự án không hợp lệ (Không dấu, không ký tự đặc biệt)!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fullPath = projectInfo.GetFullPath();
                if (Directory.Exists(fullPath))
                {
                    System.Windows.MessageBox.Show("Thư mục dự án đã tồn tại!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string projectFilePath = Path.Combine(fullPath, projectInfo.FolderName + ".VDST");
                Log.Information("Tạo dự án mới tại: {ProjectFilePath}", projectFilePath);

                ProjectRepositories.Instance.CreateProjectFolder(projectInfo);

                CreatedProjectPath = projectFilePath; // <-- Lưu vào biến này để trả về

                // 4. CẬP NHẬT DANH SÁCH GẦN ĐÂY (RECENT)
                // (Dùng Repository của Senior mà bạn đã có)
                //RecentProjectRepository.OnAddRecentPath(RecentProjectModel.Instance, projectFilePath);
                //RecentProjectRepository.OnSave(RecentProjectModel.Instance, GlobalVariable.Instance.PATH_SAVE_RECENT_PROJECT);

                // 5. THÔNG BÁO VÀ ĐÓNG CỬA SỔ

                if (obj is Window window)
                {
                    OnUploadVideo(projectFilePath);
                    window.Close();
                }



            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Lỗi khi tạo dự án: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
