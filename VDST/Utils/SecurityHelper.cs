using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace VDST.Utils
{
    public class SecurityHelper
    {
        // 1. Lấy khóa động từ Mã máy (Đã tích hợp trong file này để tiện dùng)
        private static string GetMyKey()
        {
            string machineId = ComputerInfo.GetComputerId();
            string secret = SettingApp.Instance.KeyEncryptAES ?? "VDST_Default";
            return machineId + "_" + secret;
        }

        // 2. Hàm Mã Hóa (Đã sửa lỗi Key)
        public static string Encrypt(string plainText)
        {
            // Tự động lấy khóa từ mã máy
            string keyString = GetMyKey();

            if (string.IsNullOrEmpty(plainText)) return "";

            try
            {
                using (var aes = Aes.Create())
                {
                    // SỬA LỖI: Dùng SHA256 để tạo Key chuẩn 32 bytes (256 bits) từ bất kỳ chuỗi nào
                    // Điều này an toàn hơn và không bao giờ bị lỗi độ dài
                    var keyBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(keyString));
                    aes.Key = keyBytes;

                    // Dùng 16 byte đầu của Key làm IV (để đơn giản hóa việc lưu trữ)
                    // Hoặc dùng IV mặc định (toàn 0) cũng được nếu không quá quan trọng bảo mật cao
                    var ivBytes = new byte[16];
                    Array.Copy(keyBytes, ivBytes, 16);
                    aes.IV = ivBytes;

                    ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);

                    using (MemoryStream ms = new MemoryStream())
                    {
                        using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                        using (StreamWriter sw = new StreamWriter(cs))
                        {
                            sw.Write(plainText);
                        }
                        return Convert.ToBase64String(ms.ToArray());
                    }
                }
            }
            catch { return ""; }
        }

        // 3. Hàm Giải Mã
        public static string Decrypt(string cipherText)
        {
            // Tự động lấy khóa từ mã máy
            string keyString = GetMyKey();

            if (string.IsNullOrEmpty(cipherText)) return "";

            try
            {
                using (var aes = Aes.Create())
                {
                    // Tái tạo Key y hệt lúc mã hóa
                    var keyBytes = SHA256.Create().ComputeHash(Encoding.UTF8.GetBytes(keyString));
                    aes.Key = keyBytes;

                    var ivBytes = new byte[16];
                    Array.Copy(keyBytes, ivBytes, 16);
                    aes.IV = ivBytes;

                    ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                    byte[] buffer = Convert.FromBase64String(cipherText);

                    using (MemoryStream ms = new MemoryStream(buffer))
                    using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                    using (StreamReader sr = new StreamReader(cs))
                    {
                        return sr.ReadToEnd();
                    }
                }
            }
            catch
            {
                return null; // Giải mã thất bại (do sai mã máy hoặc file lỗi)
            }
        }
    }
}
