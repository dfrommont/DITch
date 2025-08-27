using System.Xml.Linq;
using static System.Net.Mime.MediaTypeNames;

namespace DITch
{
    public class CommitSnapshot
    {
        public string commitMessage;
        public byte[] commitHash;
        public byte[]? previousCommitHash;
        public CommitSnapshotFileTree? commitSnapshotFileTree;
        public DateTime dateTime;

        public CommitSnapshot(string commitMessage, byte[] commitHash, byte[] previousCommitHash, CommitSnapshotFileTree commitSnapshotFileTree, DateTime dateTime)
        {
            this.commitMessage = commitMessage;
            this.commitHash = commitHash;
            this.previousCommitHash = previousCommitHash;
            this.commitSnapshotFileTree = commitSnapshotFileTree;
            this.dateTime = dateTime;
        }

        public CommitSnapshot(string commitMessage, byte[] commitHash, byte[] previousCommitHash, DateTime dateTime)
        {
            this.commitMessage = commitMessage;
            this.commitHash = commitHash;
            this.previousCommitHash = previousCommitHash;
            this.commitSnapshotFileTree = null;
            this.dateTime = dateTime;
        }

        public CommitSnapshot(string commitMessage, byte[] commitHash, DateTime dateTime)
        {
            this.commitMessage = commitMessage;
            this.commitHash = commitHash;
            this.previousCommitHash = null;
            this.commitSnapshotFileTree = null;
            this.dateTime = dateTime;
        }

        public string GetCommitMessage() => commitMessage;
        public void SetCommitMessage(string message) => commitMessage = message;

        public byte[] GetCommitHash() => commitHash;
        public void SetCommitHash(byte[] hash) => commitHash = hash;

        public byte[] GetPreviousCommitHash() => previousCommitHash;
        public void SetPreviousCommitHash(byte[] hash) => previousCommitHash = hash;

        public DateTime GetDateTime() => dateTime;
        public void SetDateTime(DateTime dateTime) => this.dateTime = dateTime;

        public CommitSnapshotFileTree GetCommitSnapshotFileTree() => commitSnapshotFileTree;
        public void SetCommitSnapshotFileTree(CommitSnapshotFileTree commitSnapshotFileTree) => this.commitSnapshotFileTree = commitSnapshotFileTree;

        static string TrimUpTo(string input, string marker)
        {
            int index = input.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
            if (index == -1)
                return input; // Marker not found, return original string

            return input.Substring(index + marker.Length);
        }

        public XElement GenerateXML()
        {
            return new XElement("Snapshot",
                new XElement("commit_message", commitMessage),
                new XElement("commit_hash", HashingTool.ToBase64Url(commitHash)),
                new XElement("previous_commit_hash", previousCommitHash != null ? HashingTool.ToBase64Url(previousCommitHash) : string.Empty),
                new XElement("commit_epoch_timestamp", dateTime),
                new XElement("file_tree", commitSnapshotFileTree.GenerateXML())
                );
        }

        public CommitSnapshotFileTree BuildFileTreeFromFolder(string folderPath, HashingTool hashingTool)
        {
            if (!Directory.Exists(folderPath))
                throw new DirectoryNotFoundException($"Folder not found: {folderPath}");

            // Create root CommitSnapshotFile
            var rootData = new CommitSnapshotFile(
                hashingTool.Hash(System.Text.Encoding.UTF8.GetBytes(folderPath)), // hash from folder path
                new DirectoryInfo(folderPath).Name,
                "Added");

            var rootNode = new CommitSnapshotFileTreeNode(rootData);

            // Recursively build children
            BuildChildren(hashingTool, rootNode, folderPath);

            return new CommitSnapshotFileTree(rootNode);
        }

        private void BuildChildren(HashingTool hashingTool, CommitSnapshotFileTreeNode parentNode, string path)
        {
            // Add files
            foreach (var file in Directory.GetFiles(path))
            {
                var fileData = File.ReadAllBytes(file);
                var fileHash = hashingTool.Hash(fileData);
                var fileNode = new CommitSnapshotFileTreeNode(new CommitSnapshotFile(fileHash, Path.GetFileName(file), "Added"));
                parentNode.AddChild(fileNode);
            }

            // Add subdirectories
            foreach (var dir in Directory.GetDirectories(path))
            {
                var dirHash = hashingTool.Hash(System.Text.Encoding.UTF8.GetBytes(dir));
                var dirNode = new CommitSnapshotFileTreeNode(new CommitSnapshotFile(dirHash, new DirectoryInfo(dir).Name, "Added"));
                parentNode.AddChild(dirNode);

                // Recurse into subdirectory
                BuildChildren(hashingTool, dirNode, dir);
            }
        }

