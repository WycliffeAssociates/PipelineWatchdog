namespace Core;

public interface IReprocessor
{
    public Task SendCreate(Repo repo);
    public Task SendDelete(Repo repo);
}