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

    public SnapshotTree(string commitMessage, byte[] commitHash, byte[] previousCommitHash)
    {
        Root = new SnapshotTreeNode(new CommitSnapshot(commitMessage, commitHash, previousCommitHash));
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
    public SnapshotTreeNode? FindParent(SnapshotTreeNode current, SnapshotTreeNode target)
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
    }

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

    public static void Main(string[] args)
    {
        CommitSnapshot commit1 = new CommitSnapshot("Initial commit", new byte[] { 0x11 }, new byte[] { 0x00 });
        Console.WriteLine($"Commit details: {commit1.commitMessage}, {commit1.commitHash}, Previous commit: {commit1.previousCommitHash}");
        CommitSnapshot commit2 = new CommitSnapshot("Commit 2", new byte[] { 0x22 }, new byte[] { 0x11 });
        CommitSnapshot commit3 = new CommitSnapshot("Commit 3", new byte[] { 0x33 }, new byte[] { 0x22 });
        CommitSnapshot commit4 = new CommitSnapshot("Commit 4", new byte[] { 0x44 }, new byte[] { 0x33 });

        SnapshotTreeNode node1 = new SnapshotTreeNode(commit1);
        SnapshotTreeNode node2 = new SnapshotTreeNode(commit2);
        SnapshotTreeNode node3 = new SnapshotTreeNode(commit3);
        SnapshotTreeNode node4 = new SnapshotTreeNode(commit4);

        node1.AddChildren(new List<SnapshotTreeNode> { node2, node3 });
        node2.SetParent(node1);
        node2.AddChild(node4);
        node3.SetParent(node2);
        node4.SetParent(node2);

        SnapshotTree tree = new SnapshotTree(node1);

        SnapshotTreeNode test1 = tree.FindNode(node1, new CommitSnapshot("Commit 4"));
    }
}