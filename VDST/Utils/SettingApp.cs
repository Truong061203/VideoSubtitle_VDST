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
    public class SettingApp : ViewModelBase
    {
        private static SettingApp _instance;
        public static SettingApp Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SettingApp();
                }
                return _instance;
            }

        }

        private SettingApp()
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

        private string _keyEncryptAES;
        public string KeyEncryptAES
        {
            get => _keyEncryptAES;
            set
            {
                if (_keyEncryptAES != value)
                {
                    _keyEncryptAES = value;
                    // Nhớ dùng hàm RaisePropertyChanged hoặc OnPropertyChanged của bạn
                    RaisePropertyChanged(nameof(KeyEncryptAES));
                }
            }
        }
        public void OnLoadFile(string path)
        {
            try
            {
               

                //có rồi thì đọc file và load lên object
                if (File.Exists(path))
                {
                    // đọc file json settingApp.json ComputerId
                    var json = File.ReadAllText(path);
                    Log.Information("Loading Setting App Json: " + json);
                    // chuyển json thành object SettingApps
                    var model = JsonConvert.DeserializeObject<SettingApp>(json);
                    Log.Information("Convert Object :" + model.ComputerId);

                    if (model != null)
                    {
                        ComputerId = model.ComputerId;
                    }

                }
                // nếu chưa có thì get ComputerId mới và lưu vào file 

                if (string.IsNullOrWhiteSpace(this.ComputerId))
                {
                    ComputerId = ComputerInfo.GetComputerId();
                    OnSave(path);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Save đối tượng SettingApps thành file json
        /// </summary>
        /// <param name="path"> Lưu đối tượng dưới dạng json vào file Path</param>
        public void OnSave(string path)
        {
            try
            {
                // chuyển đổi đối tượng SettingApps thành json
                var json = JsonConvert.SerializeObject(Instance, Formatting.Indented);
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
