using Azure;
using Azure.Data.Tables;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Implementation;

public class TableStorageDataSource: IRepoDataSource
{
    private readonly ILogger<ServiceBusReprocessor> _logger;
    private readonly TableClient _tableClient;
    public TableStorageDataSource(ILogger<ServiceBusReprocessor> logger, IConfiguration configuration)
    {
        _logger = logger;
        _tableClient = new TableClient(configuration.GetConnectionString("TableStorage"), "Repos");
        _tableClient.CreateIfNotExists();
    }
    public async Task<List<Repo>> GetAllRepos()
    {
        var output = new List<Repo>();
        await foreach (var item in _tableClient.QueryAsync<RepoTableRecord>())
        {
            output.Add(item);
        }
        return output;
    }

    public async Task RecordDelete(Repo repo)
    {
        await _tableClient.DeleteEntityAsync((RepoTableRecord)repo);
    }

    public async Task RecordUpdateOrCreate(Repo repo)
    {
        await _tableClient.UpsertEntityAsync((RepoTableRecord)repo);
    }
}

public class RepoTableRecord : ITableEntity
{
    public RepoTableRecord()
    {
    }
    public RepoTableRecord(Repo repo)
    {
        Repo = repo.RepoName;
        User = repo.User;
        RepoId = repo.RepoId;
        PartitionKey = "partition";
        RowKey = repo.RepoId.ToString();
    }
    public string PartitionKey { get; set; }
    public string RowKey { get; set; }
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public int RepoId { get; set; }
    public string User { get; set; }
    public string Repo { get; set; }

    public static implicit operator Repo(RepoTableRecord repo)
    {
        return new Repo()
        {
            RepoId = repo.RepoId,
            RepoName = repo.Repo,
            User = repo.User
        };
    }

    public static implicit operator RepoTableRecord(Repo repo)
    {
        return new RepoTableRecord(repo);
    }
}