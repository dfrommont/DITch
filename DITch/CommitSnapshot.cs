namespace DITch
{
    public class CommitSnapshot
    {
        public string commitMessage;
        public byte[] commitHash;
        public byte[] previousCommitHash;

        public CommitSnapshot(string commitMessage, byte[] commitHash, byte[] previousCommitHash)
        {
            this.commitMessage = commitMessage;
            this.commitHash = commitHash;
            this.previousCommitHash = previousCommitHash;
        }

        public string GetCommitMessage() => commitMessage;
        public void SetCommitMessage(string message) => commitMessage = message;

        public byte[] GetCommitHash() => commitHash;
        public void SetCommitHash(byte[] hash) => commitHash = hash;

        public byte[] GetPreviousCommitHash() => previousCommitHash;
        public void SetPreviousCommitHash(byte[] hash) => previousCommitHash = hash;
    }
}