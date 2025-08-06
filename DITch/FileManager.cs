using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DITch
{
    internal class FileManager
    {
        HashingTool hashingTool = new HashingTool(true); //temp reference for below

        //Compress
        public static byte[] Compress(string folderPath)
        {
            using var memStream = new MemoryStream();
            using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                foreach (var filePath in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                {
                    var entryName = Path.GetRelativePath(folderPath, filePath);
                    var entry = archive.CreateEntry(entryName);

                    using var entryStream = entry.Open();
                    using var fileStream = File.OpenRead(filePath);
                    fileStream.CopyTo(entryStream);
                }
            }

            memStream.Position = 0;
            return memStream.ToArray();
        }

        //Decompress
        public static bool Decompress(string SourcePath, string DestinationPath)
        {
            try
            {
                ZipFile.ExtractToDirectory(@SourcePath, @DestinationPath);
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error decompressing file: {SourcePath}, Error: {e.Message}");
                return false;
            }
        }

        //Encrypt
        public byte[] Encrypt(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(password, 16, 10000, hashingTool.GetMode() ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA1); // Salt & iterations
            aes.Key = key.GetBytes(32); // AES-256
            aes.IV = key.GetBytes(16);  // AES block size

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.Close();

            return ms.ToArray();
        }

        //Decrypt
        public byte[] Decrypt(byte[] encryptedData, string password)
        {
            using var aes = Aes.Create();
            var key = new Rfc2898DeriveBytes(password, 16, 10000, hashingTool.GetMode() ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA1);
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var ms = new MemoryStream();
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(encryptedData, 0, encryptedData.Length);
            cs.Close();

            return ms.ToArray();
        }


        //Copy file
        public bool CopyFile(string SourcePath, string destinationPath)
        {
            if (!File.Exists(SourcePath))
            {
                return false;
            }

            File.Copy(SourcePath, destinationPath, true);

            return true;
        }

        //Pull meta data
        public string GetMetaData(string filePath)
        {
            if (!File.Exists(filePath))
            {
                Console.WriteLine("File not found.");
                return "";
            }

            FileInfo fileInfo = new FileInfo(filePath);

            string metadata = $"File Name: {fileInfo.Name}{Environment.NewLine}" +
                              $"Full Path: {fileInfo.FullName}{Environment.NewLine}" +
                              $"Size: {fileInfo.Length} bytes{Environment.NewLine}" +
                              $"Created: {fileInfo.CreationTime}{Environment.NewLine}" +
                              $"Last Modified: {fileInfo.LastWriteTime}{Environment.NewLine}" +
                              $"Last Accessed: {fileInfo.LastAccessTime}{Environment.NewLine}" +
                              $"Attributes: {fileInfo.Attributes}";

            return metadata;
        }

        //Save meta data to a new file with hash name
        public bool SaveMetaDataFile(string MetaData, string DestinationPath, byte[] HashCode)
        {
            if (!File.Exists(DestinationPath))
            {
                Console.WriteLine("File not found.");
                return false;
            }

            string metadataFilePath = DestinationPath + BitConverter.ToUInt32(HashCode, 0) + ".metadata";

            File.WriteAllText(metadataFilePath, MetaData);

            return true;
        }
    }
}
