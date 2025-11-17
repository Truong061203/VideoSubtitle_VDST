using Newtonsoft.Json;
using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VDST.Repositories;

namespace VDST.Sevices
{
    public class VaivService
    {
        private readonly HttpClient _httpClient;
        public string BaseUrl = "https://api.vaiv.yteco.live/";
        private string ContentTypeJson = "application/json";

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


        public VaivService()
        {
            _httpClient = new HttpClient()
            {
                BaseAddress = new Uri(BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            };
            _httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(ContentTypeJson));
        }


        public void SetAuthenticationToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
        }

        public async Task<DTOs.VaivResponseDTO> LoginAsync(DTOs.VaivDTO bodyLogin)
        {
            var convertBody = JsonConvert.SerializeObject(bodyLogin);
            var request = new StringContent(convertBody, Encoding.UTF8, ContentTypeJson);
            Log.Information("Vaiv Request Body : " + request);
            var response = await _httpClient.PostAsync("auth/login-tool", request);
            Log.Information("Response Vaiv + "+response);

            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var vaivResponse = JsonConvert.DeserializeObject<DTOs.VaivResponseDTO>(responseContent);
                Log.Information("Vaiv Response Convert : " + vaivResponse);

                SetAuthenticationToken(vaivResponse.access_token);


                return vaivResponse;
            }

            var reasone = response.ReasonPhrase;
            Log.Error("Vaiv Login Failed: " + reasone);
            var statusCode = (int)response.StatusCode;
            if (statusCode >= 400 && statusCode < 500)
            {
                MessageBox.Show("Thông tin đăng nhập chưa đúng! ","Thông Báo" , MessageBoxButton.OK , MessageBoxImage.Error);
            }
            else if (statusCode >= 500)
            {
               
                MessageBox.Show("Lỗi máy chủ , vui lòng thử lại sau! ", "Thông Báo", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
            return null;

        }
    }
}
