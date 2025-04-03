using Core;
using Logic;
using Tests.Helpers;

namespace Tests;

public class Tests
{
    private FakeRepoDataSource _repoDataSource;
    private FakeRepoSource _repoSource;
    private FakeReprocessor _reprocessor;
    private FakeLogger _logger = new FakeLogger();
    [SetUp]
    public void Setup()
    {
        _repoDataSource = new FakeRepoDataSource();
        _repoSource = new FakeRepoSource();
        _reprocessor = new FakeReprocessor();
    }

    [Test]
    public async Task TestListener()
    {
        // Test Create
        var message = new WACSMessage()
        {
            EventType = "repo",
            Action = "created",
            RepoId = 1,
            DefaultBranch = "main",
            User = "tony",
            Repo = "friday"
        };
        var logic = new ListenerLogic(_repoDataSource, _logger);
        await logic.Handle(message);
        Assert.That(_repoDataSource.Repos, Has.Count.EqualTo(1));
        Assert.Multiple(() =>
        {
            Assert.That(_repoDataSource.Repos[0].RepoId, Is.EqualTo(message.RepoId));
            Assert.That(_repoDataSource.Repos[0].DefaultBranch, Is.EqualTo(message.DefaultBranch));
            Assert.That(_repoDataSource.Repos[0].User, Is.EqualTo(message.User));
            Assert.That(_repoDataSource.Repos[0].RepoName, Is.EqualTo(message.Repo));
        });

        // Test Update
        message.Action = "updated";
        message.DefaultBranch = "master";
        
        await logic.Handle(message);
        Assert.That(_repoDataSource.Repos, Has.Count.EqualTo(1));
        Assert.That(_repoDataSource.Repos[0].DefaultBranch, Is.EqualTo(message.DefaultBranch));
        
        // Test Delete
        message.Action = "deleted";
        await logic.Handle(message);
        Assert.That(_repoDataSource.Repos, Is.Empty);
        
        // Test with an unhandled action
        message.EventType = "issue";
        message.Action = "created";
        await logic.Handle(message);
        Assert.That(_repoDataSource.Repos, Is.Empty);
        
        // Test with push
        message.EventType = "push";
        message.Action = "push";
        message.RepoId = 1;
        await logic.Handle(message);
        Assert.That(_repoDataSource.Repos, Has.Count.EqualTo(1));
    }

    [Test]
    public async Task TestReconcileLogicWith()
    {
        var updateHappened = false;
        var deleteHappened = false;
        _reprocessor.OnSendCreate += (repo) =>
        {
            updateHappened = true;
        };
        _reprocessor.OnSendDelete += (repo) =>
        {
            deleteHappened = true;
        };
        var logic = new ReconcileLogic(_repoSource, _repoDataSource, _reprocessor, _logger);
        await logic.RunAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updateHappened, Is.False);
            Assert.That(deleteHappened, Is.False);
        });
        
        // Reset
        updateHappened = false;
        deleteHappened = false;
        
        // Test with creates
        _repoSource.Repos = [new Repo(){RepoId = 1, RepoName = "friday", User = "tony", DefaultBranch = "main"}];
        _repoDataSource.Repos = [];
        await logic.RunAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updateHappened, Is.True);
            Assert.That(deleteHappened, Is.False);
        });
        
        // Reset
        updateHappened = false;
        deleteHappened = false;
        
        //Test with deletes
        _repoDataSource.Repos = [new Repo(){RepoId = 1, RepoName = "friday", User = "tony", DefaultBranch = "main"}];
        _repoSource.Repos = [];
        await logic.RunAsync();
        Assert.Multiple(() =>
        {
            Assert.That(updateHappened, Is.False);
            Assert.That(deleteHappened, Is.True);
        });
    }
}