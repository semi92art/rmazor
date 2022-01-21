using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Common.Utils.Encryption
{
    public class Cryptography : IDisposable
    {
        private static Cryptography _instance;
        public static  Cryptography Instange => _instance ??= new Cryptography("---");
        
        
        private readonly Aes m_Encryptor;

        public Cryptography(string _Key)
        {
            m_Encryptor = Aes.Create();
            var pdb = new Rfc2898DeriveBytes(_Key, new byte[]
            {
                0x49, 0x76, 0x61, 0x6e, 0x20, 0x4d, 0x65, 0x64, 0x76, 0x65, 0x64, 0x65, 0x76
            });

            if (m_Encryptor == null)
                return;
            m_Encryptor.Key = pdb.GetBytes(32);
            m_Encryptor.IV = pdb.GetBytes(16);
        }

        public string Encrypt(string _PlainText)
        {
            if (string.IsNullOrEmpty(_PlainText))
                return _PlainText;

            var clearBytes = Encoding.Unicode.GetBytes(_PlainText);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, m_Encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(clearBytes, 0, clearBytes.Length);
            }
            _PlainText = Convert.ToBase64String(ms.ToArray());

            return _PlainText;
        }

        public string Decrypt(string _CipherText)
        {
            if (string.IsNullOrEmpty(_CipherText))
                return _CipherText;

            _CipherText = _CipherText.Replace(" ", "+");
            var cipherBytes = Convert.FromBase64String(_CipherText);

            using var ms = new MemoryStream();
            using (var cs = new CryptoStream(ms, m_Encryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
                cs.Write(cipherBytes, 0, cipherBytes.Length);
            }
            _CipherText = Encoding.Unicode.GetString(ms.ToArray());
            return _CipherText;
        }

        public void Dispose()
        {
            m_Encryptor.Dispose();
        }
    }
}