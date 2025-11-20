using Newtonsoft.Json;
using Serilog;
using System.IO;
using System.Windows;
using VDST.Sevices.DTOs;
using VDST.Utils;
using VDST.ViewModel;

namespace VDST.Repositories
{

    public class TokenRespositories : ViewModelBase
    {
        private static TokenRespositories _instance;
        public static TokenRespositories Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new TokenRespositories();
                }
                return _instance;
            }
        }
        private string _pathToken { get; set; }
        private string _credentialPath { get; set; }


        /// <summary>
        /// Save Token Respositories and Credential File 
        /// </summary>
        public TokenRespositories()
        {
            _pathToken = GlobalVariable.Instance.Path_Save_Token;
            _credentialPath = GlobalVariable.Instance.Path_Save_Credential;
        }


        /// <summary>
        /// Save File Token VaiV    
        /// </summary>
        public void SaveToken(VaivResponseDTO response)
        {
            try
            {

                // chuyển đổi đối tượng thành chuỗi JSON
                var jsonResonse = JsonConvert.SerializeObject(response);
                var encrypted = SecurityHelper.Encrypt(jsonResonse);
                File.WriteAllText(_pathToken, encrypted);

            }
            catch (Exception ex)
            {
                MessageBox.Show("Save Token Error: " + ex.Message);
            }
        }

        /// <summary>
        /// Read File Token VaiV
        /// </summary>
        /// <returns></returns>
        public VaivResponseDTO LoadToken()
        {
            try
            {
                if (File.Exists(_pathToken))
                {
                    var json = File.ReadAllText(_pathToken);
                    var decrypted = SecurityHelper.Decrypt(json);
                    var response = JsonConvert.DeserializeObject<VaivResponseDTO>(decrypted);
                    return response;
                }
                return null;
            }
            catch (Exception ex)
            {
                Log.Information("Load Token Error: " + ex.Message);
                return null;
            }
        }


        /// <summary>
        /// Save Credential User and Password
        /// </summary>
        /// <param name="credential"></param>
        public void SaveCredential(string _username, string _password)
        {
            // chuyển đổi đối tượng thành chuỗi JSON
            var userSave = new VaivDTO
            {
                username = _username,
                password = _password
            };
            var jsonCredential = JsonConvert.SerializeObject(userSave);
            Log.Information("Save Credential Json: " + jsonCredential);

            // mã hoá và lưu tk mk xuông file
            string encrypted = SecurityHelper.Encrypt(jsonCredential);
            File.WriteAllText(_credentialPath, encrypted);
        }



        /// <summary>
        /// Read Credential User and Password
        /// </summary>
        /// <returns></returns>
        public VaivDTO LoadCredential()
        {
            if (!File.Exists(_credentialPath))
                return null;
            // read file và giải mã
            var jsonEncrypted = File.ReadAllText(_credentialPath);
            string jsonSercurity = SecurityHelper.Decrypt(jsonEncrypted);

            var converted = JsonConvert.DeserializeObject<VaivDTO>(jsonSercurity);
            return converted;
        }

        /// <summary>
        /// Delete Token and Credential File
        /// </summary>
        public void ClearAll()
        {
            if (File.Exists(_pathToken))
                File.Delete(_pathToken);

            if (File.Exists(_credentialPath))
                File.Delete(_credentialPath);
        }
        public void ClearCrendential()
        {
            if (File.Exists(_credentialPath))
                File.Delete(_credentialPath);
        }

    }
}
