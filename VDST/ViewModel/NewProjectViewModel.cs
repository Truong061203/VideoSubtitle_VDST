using Microsoft.Web.WebView2.Core;
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
using VDST.View;

namespace VDST.ViewModel
{
    public class NewProjectViewModel : ViewModelBase, IViewModel
    {
        public ProjectInfo Project { get; set; }
        public ObservableCollection<string> Languages { get; set; }
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
            Project = new ProjectInfo();
            Languages = new ObservableCollection<string>
            {
                "Auto Detect", "Vietnamese", "English", "Japanese", "Korean", "Chinese"
            };
            Project.SourceLang = "Auto Detect";
            Project.TargetLang = "English";
        }

        private bool CanOk()
        {
            if (string.IsNullOrEmpty(Project.ProjectName) ||
                string.IsNullOrEmpty(Project.FolderName) ||
                string.IsNullOrEmpty(Project.SaveLocation) ||
                string.IsNullOrEmpty(Project.SourceLang) ||
                string.IsNullOrEmpty(Project.TargetLang))
                return false;
            return true;
        }

        private void OnOk(object obj)
        {
            var fullPath = Path.Combine(Project.SaveLocation, Project.FolderName);
            if (Directory.Exists(fullPath))
            {
                System.Windows.MessageBox.Show("Thư mục dự án đã tồn tại. Vui lòng chọn tên khác.", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Directory.CreateDirectory(fullPath);

            if (obj is Window window)
            {
                MainWindows main = new MainWindows(); 
                main.Show();
                window.Close();
            }
        }


        private void OnChoose(object obj)
        {
            using (var dialog = new FolderBrowserDialog())
            {
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    Project.SaveLocation = dialog.SelectedPath;
                    // Cập nhật lại trạng thái nút OK
                    (OkCommand as VfxCommand)?.RaiseCanExecuteChanged();
                }
            }
        }
    }
}
