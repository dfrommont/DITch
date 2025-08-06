public class SnapshotTreeNode
{
    public CommitSnapshot Data { get; set; }
    public SnapshotTreeNode? Parent { get; set; }
    public List<SnapshotTreeNode> Children { get; set; }

    public SnapshotTreeNode(CommitSnapshot data)
    {
        Data = data;
        Parent = null;
        Children = new List<SnapshotTreeNode>();
    }

    public SnapshotTreeNode(CommitSnapshot data, List<SnapshotTreeNode> children)
    {
        Data = data;
        Parent = null;
        Children = children;
        foreach (var child in children)
        {
            child.Parent = this;
        }
    }

    public SnapshotTreeNode(CommitSnapshot data, SnapshotTreeNode parent, List<SnapshotTreeNode> children)
    {
        Data = data;
        Parent = parent;
        Children = children;
        foreach (var child in children)
        {
            child.Parent = this;
        }
    }

    public SnapshotTreeNode(CommitSnapshot data, SnapshotTreeNode parent)
    {
        this.Data = data;
        this.Parent = parent;
        this.Children = new List<SnapshotTreeNode>();
    }

    public void AddChild(SnapshotTreeNode child)
    {
        child.Parent = this;
        Children.Add(child);
    }

    public void AddChildren(List<SnapshotTreeNode> children)
    {
        foreach (var child in children)
        {
            child.Parent = this;
            Children.Add(child);
        }
    }

    public bool RemoveChild(SnapshotTreeNode child)
    {
        return Children.Remove(child);
    }

    public void RemoveChildren(List<SnapshotTreeNode> children)
    {
        foreach (var child in children)
        {
            if (Children.Remove(child))
                child.Parent = null;
        }
    }

    public SnapshotTreeNode? GetParent() => Parent;

    public void SetParent(SnapshotTreeNode parent) => Parent = parent;

    // General-purpose child filter
    public List<SnapshotTreeNode> FindChildrenBy(Predicate<CommitSnapshot> match)
    {
        return Children.Where(child => match(child.Data)).ToList();
    }
}