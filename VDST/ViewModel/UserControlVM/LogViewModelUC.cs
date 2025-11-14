using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using VDST.Commands;
using Serilog;
using VDST.Model.Interfaces;
using VDST.View.VUserControl;
using VDST.Sevices;
using VDST.Sevices.DTOs;
using VDST.Utils;
using VDST.View;
using VDST.Repositories;

namespace VDST.ViewModel.UserControlVM
{
    public class LogViewModelUC : ViewModelBase, IViewModel
    {

        private string _username;
        public string Username
        {
            get { return _username; }
            set
            {
                _username = value;
                Log.Information("Username changed to: " + value);
                RaisePropertyChanged(nameof(Username));
                (LoginCommand as VfxCommand)?.RaiseCanExecuteChanged();
            }
        }

        private string _password;
        public string Password
        {
            get { return _password; }
            set
            {
                _password = value;
                Log.Information("Password changed to: " + value);
                RaisePropertyChanged(nameof(Password));
                (LoginCommand as VfxCommand)?.RaiseCanExecuteChanged();

            }
        }

        private bool _isRememberMe;
        public bool IsRememberMe
        {
            get { return _isRememberMe; }
            set
            {
                _isRememberMe = value;
                Log.Information("IsRememberMe changed to: " + value);
                RaisePropertyChanged(nameof(IsRememberMe));
            }
        }


        public event Action RegisterRequested;

        public event Action LoginSucess;

        public ICommand LoginCommand { get; set; }
        public ICommand RegisterCommand { get; set; }
        public LogViewModelUC()
        {
            Init_Data();

        }
        public void Init_Data()
        {
            LoginCommand = new VfxCommand(OnLogin, CanLogin);
            RegisterCommand = new VfxCommand(OnGoToRegister, () => true);
            LoadCredential();
        }
        private void LoadCredential()
        {
            // check saved credential
            var credential = TokenRespositories.Instance.LoadCredential();
            if (credential != null)
            {
                Username = credential.username;
                Password = credential.password;
                IsRememberMe = true;
            }
            else
            {
                IsRememberMe = false;
            }
        }

        private void OnGoToRegister(object obj)
        {
            RegisterRequested?.Invoke();
        }

        private bool CanLogin()
        {
            if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(Password))
            {
                return true;
            }
            return false;
        }



        private async void OnLogin(object obj)
        {

            var realComputerId = Utils.ComputerInfo.GetComputerId();
            var bodyRequest = new VaivDTO
            {
                username = Username,
                password = Password,
                computerId = realComputerId
            };
            var resultVaiv = await VaivService.Instance.LoginAsync(bodyRequest);
            if (resultVaiv != null)
            {
                resultVaiv.Expried = DateTime.Now.AddSeconds(resultVaiv.expires_in);

                Log.Information("ResponseVaiv Login Success :" + resultVaiv.Expried);

                TokenRespositories.Instance.SaveToken(resultVaiv);

                if (IsRememberMe)
                {
                    TokenRespositories.Instance.SaveCredential(Username, Password);
                }
                else
                {
                    TokenRespositories.Instance.ClearCrendential();
                }

                if (obj is Window window)
                {
                    MainWindows mainWindowsView = new MainWindows();
                    mainWindowsView.Show();
                    window.Close();

                }
            }
            else
            {
                return;
            }

        }
    }
}
