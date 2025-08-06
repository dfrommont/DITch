using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace DITch
{
    internal class HashingTool
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

        public byte[] Hash(byte[] data) {
            return HashMode == false ? Sha1.ComputeHash(data) : Sha256.ComputeHash(data);
        }
    }
}