        public List<byte[]> GetAllFileHashes()
        {
            var hashes = new List<byte[]>();
            CollectHashes(commitSnapshotFileTree.Root, hashes);
            return hashes;
        }

        private void CollectHashes(CommitSnapshotFileTreeNode node, List<byte[]> hashes)
        {
            hashes.Add(node.Data.GetFileHash());
            foreach (var child in node.Children)
            {
                CollectHashes(child, hashes);
            }
        }

        public List<string> GetAllFileNames()
        {
            var names = new List<string>();
            CollectNames(commitSnapshotFileTree.Root, names);
            return names;
        }

        private void CollectNames(CommitSnapshotFileTreeNode node, List<string> names)
        {
            names.Add(node.Data.GetFileName());
            foreach (var child in node.Children)
            {
                CollectNames(child, names);
            }
        }

        public static CommitSnapshot SnapshotFromXML(XElement xml)
        {
            // Read commit message
            string commitMessage = xml.Element("commit_message")?.Value ?? string.Empty;

            // Read commit hash (stored as Base64)
            string commitHashString = xml.Element("commit_hash")?.Value ?? string.Empty;
            byte[] commitHash = HashingTool.FromBase64Url(commitHashString);

            // Read previous commit hash (nullable)
            string prevHashString = xml.Element("previous_commit_hash")?.Value;
            byte[]? previousCommitHash = string.IsNullOrEmpty(prevHashString) ? null : HashingTool.FromBase64Url(prevHashString);

            // Read datetime
            string timestampStr = xml.Element("commit_epoch_timestamp")?.Value ?? string.Empty;
            DateTime dateTime = DateTime.Parse(timestampStr);

            // Read file tree (optional)
            XElement fileTreeElement = xml.Element("file_tree")?.Element("File"); // Root file node
            CommitSnapshotFileTree? fileTree = null;
            if (fileTreeElement != null)
            {
                var rootNode = ParseFileTreeNode(fileTreeElement, null);
                fileTree = new CommitSnapshotFileTree(rootNode);
            }

            return new CommitSnapshot(commitMessage, commitHash, previousCommitHash, fileTree, dateTime);
        }

        private static CommitSnapshotFileTreeNode ParseFileTreeNode(XElement element, CommitSnapshotFileTreeNode? parent)
        {
            // Read attributes
            string hashHex = element.Attribute("hash")?.Value ?? "";
            string fileName = element.Attribute("name")?.Value ?? "";

            // Convert hash hex string back to byte[]
            byte[] hashBytes = Enumerable.Range(0, hashHex.Length / 2)
                .Select(i => Convert.ToByte(hashHex.Substring(i * 2, 2), 16))
                .ToArray();

            var node = new CommitSnapshotFileTreeNode(new CommitSnapshotFile(hashBytes, fileName, "Added"), parent);

            // Recursively add children
            foreach (var childElem in element.Elements("File"))
            {
                var childNode = ParseFileTreeNode(childElem, node);
                node.AddChild(childNode);
            }

            return node;
        }

        public bool UnpackCommitNode(CommitSnapshotFileTreeNode commitNode, string path, bool success)
        {
            foreach (CommitSnapshotFileTreeNode child in commitNode.Children)
            {
                UnpackCommitNode(child, path, success);
            }

            try
            {
                byte[] hashCode = commitNode.Data.GetFileHash();
                FileInfo hashFile = new FileInfo(path + "\\..\\.dit\\index\\" + HashingTool.ToBase64Url(hashCode));

                var data = new Dictionary<string, string>();
                foreach (var line in File.ReadAllLines(path + "\\..\\.dit\\index\\" + HashingTool.ToBase64Url(hashCode) + ".metadata"))
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(new[] { ':' }, 2); // split into key and value
                    if (parts.Length == 2)
                    {
                        string key = parts[0].Trim();
                        string value = parts[1].Trim();
                        data[key] = value;
                    }
                }

                // Extract values
                string fileName = data.ContainsKey("File Name") ? data["File Name"] : string.Empty;
                string fullPath = data.ContainsKey("Full Path") ? data["Full Path"] : string.Empty;

                string result = TrimUpTo(fullPath, path);

                File.WriteAllBytes(path + "\\" + result, File.ReadAllBytes(hashFile.FullName));

                success = true;
            }
            catch (Exception ex) {
                Console.WriteLine($"Unpacking commit failed, Error: {ex.Message}");
                success = false;
            }

            return success;
        }
    }
}