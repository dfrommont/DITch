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
        public byte[] CompressFolder(string folderPath)
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

        public byte[] CompressFile(string filePath)
        {
            using var memStream = new MemoryStream();
            using (var archive = new ZipArchive(memStream, ZipArchiveMode.Create, leaveOpen: true))
            {
                var entryName = Path.GetRelativePath(filePath, filePath);
                var entry = archive.CreateEntry(entryName);

                using var entryStream = entry.Open();
                using var fileStream = File.OpenRead(filePath);
                fileStream.CopyTo(entryStream);
            }

            memStream.Position = 0;
            return memStream.ToArray();
        }

        //Decompress
        public void DecompressFolder(byte[] zipData, string destinationFolder)
        {
            // Ensure destination directory exists
            Directory.CreateDirectory(destinationFolder);

            using var memoryStream = new MemoryStream(zipData);
            using var archive = new ZipArchive(memoryStream, ZipArchiveMode.Read);

            foreach (var entry in archive.Entries)
            {
                string fullPath = Path.Combine(destinationFolder, entry.FullName);

                // Handle directory entries
                if (string.IsNullOrEmpty(entry.Name))
                {
                    Directory.CreateDirectory(fullPath);
                    continue;
                }

                // Ensure directory for the file exists
                Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

                // Extract file
                using var entryStream = entry.Open();
                using var fileStream = File.Create(fullPath);
                entryStream.CopyTo(fileStream);
            }
        }

        //Encrypt
        public byte[] Encrypt(byte[] data, string password)
        {
            using var aes = Aes.Create();
            aes.Padding = PaddingMode.PKCS7;

            // Generate salt
            byte[] salt = RandomNumberGenerator.GetBytes(16);

            var key = new Rfc2898DeriveBytes(password, salt, 10000, hashingTool.GetMode() ? HashAlgorithmName.SHA256 : HashAlgorithmName.SHA1); // Salt & iterations
            aes.Key = key.GetBytes(32);
            aes.IV = key.GetBytes(16);

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length); // Prepend salt to output

            using var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();

            return ms.ToArray();
        }


        public bool WriteEncryptedFile(byte[] data, string path)
        {
            try
            {
                File.WriteAllBytes(@path, data);
            } catch (Exception e)
            {
                Console.WriteLine($"Error writing encrypted data to file {path}, Error: {e.Message}");
                return false;
            }
            return true;
        }

        public byte[] ReadEncryptedFile(string path)
        {
            try
            {
                return File.ReadAllBytes(@path);
            } catch (Exception e)
            {
                Console.WriteLine($"Error reading data from encrypted file {path}, Error: {e.Message}");
                return [0];
            }
        }

        //Decrypt
        public byte[] Decrypt(byte[] encryptedData, string password)
        {
            try
            {
                using var aes = Aes.Create();
                aes.Padding = PaddingMode.PKCS7;

                using var msInput = new MemoryStream(encryptedData);

                // Read the salt from the beginning
                byte[] salt = new byte[16];
                msInput.Read(salt, 0, salt.Length);

                var key = new Rfc2898DeriveBytes(password, salt, 10000, HashAlgorithmName.SHA256);
                aes.Key = key.GetBytes(32);
                aes.IV = key.GetBytes(16);

                using var cs = new CryptoStream(msInput, aes.CreateDecryptor(), CryptoStreamMode.Read);
                using var msOutput = new MemoryStream();
                cs.CopyTo(msOutput);

                return msOutput.ToArray();
            } catch (Exception e)
            {
                Console.WriteLine($"Failed decrypting data, likely due to incorrect password, Error: {e.Message}");
                return [0];
            }
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
            if (!Directory.Exists(DestinationPath))
            {
                Console.WriteLine("File not found.");
                return false;
            }

            string metadataFilePath = DestinationPath + HashingTool.ToBase64Url(HashCode) + ".metadata";

            File.WriteAllText(metadataFilePath, MetaData);

            return true;
        }

        public string GetDirectoryStructure(string rootPath)
        {
            if (!Directory.Exists(rootPath))
                throw new DirectoryNotFoundException($"The directory '{rootPath}' does not exist.");

            StringBuilder sb = new StringBuilder();
            BuildStructure(rootPath, sb, 0);
            return sb.ToString();
        }

        private void BuildStructure(string path, StringBuilder sb, int depth)
        {
            string indent = new string('\t', depth);
            string dirName = Path.GetFileName(path);
            if (string.IsNullOrEmpty(dirName))
                dirName = path; // In case of root directory (like C:\)

            sb.AppendLine($"{indent}[{dirName}]");

            // List files in the directory
            foreach (var file in Directory.GetFiles(path))
            {
                sb.AppendLine($"{indent}\t{Path.GetFileName(file)}");
            }

            // Recursively list subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                BuildStructure(dir, sb, depth + 1);
            }
        }
    }
}
