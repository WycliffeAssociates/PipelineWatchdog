namespace Core;

public interface IRepoSource
{
    public Task<List<Repo>> GetAllRepos();
}