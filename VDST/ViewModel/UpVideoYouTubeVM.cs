using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VDST.Commands;
using VDST.Model;
using VDST.Model.Interfaces;
using VDST.Repositories;

namespace VDST.ViewModel
{
    public class UpVideoYouTubeVM : ViewModelBase
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
        private string _title;
        public string Title
        {
            get => _title;
            set
            {
                _title = value;
                Log.Information("Change Title : " + _title);
                RaisePropertyChanged(nameof(Title));
            }
        }

        public ICommand LoadedCommand { get; set; }
        public ICommand SaveCommand { get; set; }





        private void OnLoaded(object obj)
        {
            if (obj is Window win)
            {
                if (win.Tag is string filePath)
                {
                    LoadProjectData(filePath);
                }
            }
        }

        // Hàm này được gọi từ bên ngoài (Lúc mới mở app)
        public void LoadProjectData(string filePath)
        {
            try
            {
                var current = ProjectRepositories.Instance.LoadProject(filePath);
                if (current != null)

                {
                    CurrentProject = current;
                    Title = "VDST - " + CurrentProject.ProjectName;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi mở project :" + ex.Message, "Errorr", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        // Hàm lưu khi người dùng bấm Ctrl+S hoặc nút Lưu
        private void OnSave(object obj)
        {
            if (CurrentProject != null)
            {
                MessageBox.Show("Đã lưu dự án thành công!");
            }
        }

    }
}
