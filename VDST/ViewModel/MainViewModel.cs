using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VDST.Commands;
using VDST.Model;
using VDST.Repositories;

namespace VDST.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private ProjectInfomation _currentProject;
        public ProjectInfomation CurrentProject
        {
            get => _currentProject;
            set 
            {
                _currentProject = value; 
                RaisePropertyChanged(nameof(CurrentProject));
            }
        }

        public ICommand SaveCommand { get; set; }

        public MainViewModel()
        {
            SaveCommand = new VfxCommand(OnSave, () => CurrentProject != null);
        }

        // Hàm này được gọi từ bên ngoài (Lúc mới mở app)
        public void LoadProjectData(string filePath)
        {
            try
            {
                CurrentProject = ProjectRepositories.Instance.LoadProject(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        // Hàm lưu khi người dùng bấm Ctrl+S hoặc nút Lưu
        private void OnSave(object obj)
        {
            if (CurrentProject != null)
            {
                ProjectRepositories.Instance.SaveProject(CurrentProject);
                MessageBox.Show("Đã lưu dự án thành công!");
            }
        }

        // --- Sau này, khi làm chức năng YouTube ---
        public void UpdateVideoPath(string newPath)
        {
            CurrentProject.VideoPath = newPath;
            // Tự động lưu luôn để chắc ăn
            ProjectRepositories.Instance.SaveProject(CurrentProject);
        }
    }
}
