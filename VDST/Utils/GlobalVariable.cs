using Microsoft.VisualBasic.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Utils
{
    public class GlobalVariable
    {
        private static GlobalVariable _instance;
        public static GlobalVariable Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GlobalVariable();
                }
                return _instance;
            }
        }
        public string APP_SETTING_FOLER { get; set; }
        public string PATH_SETTING_APP { get; set; }

        public GlobalVariable()
        {
            // C:\Users\<TênBạn>\AppData\Roaming
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            APP_SETTING_FOLER = System.IO.Path.Combine(appData, "VDST_App");
            Serilog.Log.Information("App_setting_folder :" +APP_SETTING_FOLER);
            if (!Directory.Exists(APP_SETTING_FOLER))
            {
                Directory.CreateDirectory(APP_SETTING_FOLER);
            }
            PATH_SETTING_APP = System.IO.Path.Combine(APP_SETTING_FOLER, "settingApp.json");
            Serilog.Log.Information("Path_setting_folder :" + APP_SETTING_FOLER);
        }
    }
}
