using Serilog;
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
using VDST.Model.Interfaces;
using VDST.Repositories;

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
                RaisePropertyChanged(nameof(YoutubeUrl));
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
                    TitleApp = "VDST - "+ project.ProjectName;
                    ProjectFilePath = projectFilePath; // GIỮ LUÔN ĐỂ SAU NÀY SAVE
                }
            }
        }

        private void SaveProject()
        {
            if (CurrentProject == null || string.IsNullOrEmpty(ProjectFilePath))
                return;

            ProjectRepositories.Instance.SaveProject(CurrentProject, ProjectFilePath);
        }


    }
}
