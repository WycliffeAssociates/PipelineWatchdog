using Core;

namespace Tests.Helpers;

public class FakeRepoSource: IRepoSource
{
    public List<Repo> Repos { get; set; } = new List<Repo>();
    public Task<List<Repo>> GetAllRepos()
    {
        return Task.FromResult(Repos);
    }
}