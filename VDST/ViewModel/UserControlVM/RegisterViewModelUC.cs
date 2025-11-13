using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VDST.Model.Interfaces;
using VDST.Utils;

namespace VDST.ViewModel.UserControlVM
{
    public class RegisterViewModelUC : ViewModelBase, IViewModel
    {
        public event Action BackToLoginRequested;

        public ICommand BackToLoginCommand { get; set; }
        private string _computerId ;
        public string ComputerId
        {
            get { return _computerId; }
            set
            {
                _computerId = value;
                Log.Information("ComputerId changed to: " + value);
                RaisePropertyChanged(nameof(ComputerId));
            }
        }

        public RegisterViewModelUC()
        {
            Init_Data(); 
        }
        public void Init_Data()
        {
            BackToLoginCommand = new Commands.VfxCommand(OnBackToLogin, () => true);
            ComputerId = SettingApp.Instance.ComputerId;
        }

        private void OnBackToLogin(object obj)
        {
            BackToLoginRequested?.Invoke();
        }
    }
}
