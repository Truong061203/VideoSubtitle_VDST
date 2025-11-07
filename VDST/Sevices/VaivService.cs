using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Sevices
{
    public class VaivService
    {
        private static VaivService _instance;
        public static VaivService Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new VaivService();
                }
                return _instance;
            }
        } 
        private readonly HttpClient _httpClient;  
        public string BaseUrl = "https://api.vaiv.yteco.live/auth/";

    }
}
