using Core;
using Microsoft.Extensions.Logging;

namespace Logic;

public class ListenerLogic
{
    private readonly IRepoDataSource _repoDataSource;
    private readonly ILogger _logger;
    public ListenerLogic(IRepoDataSource repoDataSource, ILogger logger)
    {
        _repoDataSource = repoDataSource;
        _logger = logger;
    }

    public async Task Handle(WACSMessage message)
    {
        var actionType = GetActionType(message);
        switch (actionType)
        {
            case ActionType.Create:
            case ActionType.Update:
                await _repoDataSource.RecordUpdateOrCreate(message);
                break;
            case ActionType.Delete:
                await _repoDataSource.RecordDelete(message);
                break;
            case ActionType.Unknown:
                // If it is unknown then we just ignore and move on
                break;
            default:
                // This should never get hit but just in case
                throw new ArgumentOutOfRangeException(nameof(actionType), actionType, "Got ourselves into a position we never should be in");
        }
    }

    private ActionType GetActionType(WACSMessage message)
    {
        return message.EventType switch
        {
            "repo" when message.Action == "updated" => ActionType.Update,
            "repo" when message.Action == "created" => ActionType.Create,
            "repo" when message.Action == "deleted" => ActionType.Delete,
            "push" => ActionType.Update,
            _ => ActionType.Unknown
        };
    }

    enum ActionType
    {
        Unknown,
        Create,
        Update,
        Delete
    }
}