
namespace DITch
{

    public class DITch
    {
        public static void Main(string[] args)
        {
            FileManager fileManager = new FileManager();
            HashingTool hashingTool = new HashingTool(true);

            string path = "C:\\Users\\david\\Downloads\\DITch Logo (1)\\temp";

            byte[] compressedFile = fileManager.CompressFolder(path);

            byte[] encryptedFile = fileManager.Encrypt(compressedFile, "David");

            UInt32 hashName = BitConverter.ToUInt32(hashingTool.Hash(encryptedFile), 0);

            Console.WriteLine("Attempted to write encrypted file, go check bitch");

            bool success = fileManager.WriteEncryptedFile(encryptedFile, "C:\\Users\\david\\Downloads\\DITch Logo (1)\\temp_1\\" + hashName + ".secure");

            Console.WriteLine("Attempting decryption");

            // ❗ FIX: Use the encrypted data, not the compressed one
            byte[] decryptedFile = fileManager.ReadEncryptedFile("C:\\Users\\david\\Downloads\\DITch Logo (1)\\temp_1\\" + hashName + ".secure");
            byte[] decryptedData = fileManager.Decrypt(decryptedFile, "David");

            // Decompress
            fileManager.DecompressFolder(decryptedData, @"C:\Users\david\Downloads\DITch Logo (1)\temp_2");
        }
    }
}