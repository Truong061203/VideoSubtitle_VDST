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
    /// <summary>
    /// save on load file ComputerId và KeyEncryptAES
    /// </summary>
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
                    Log.Information("KeyEncryptAES changed to: " + value);
                    RaisePropertyChanged(nameof(KeyEncryptAES));
                }
            }
        }
        /// <summary>
        /// Load File ComputerId từ path
        /// </summary>
        public void LoadFileC(string path)
        {
            try
            {

                // check file tồn tại thì đọc và gán giá trị cho ComputerId và EncryptAES
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
                        KeyEncryptAES = model.KeyEncryptAES;
                    }
                }
                if (string.IsNullOrWhiteSpace(ComputerId) && string.IsNullOrWhiteSpace(KeyEncryptAES))
                {
                    ComputerId = ComputerInfo.GetComputerId();
                    KeyEncryptAES = Guid.NewGuid().ToString("N");
                    OnSaveFileC(path);
                }

            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading settings: " + ex.Message);
            }
        }

        /// <summary>
        /// Save File ComputerId On Path 
        /// </summary>
        public void OnSaveFileC(string path)
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
