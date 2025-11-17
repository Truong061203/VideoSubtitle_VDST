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
        // lấy mã UUID (Mã hệ thống)
        // CPU, Mainboard, BIOS.

        public static string GetComputerId(string prefix = "")
        {
            return prefix + GetHash("CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId());
        }

        public static string GenerateComputerId(string prefix)
        {
            return prefix + GetHash("CPU >> " + CpuId() + "\nBIOS >> " + BiosId() + "\nBASE >> " + BaseId() + $">> {DateTime.Now}");
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

        private static string Identifier(string wmiClass, string wmiProperty)
        {
            string text = "";
            foreach (ManagementObject instance in new ManagementClass(wmiClass).GetInstances().Cast<ManagementObject>())
            {
                if (text == "")
                {
                    try
                    {
                        text = instance[wmiProperty]?.ToString();
                    }
                    catch
                    {
                        continue;
                    }

                    break;
                }
            }

            return text;
        }

        private static string CpuId()
        {
            string text = Identifier("Win32_Processor", "UniqueId");
            if (text == "")
            {
                text = Identifier("Win32_Processor", "ProcessorId");
                if (text == "")
                {
                    string text2 = Identifier("Win32_Processor", "Name");
                    if (text2 == "")
                    {
                        text2 = Identifier("Win32_Processor", "Manufacturer");
                    }

                    text = text2 + Identifier("Win32_Processor", "MaxClockSpeed");
                }
            }

            return text;
        }

        private static string BiosId()
        {
            return Identifier("Win32_BIOS", "Manufacturer") + Identifier("Win32_BIOS", "SMBIOSBIOSVersion") + Identifier("Win32_BIOS", "IdentificationCode") + Identifier("Win32_BIOS", "SerialNumber") + Identifier("Win32_BIOS", "ReleaseDate") + Identifier("Win32_BIOS", "Version");
        }

        private static string BaseId()
        {
            return Identifier("Win32_BaseBoard", "Model") + Identifier("Win32_BaseBoard", "Manufacturer") + Identifier("Win32_BaseBoard", "Name") + Identifier("Win32_BaseBoard", "SerialNumber");
        }

    }
}
