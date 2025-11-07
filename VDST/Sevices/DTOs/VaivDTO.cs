using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Sevices.DTOs
{
  public  class VaivDTO
    { 
        public string username { get; set; } 
        public string password { get; set; } 
        public string computerId { get; set; }
    } 
    public class VaivResponseDTO
    {
        public string access_token { get; set; }
        public int expires_in { get; set; }
        public string refresh_token { get; set; }
        public int refresh_expires_in { get; set; }
    }
}
