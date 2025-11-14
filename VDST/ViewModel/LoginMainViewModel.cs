using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VDST.Commands;
using Serilog;
using VDST.Model.Interfaces;
using System.Windows;
using VDST.View.VUserControl;
using VDST.ViewModel.UserControlVM;
using VDST.Repositories;
using VDST.View;

namespace VDST.ViewModel
{
    public class LoginMainViewModel : ViewModelBase, IViewModel
    {
        public event Action RequestClose;

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
                _logUC = new LogViewModelUC();
                _registerUC = new RegisterViewModelUC();
                _logUC.RegisterRequested += ShowRegisterView;
                _registerUC.BackToLoginRequested += ShowLoginView;
                _CurrentView = _logUC;
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
