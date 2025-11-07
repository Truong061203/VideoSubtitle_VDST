using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using VDST.Model.Interfaces;

namespace VDST.ViewModel.UserControlVM
{
    public class RegisterViewModelUC : ViewModelBase, IViewModel
    {
        public event Action BackToLoginRequested;

        public ICommand BackToLoginCommand { get; set; }
        private string _computerId = "0E68-8508-90CC-CA49-3790-2241-5BE2-9809";
        public string ComputerId
        {
            get { return _computerId; }
            set
            {
                _computerId = value;
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
        }

        private void OnBackToLogin(object obj)
        {
            BackToLoginRequested?.Invoke();
        }
    }
}
