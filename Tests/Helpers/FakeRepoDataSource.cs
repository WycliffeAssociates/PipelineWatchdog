using Core;

namespace Tests.Helpers;

public class FakeRepoDataSource: IRepoDataSource
{
    public List<Repo> Repos { get; set; } = new List<Repo>();
    public async Task<List<Repo>> GetAllRepos()
    {
        return await Task.FromResult(Repos);
    }

    public async Task RecordDelete(Repo repo)
    {
        var existingRepo = Repos.FirstOrDefault(r => r.RepoId == repo.RepoId);
        if (existingRepo != null)
        {
            Repos.Remove(existingRepo);
        }
        await Task.CompletedTask;
    }

    public async Task RecordUpdateOrCreate(Repo repo)
    {
        var existingRepo = Repos.FirstOrDefault(r => r.RepoId == repo.RepoId);
        
        if (existingRepo != null)
        {
            // A little hackish but this is easier than updating all fields
            Repos.Remove(existingRepo);
        }

        Repos.Add(repo);
        await Task.CompletedTask;
    }
}