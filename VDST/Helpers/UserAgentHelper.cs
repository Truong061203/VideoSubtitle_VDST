using Serilog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Helpers
{
    public static class UserAgentHelper
    {
        private static readonly Random _random = new Random();
        private static string _currentUserAgent;

        // Danh sách VIP (Desktop Only)
        private static readonly List<string> _safeUserAgents = new List<string>
        {
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/121.0.0.0 Safari/537.36",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.0.0 Safari/537.36 Edg/122.0.0.0",
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:123.0) Gecko/20100101 Firefox/123.0"
        };

        static UserAgentHelper()
        {
            RotateUserAgent();
        }

    
        public static string GetCurrentUA()
        {
            if (string.IsNullOrWhiteSpace(_currentUserAgent))
            {
                Log.Warning("UA hiện tại bị rỗng. Đang khởi tạo lại...");
                RotateUserAgent();
            }
            return _currentUserAgent;
        }

     
        public static void RotateUserAgent()
        {
            try
            {
                int index = _random.Next(_safeUserAgents.Count);
                string oldUA = _currentUserAgent;
                _currentUserAgent = _safeUserAgents[index];

                // Đảm bảo không random trùng lại cái cũ (nếu list > 1)
                if (_safeUserAgents.Count > 1 && _currentUserAgent == oldUA)
                {
                    RotateUserAgent();
                    return;
                }

            }
            catch (Exception ex)
            {
                Log.Error(ex, "Lỗi khi Rotate UA. Fallback về Chrome mặc định.");
                _currentUserAgent = _safeUserAgents[0];
            }
        }
    }
}
