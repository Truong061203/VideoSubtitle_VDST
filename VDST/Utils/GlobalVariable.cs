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
        public string Path_Save_App_Setting { get; set; }

        public GlobalVariable()
        {

            Loaded();
        }
        public void Loaded()
        {
            // C:\Users\<TênBạn>\AppData\Roaming
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            APP_SETTING_FOLER = Path.Combine(appData, "VDST");
            Log.Information("APP_SETTING_FOLER :" + APP_SETTING_FOLER);
            if (!Directory.Exists(APP_SETTING_FOLER))
            {
                Directory.CreateDirectory(APP_SETTING_FOLER);
            }
            Path_Save_App_Setting = Path.Combine(APP_SETTING_FOLER, "ComputerId.json");
            Log.Information("Path_setting_folder :" + Path_Save_App_Setting);

        }


    }
}
