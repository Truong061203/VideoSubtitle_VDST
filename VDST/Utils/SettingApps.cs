using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
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
                Log.Information("ComputerId changed to: " + value);
                RaisePropertyChanged(nameof(ComputerId));
            }
        }

        public void OnLoadFile(string path)
        {
            //có rồi thì đọc file và load lên object
            if (File.Exists(path))
            {
                try
                {

                    // đọc file json settingApp.json ComputerId
                    var json = File.ReadAllText(path);
                    Log.Information("Loading Setting App Json: " + json);
                    // chuyển json thành object SettingApps
                    _instance = JsonConvert.DeserializeObject<SettingApps>(json); 
                    Log.Information("Loaded ComputerId: " + _instance.ComputerId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading settings: " + ex.Message);
                }
            }
            // nếu chưa có thì get ComputerId mới và lưu vào file 
            ComputerId = ComputerInfo.GetComputerId(); 
            OnSave(path);

        }

        /// <summary>
        /// Save đối tượng SettingApps thành file json
        /// </summary>
        /// <param name="path"> Lưu đối tượng dưới dạng json vào file Path</param>
        public void OnSave(string path)
        {
            try
            {
                var json = JsonConvert.SerializeObject(_instance.ComputerId, Formatting.Indented);
                Log.Information("Setting App Json: " + json);
                File.WriteAllText(path, json);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
            }
        }
    }
}
