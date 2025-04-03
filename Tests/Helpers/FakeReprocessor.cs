using Core;

namespace Tests.Helpers;

public class FakeReprocessor: IReprocessor
{
    public Action<Repo>? OnSendCreate { get; set; }
    public Action<Repo>? OnSendDelete { get; set; }
    public Task SendCreate(Repo repo)
    {
        OnSendCreate?.Invoke(repo);
        return Task.CompletedTask;
    }

    public Task SendDelete(Repo repo)
    {
        OnSendDelete?.Invoke(repo);
        return Task.CompletedTask;
    }
}