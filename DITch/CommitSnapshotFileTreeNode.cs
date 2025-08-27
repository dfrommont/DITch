namespace DITch
{
    public class CommitSnapshotFileTreeNode
    {
        public CommitSnapshotFile Data { get; set; }
        public CommitSnapshotFileTreeNode? Parent { get; set; }
        public List<CommitSnapshotFileTreeNode> Children { get; set; }

        public CommitSnapshotFileTreeNode(CommitSnapshotFile data)
        {
            Data = data;
            Parent = null;
            Children = new List<CommitSnapshotFileTreeNode>();
        }

        public CommitSnapshotFileTreeNode(CommitSnapshotFile data, List<CommitSnapshotFileTreeNode> children)
        {
            Data = data;
            Parent = null;
            Children = children;
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }

        public CommitSnapshotFileTreeNode(CommitSnapshotFile data, CommitSnapshotFileTreeNode parent, List<CommitSnapshotFileTreeNode> children)
        {
            Data = data;
            Parent = parent;
            Children = children;
            foreach (var child in children)
            {
                child.Parent = this;
            }
        }

        public CommitSnapshotFileTreeNode(CommitSnapshotFile data, CommitSnapshotFileTreeNode parent)
        {
            this.Data = data;
            this.Parent = parent;
            this.Children = new List<CommitSnapshotFileTreeNode>();
        }

        public void AddChild(CommitSnapshotFileTreeNode child)
        {
            child.Parent = this;
            Children.Add(child);
        }

        public void AddChildren(List<CommitSnapshotFileTreeNode> children)
        {
            foreach (var child in children)
            {
                child.Parent = this;
                Children.Add(child);
            }
        }

        public bool RemoveChild(CommitSnapshotFileTreeNode child)
        {
            return Children.Remove(child);
        }

        public void RemoveChildren(List<CommitSnapshotFileTreeNode> children)
        {
            foreach (var child in children)
            {
                if (Children.Remove(child))
                    child.Parent = null;
            }
        }

        public CommitSnapshotFileTreeNode? GetParent() => Parent;

        public void SetParent(CommitSnapshotFileTreeNode parent) => Parent = parent;

        // General-purpose child filter
        public List<CommitSnapshotFileTreeNode> FindChildrenBy(Predicate<CommitSnapshotFile> match)
        {
            return Children.Where(child => match(child.Data)).ToList();
        }
    }
}
