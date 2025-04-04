using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Implementation;

public class GiteaRepoSource: IRepoSource
{
    private static readonly ActivitySource _activitySource = new(nameof(GiteaRepoSource));
    private readonly HttpClient _client;
    private readonly ILogger<GiteaRepoSource> _logger;
    public GiteaRepoSource(IConfiguration configuration, ILogger<GiteaRepoSource> logger)
    {
        _logger = logger;
         var baseUrl = configuration["WACSUrl"];
            _client = new HttpClient()
            {
                BaseAddress = new Uri(baseUrl + "/api/v1/")
            };
    }
    public async Task<List<Repo>> GetAllRepos()
    {
        using var activity = _activitySource.StartActivity();
        var noMoreToFetch = false;
        var currentPage = 0;
        var perPage = 100;
        var output = new List<Repo>();
        while (!noMoreToFetch)
        {
            try
            {
                var tmp = await _client.GetFromJsonAsync<RepoSearchResult>(
                    $"repos/search?page={currentPage}&limit={perPage}");

                // Loop through and add to channel
                foreach (var repo in tmp.data)
                {
                    output.Add(repo);
                }

                if (tmp.data.Count == 0)
                {
                    noMoreToFetch = true;
                }
                currentPage++;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
            }
        }
        return output;
    }
}

internal class RepoSearchResult
{
    public List<WACSRepo> data { get; set; }
}
public class WACSRepo
{
    public int id { get; set; }
    public string name { get; set; }
    [JsonPropertyName("default_branch")]
    public string DefaultBranch { get; set; }
    [JsonPropertyName("html_url")]
    public string HtmlUrl { get; set; }
    public WACSUser owner { get; set; }

    public static implicit operator Repo(WACSRepo wacsRepo)
    {
        return new Repo()
        {
            RepoId = wacsRepo.id,
            RepoName = wacsRepo.name,
            User = wacsRepo.owner.username,
            DefaultBranch = wacsRepo.DefaultBranch,
            RepoHtmlUrl = wacsRepo.HtmlUrl,
        };
    }
}

public class WACSUser
{
    public string username { get; set; }
}