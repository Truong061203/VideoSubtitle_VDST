using System;
using System.Collections.Generic;
using System.IO;
using Serilog;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Utils
{
    /// <summary>
    /// Save global variable for File App
    /// </summary>
    /// Tạo thư mục lưu trữ cài đặt ứng dụng trong AppData/Roaming
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
        public string Path_Save_ComputerId { get; set; }
        public string Path_Save_Token { get; set; }
        public string Path_Save_Credential { get; set; }
        public string PATH_SAVE_RECENT_PROJECT { get; private set; }

        public GlobalVariable()
        {
            Loaded();
        }
        public void Loaded()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            APP_SETTING_FOLER = Path.Combine(appData, "VDST");
            Log.Information("APP_SETTING_FOLER :" + APP_SETTING_FOLER);
            if (!Directory.Exists(APP_SETTING_FOLER))
            {
                Directory.CreateDirectory(APP_SETTING_FOLER);
            }
            Path_Save_ComputerId = Path.Combine(APP_SETTING_FOLER, "ComputerId.json");

            Path_Save_Token = Path.Combine(APP_SETTING_FOLER, "token.VDST");
            Path_Save_Credential = Path.Combine(APP_SETTING_FOLER, "Credential.VDST");
            PATH_SAVE_RECENT_PROJECT = Path.Combine(APP_SETTING_FOLER, "recent_projects.json");
        }


    }
}
