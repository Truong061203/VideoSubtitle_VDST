using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management;
using Serilog;
using Microsoft.VisualBasic.Logging;
using XSystem.Security.Cryptography;

namespace VDST.Utils
{
    public class ComputerInfo
    {

        // Hàm chính mà "Quản lý" (SettingApp) sẽ gọi
        // Chúng ta sẽ lấy mã UUID (Mã hệ thống)

        public static string GetComputerId(string prefix = "")
        {
            // Ghép 3 mã và băm (hash)
            return prefix + GetHash("CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId());
        }

        
        
        private static string GetHash(string s)
        {
            return GetHexString(new MD5CryptoServiceProvider().ComputeHash(new ASCIIEncoding().GetBytes(s)));
        }

     
        private static string GetHexString(byte[] bt)
        {
            string text = string.Empty;
            for (int i = 0; i < bt.Length; i++)
            {
                byte num = bt[i];
                int num2 = num & 0xF;
                int num3 = (num >> 4) & 0xF;
                string text2 = ((num3 <= 9) ? (text + num3) : (text + (char)(num3 - 10 + 65)));
                text = ((num2 <= 9) ? (text2 + num2) : (text2 + (char)(num2 - 10 + 65)));
                if (i + 1 != bt.Length && (i + 1) % 2 == 0)
                {
                    text += "-";
                }
            }
            return text;
        }


        //
        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string text = "";
            try
            {
                ManagementObjectSearcher searcher = new ManagementObjectSearcher($"SELECT {wmiProperty} FROM {wmiClass}");
                foreach (ManagementObject instance in searcher.Get().OfType<ManagementObject>())
                {
                    text = instance[wmiProperty]?.ToString();
                    if (!string.IsNullOrEmpty(text))
                        break;
                }
            }
            catch { /* Bỏ qua lỗi */ }
            return text ?? "";
        }

        //
        private static string CpuId()
        {
            string text = Identifier("Win32_Processor", "UniqueId");
            if (text == "")
            {
                text = Identifier("Win32_Processor", "ProcessorId");
                if (text == "")
                {
                    text = Identifier("Win32_Processor", "Name"); // Lấy Tên CPU
                    if (text == "")
                        text = Identifier("Win32_Processor", "Manufacturer");
                    text += Identifier("Win32_Processor", "MaxClockSpeed"); // Lấy tốc độ
                }
            }
            return text;
        }

        //
        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") +
                   Identifier("Win32_BIOS", "SMBIOSBIOSVersion") +
                   Identifier("Win32_BIOS", "SerialNumber");
        }

        //
        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model") +
                   Identifier("Win32_BaseBoard", "Manufacturer") +
                   Identifier("Win32_BaseBoard", "SerialNumber");
        }
    }
}
