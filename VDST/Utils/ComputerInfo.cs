using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Serilog;
using Microsoft.VisualBasic.Logging;

namespace VDST.Utils
{
    public class ComputerInfo
    {

        // Hàm chính mà "Quản lý" (SettingApp) sẽ gọi
        // Chúng ta sẽ lấy mã UUID (Mã hệ thống)
        public static string GetComputerId()
        {
            return Identifier("Win32_ComputerSystemProduct", "UUID");
        }

        // Hàm helper chính để truy vấn WMI
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string text = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {wmiProperty} FROM {wmiClass}");
                foreach (ManagementObject instance in searcher.Get().OfType<ManagementObject>())
                {
                    text = instance[wmiProperty]?.ToString(); 
                    Serilog.Log.Information("Computer {0} : {1}", wmiProperty, text);

                    if (!string.IsNullOrEmpty(text))
                    {
                        break;
                    }
                }
            }
            catch (Exception)
            {
                return "WMI_Error";
            }
            return text ?? "UUID_NotFound";
        }
    }
}
