using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VDST.ViewModel;

namespace VDST.Utils
{
    public class SettingApps : ViewModelBase
    {
        private static SettingApps _instance;
        public static SettingApps Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingApps();
                }
                return _instance;
            }

        }

        private SettingApps()
        {
        }

        private string _computerId;
        public string ComputerId
        {
            get { return _computerId; }
            set
            {
                _computerId = value;
                RaisePropertyChanged(nameof(ComputerId));
            }
        } 
        public void OnLoad(string path)
        {
        }
    }
}
