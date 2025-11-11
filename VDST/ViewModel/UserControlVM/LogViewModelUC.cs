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
        public event Action RegisterRequested;

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
            var bodyRequest = new VaivDTO
            {
                username = this.Username,
                password = this.Password,
                computerId = "0E68-8508-90CC-CA49-3790-2241-5BE2-9809"
            };
            var responseVaiv = await VaivService.Instance.LoginAsync(bodyRequest);
            if (responseVaiv != null)
            {
                MessageBox.Show("Login Successful! Access Token: ");
            }
            else
            {
                MessageBox.Show("Login Failed!");
            }

        }
    }
}
