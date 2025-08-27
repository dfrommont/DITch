using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DITch
{
    public class HashingTool
    {
        private SHA1 Sha1;
        private SHA256 Sha256;
        private bool HashMode;

        public HashingTool(bool HashMode) {
            Sha1 = SHA1.Create();
            Sha256 = SHA256.Create();
            this.HashMode = HashMode;
        }

        public HashingTool(int HashMode)
        {
            Sha1 = SHA1.Create();
            Sha256 = SHA256.Create();
            this.HashMode = HashMode > 0;
        }

        public bool ChangeMode(bool mode)
        {
            this.HashMode = mode;
            return HashMode;
        }

        public bool ChangeMode()
        {
            this.HashMode = !this.HashMode;
            return HashMode;
        }

        public bool GetMode() => HashMode;

        public byte[] Hash(byte[] data) {
            return HashMode == false ? Sha1.ComputeHash(data) : Sha256.ComputeHash(data);
        }

        public static string ToBase64Url(byte[] data)
        {
            return Convert.ToBase64String(data)
                .Replace('+', '-')
                .Replace('/', '_')
                .TrimEnd('=');
        }

        public static byte[] FromBase64Url(string base64Url)
        {
            string padded = base64Url
                .Replace('-', '+')
                .Replace('_', '/');
            switch (padded.Length % 4)
            {
                case 2: padded += "=="; break;
                case 3: padded += "="; break;
            }
            return FromBase64Url(padded);
        }
    }
}
