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
using VDST.Utils;
using VDST.View;

namespace VDST.ViewModel
{
    public class NewProjectViewModel : ViewModelBase, IViewModel
    {
        public ProjectInfo projectInfo { get; set; }
        public string CreatedProjectPath { get; private set; } // Thêm thuộc tính này
        public ObservableCollection<LanguageOption> SourceLanguages { get; set; }
        public ObservableCollection<LanguageOption> TargetLanguages { get; set; }
        public ICommand ChooseCommand { get; set; }
        public ICommand OkCommand { get; set; }


        public NewProjectViewModel()
        {
            Init_Data();
        }
        public void Init_Data()
        {
            ChooseCommand = new VfxCommand(OnChoose, () => true);
            OkCommand = new VfxCommand(OnOk, CanOk);
            projectInfo = new ProjectInfo();
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
            projectInfo.SaveLocation = string.Empty;

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

                if (projectInfo.SourceLanguage.IsAuto = false && projectInfo.TargetLanguage.Code == projectInfo.SourceLanguage.Code)
                {
                    System.Windows.MessageBox.Show("Ngôn ngữ đích phải khác ngôn ngữ gốc.\nVí dụ: Source = English → Target = Vietnamese.",
                                                    "Xung đột ngôn ngữ",
                                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                if (!ProjectInfo.IsProjectNameValid(projectInfo.ProjectName))
                {
                    System.Windows.MessageBox.Show(
                        "Tên dự án không hợp lệ.\n" +
                        "Chỉ được dùng chữ cái không dấu (A-Z), số, khoảng trắng, _ và -.\n" +
                        "Vui lòng không dùng tiếng Việt có dấu hoặc ký tự đặc biệt.", "Errorr", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                string fullPath = projectInfo.GetFullPath();
                if (Directory.Exists(fullPath))
                {
                    System.Windows.MessageBox.Show("Thư mục dự án đã tồn tại, vui lòng chọn tên khác.",
                                                   "Trùng tên thư mục",
                                                    MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                Directory.CreateDirectory(fullPath);
                Directory.CreateDirectory(Path.Combine(fullPath, "input"));
                Directory.CreateDirectory(Path.Combine(fullPath, "output"));
                Directory.CreateDirectory(Path.Combine(fullPath, "audio"));

                System.Windows.MessageBox.Show("Tạo dự án thành công!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);

                string projectFilePath = Path.Combine(fullPath, projectInfo.FolderName + ".VDST");
                Log.Information("Đường dẫn file dự án: {ProjectFilePath}", projectFilePath);

                string jsonContent = JsonConvert.SerializeObject(projectInfo, Formatting.Indented);
                Log.Information("Nội dung file dự án: {JsonContent}", jsonContent);
                File.WriteAllText(projectFilePath, jsonContent);

                // 5. LƯU VÀO "RECENT PROJECTS" (Dự án gần đây)
                // Gọi Repository của Senior để lưu
                //RecentProjectRepository.OnAddRecentPath(RecentProjectModel.Instance, projectFilePath);
                //RecentProjectRepository.OnSave(RecentProjectModel.Instance, GlobalVariable.Instance.PATH_SAVE_RECENT_PROJECT);

                if (obj is Window window)
                {

                  
                    window.DialogResult = true;
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
