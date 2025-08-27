using System.Xml.Linq;

namespace DITch
{
    public class CommitSnapshotFileTree
    {
        public CommitSnapshotFileTreeNode Root;

        // Constructors
        public CommitSnapshotFileTree(CommitSnapshotFile data)
        {
            Root = new CommitSnapshotFileTreeNode(data);
        }

        public CommitSnapshotFileTree(CommitSnapshotFile data, List<CommitSnapshotFileTreeNode> children)
        {
            Root = new CommitSnapshotFileTreeNode(data, children);
        }

        public CommitSnapshotFileTree(CommitSnapshotFile data, CommitSnapshotFileTreeNode parent, List<CommitSnapshotFileTreeNode> children)
        {
            Root = new CommitSnapshotFileTreeNode(data, parent, children);
        }

        public CommitSnapshotFileTree(CommitSnapshotFileTreeNode root)
        {
            Root = root;
        }

        public CommitSnapshotFileTree(byte[] fileHash, string fileName, string status)
        {
            Root = new CommitSnapshotFileTreeNode(new CommitSnapshotFile(fileHash, fileName, status));
        }

        public CommitSnapshotFileTreeNode GetRoot() => Root;

        // General recursive search
        public CommitSnapshotFileTreeNode? FindNode(CommitSnapshotFileTreeNode node, Predicate<CommitSnapshotFile> match)
        {
            if (match(node.Data))
                return node;

            foreach (var child in node.Children)
            {
                var result = FindNode(child, match);
                if (result != null)
                    return result;
            }

            return null;
        }

        // Find all matching nodes
        public List<CommitSnapshotFileTreeNode> FindAllNodes(CommitSnapshotFileTreeNode node, Predicate<CommitSnapshotFile> match)
        {
            var matches = new List<CommitSnapshotFileTreeNode>();

            if (match(node.Data))
                matches.Add(node);

            foreach (var child in node.Children)
            {
                matches.AddRange(FindAllNodes(child, match));
            }

            return matches;
        }

        // Shorthand wrapper methods
        public CommitSnapshotFileTreeNode? FindNodeByFileName(CommitSnapshotFileTreeNode node, string fileName)
        {
            return FindNode(node, s => s.GetFileName() == fileName);
        }

        public CommitSnapshotFileTreeNode? FindNodeByFileHash(CommitSnapshotFileTreeNode node, byte[] fileHash)
        {
            return FindNode(node, s => s.GetFileHash().SequenceEqual(fileHash));
        }

        public CommitSnapshotFileTreeNode? FindNodeByStatus(CommitSnapshotFileTreeNode node, string status)
        {
            return FindNode(node, s => s.GetStatus() == status);
        }

        // Find all matching nodes by message/hash
        public List<CommitSnapshotFileTreeNode> FindAllNodesByFileName(CommitSnapshotFileTreeNode node, string fileName)
        {
            return FindAllNodes(node, s => s.GetFileName() == fileName);
        }

        public List<CommitSnapshotFileTreeNode> FindAllNodesByFileHash(CommitSnapshotFileTreeNode node, byte[] fileHash)
        {
            return FindAllNodes(node, s => s.GetFileHash().SequenceEqual(fileHash));
        }

        public List<CommitSnapshotFileTreeNode> FindAllNodesByStatus(CommitSnapshotFileTreeNode node, string status)
        {
            return FindAllNodes(node, s => s.GetStatus() == status);
        }

        // Find parent of given child (searches recursively)
        /**public SnapshotTreeNode? FindParent(CommitSnapshotFileTreeNode current, CommitSnapshotFileTreeNode target)
        {
            foreach (var child in current.Children)
            {
                if (child == target)
                    return current;

                var result = FindParent(child, target);
                if (result != null)
                    return result;
            }

            return null;
        }**/

        // Find node that has a matching child with matching data
        public CommitSnapshotFileTreeNode? FindNodeWithChildMatching(CommitSnapshotFileTreeNode current, Predicate<CommitSnapshotFile> match)
        {
            foreach (var child in current.Children)
            {
                if (match(child.Data))
                    return current;

                var result = FindNodeWithChildMatching(child, match);
                if (result != null)
                    return result;
            }

            return null;
        }

        public XElement GenerateXML()
        {
            return GenerateXMLFromNode(Root);
        }

        private XElement GenerateXMLFromNode(CommitSnapshotFileTreeNode node)
        {
            // Convert byte[] hash into a hex string for readability
            string hashString = BitConverter.ToString(node.Data.GetFileHash()).Replace("-", "").ToLower();

            // Create element with hash as the name (or you could make it an attribute instead)
            XElement element = new XElement("File",
                new XAttribute("hash", hashString),
                new XAttribute("name", node.Data.GetFileName() ?? string.Empty),
                new XAttribute("Status", node.Data.GetStatus() ?? string.Empty));

            // Recursively add children
            foreach (var child in node.Children)
            {
                element.Add(GenerateXMLFromNode(child));
            }

            return element;
        }
    }
}