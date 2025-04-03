namespace Core;

public interface IRepoDataSource: IRepoSource
{
    public Task RecordDelete(Repo repo);
    public Task RecordUpdateOrCreate(Repo repo);
}