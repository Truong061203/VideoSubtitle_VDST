using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
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


        public TokenRespositories()
        {
            _pathToken = GlobalVariable.Instance.Path_Save_Token;
        }
        public void SaveToken(VaivResponseDTO response)
        {
            // chuyển đổi đối tượng thành chuỗi JSON
            var jsonResonse = JsonConvert.SerializeObject(response);
            Log.Information("Save Token Json: " + jsonResonse);
            File.WriteAllText(_pathToken, jsonResonse);
        }

        public VaivResponseDTO LoadToken()
        {
                if (File.Exists(_pathToken))
                {
                    var json = File.ReadAllText(_pathToken);
                    var response = JsonConvert.DeserializeObject<VaivResponseDTO>(json);
                    return response;
                }
                return null;
        }

    }
}
