namespace Core;

public class Repo
{
    public int RepoId { get; set; }
    public string RepoHtmlUrl { get; set; }
    public string User { get; set; }
    public string RepoName { get; set; }
    public string DefaultBranch { get; set; }
    
    public static implicit operator Repo(WACSMessage message)
    {
        return new Repo
        {
            RepoId = message.RepoId,
            User = message.User,
            RepoName = message.Repo,
            RepoHtmlUrl = message.RepoHtmlUrl,
            DefaultBranch = message.DefaultBranch
        };
    }
    public WACSMessage ToWACSMessage(string eventType, string action)
    {
        return new WACSMessage
        {
            RepoId = RepoId,
            User = User,
            Repo = RepoName,
            RepoHtmlUrl = RepoHtmlUrl,
            DefaultBranch = DefaultBranch,
            EventType = eventType,
            Action = action,
            LatestCommit = new SimplifiedCommit()
        };
    }
}