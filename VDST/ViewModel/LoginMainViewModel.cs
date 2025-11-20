using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VDST.Commands;
using VDST.Model.Interfaces;
using VDST.Repositories;
using VDST.View;
using VDST.View.VUserControl;
using VDST.View.Windows;
using VDST.ViewModel.UserControlVM;

namespace VDST.ViewModel
{
    public class LoginMainViewModel : ViewModelBase, IViewModel
    {

        private object _CurrentView;
        public object CurrentViewModel
        {
            get { return _CurrentView; }
            set
            {
                _CurrentView = value;
                Log.Information("Switched to view: {ViewType}", value);
                RaisePropertyChanged(nameof(CurrentViewModel));
            }
        }

        private LogViewModelUC _logUC;
        private RegisterViewModelUC _registerUC;
        public LoginMainViewModel()
        {
            Init_Data();
        }

        public void Init_Data()
        {
            var token = TokenRespositories.Instance.LoadToken();
            // Kiểm tra: Có token VÀ Còn hạn không?
            if (token != null && !string.IsNullOrEmpty(token.access_token) && token.Expried > DateTime.Now)
            {
                NewProjectConfiguration newProjectConfiguration = new NewProjectConfiguration();
                newProjectConfiguration.ShowDialog();
            }
            else
            {
                _logUC = new LogViewModelUC();
                _registerUC = new RegisterViewModelUC();
                _logUC.RegisterRequested += ShowRegisterView;
                _registerUC.BackToLoginRequested += ShowLoginView;
                _CurrentView = _logUC;
            }
        }


        public void ShowLoginView()
        {
            CurrentViewModel = _logUC;
        }
        public void ShowRegisterView()
        {
            CurrentViewModel = _registerUC;
        }
    }
}
