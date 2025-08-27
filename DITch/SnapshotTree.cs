namespace DITch
{
    public class SnapshotTree
    {
        public SnapshotTreeNode Root;

        // Constructors
        public SnapshotTree(CommitSnapshot data)
        {
            Root = new SnapshotTreeNode(data);
        }

        public SnapshotTree(CommitSnapshot data, List<SnapshotTreeNode> children)
        {
            Root = new SnapshotTreeNode(data, children);
        }

        public SnapshotTree(CommitSnapshot data, SnapshotTreeNode parent, List<SnapshotTreeNode> children)
        {
            Root = new SnapshotTreeNode(data, parent, children);
        }

        public SnapshotTree(SnapshotTreeNode root)
        {
            Root = root;
        }

        public SnapshotTree(string commitMessage, byte[] commitHash, byte[] previousCommitHash, DateTime dateTime)
        {
            Root = new SnapshotTreeNode(new CommitSnapshot(commitMessage, commitHash, previousCommitHash, dateTime));
        }

        public SnapshotTreeNode GetRoot() => Root;

        // General recursive search
        public SnapshotTreeNode? FindNode(SnapshotTreeNode node, Predicate<CommitSnapshot> match)
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
        public List<SnapshotTreeNode> FindAllNodes(SnapshotTreeNode node, Predicate<CommitSnapshot> match)
        {
            var matches = new List<SnapshotTreeNode>();

            if (match(node.Data))
                matches.Add(node);

            foreach (var child in node.Children)
            {
                matches.AddRange(FindAllNodes(child, match));
            }

            return matches;
        }

        // Shorthand wrapper methods
        public SnapshotTreeNode? FindNodeByMessage(SnapshotTreeNode node, string message)
        {
            return FindNode(node, s => s.commitMessage == message);
        }

        public SnapshotTreeNode? FindNodeByHash(SnapshotTreeNode node, byte[] commitHash)
        {
            return FindNode(node, s => s.commitHash.SequenceEqual(commitHash));
        }

        public SnapshotTreeNode? FindNodeByPreviousHash(SnapshotTreeNode node, byte[] previousHash)
        {
            return FindNode(node, s => s.previousCommitHash.SequenceEqual(previousHash));
        }

        // Find all matching nodes by message/hash
        public List<SnapshotTreeNode> FindAllNodesByMessage(SnapshotTreeNode node, string message)
        {
            return FindAllNodes(node, s => s.commitMessage == message);
        }

        public List<SnapshotTreeNode> FindAllNodesByHash(SnapshotTreeNode node, byte[] commitHash)
        {
            return FindAllNodes(node, s => s.commitHash.SequenceEqual(commitHash));
        }

        public List<SnapshotTreeNode> FindAllNodesByPreviousHash(SnapshotTreeNode node, byte[] previousHash)
        {
            return FindAllNodes(node, s => s.previousCommitHash.SequenceEqual(previousHash));
        }

        // Find parent of given child (searches recursively)
        /**public SnapshotTreeNode? FindParent(SnapshotTreeNode current, SnapshotTreeNode target)
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
        public SnapshotTreeNode? FindNodeWithChildMatching(SnapshotTreeNode current, Predicate<CommitSnapshot> match)
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
    }
}