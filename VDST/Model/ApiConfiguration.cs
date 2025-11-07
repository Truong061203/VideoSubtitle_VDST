using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Model
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; } = "https://api.vaiv.yteco.live/";
   public string LogToolEndpoint { get; set; } = "auth/login-tool";
    }
}
