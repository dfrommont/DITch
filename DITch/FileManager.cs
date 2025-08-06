using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DITch
{
    internal class FileManager
    {

        //Compress
        //Decompress
        //Move file
        //Copy file
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
