using System.Diagnostics;
using Core;
using Microsoft.Extensions.Logging;
using Telemetry;

namespace Logic;

public class ReconcileLogic
{
    private readonly IRepoSource _source;
    private readonly IRepoSource _cached;
    private readonly IReprocessor _reprocessor;
    private readonly ILogger _logger;
    private static readonly ActivitySource _activitySource = new(nameof(ReconcileLogic));
    private readonly WatchdogMetrics? _metrics;
    public ReconcileLogic(IRepoSource source, IRepoSource cached,  IReprocessor reprocessor, ILogger logger, WatchdogMetrics? metrics = null)
    {
        _source = source;
        _cached = cached;
        _reprocessor = reprocessor;
        _logger = logger;
        _metrics = metrics;
    }

    public async Task RunAsync()
    {
        using var activity = _activitySource.StartActivity();
        var sourceTask = _source.GetAllRepos();
        var cachedTask = _cached.GetAllRepos();
        
        var cachedRepos = await cachedTask;
        var sourceRepos = await sourceTask;
        
        var reposToDelete = new List<Repo>();
        var reposToCreate = new List<Repo>();
        foreach(var repo in cachedRepos)
        {
            if (!sourceRepos.Any(r => r.RepoId == repo.RepoId))
            {
                reposToDelete.Add(repo);
            }
        }

        foreach (var repo in sourceRepos)
        {
            if (!cachedRepos.Any(r => r.RepoId == repo.RepoId))
            {
                reposToCreate.Add(repo);
            }
        }
        
        _logger.LogInformation("Found {Count} Repos to Delete", reposToDelete.Count);
        _logger.LogInformation("Found {Count} Repos to Create", reposToCreate.Count);
        
        _metrics?.CreatesSent(reposToCreate.Count);
        _metrics?.DeletesSent(reposToDelete.Count);
        foreach (var repo in reposToDelete)
        {
            await _reprocessor.SendDelete(repo);
        }

        foreach (var repo in reposToCreate)
        {
            await _reprocessor.SendCreate(repo);
        }
    }
}