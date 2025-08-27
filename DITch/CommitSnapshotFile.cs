using System.Xml.Linq;

namespace DITch
{
    public class CommitSnapshotFile
    {
        private byte[] fileHash;
        private string fileName;
        private string status;

        public CommitSnapshotFile(byte[] fileHash, string fileName, string status)
        {
            this.fileHash = fileHash;
            this.fileName = fileName;
            this.status = status;
        }

        public byte[] GetFileHash() => fileHash;
        public void SetFileHash(byte[] fileHash) => this.fileHash = fileHash;

        public string GetFileName() => fileName;
        public void SetFileName(string fileName) => this.fileName = fileName;

        public string GetStatus() => status;
        public void SetStatus(string status) => this.status = status; 
    }
}