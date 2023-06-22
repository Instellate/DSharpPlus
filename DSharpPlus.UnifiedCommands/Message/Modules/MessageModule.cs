using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message.Modules;

/// <summary>
/// The MessageModule. Abstract class used for building message commands.
/// </summary>
public abstract class MessageModule : IMessageModule
{
    public static readonly IMessageResult EmptyResult = new MessageResult { Type = MessageResultType.Empty };

    public DiscordMessage Message { get; set; } = null!;
    /// <summary>
    /// The latest message that have been received and processed by PostAsync.
    /// </summary>
    public DiscordMessage? NewestMessage { get; internal set; } = null;
    public DiscordClient Client { get; set; } = null!;
    public MessageActionDelegate MessageActionDelegate { get; set; } = null!;

    /// <summary>
    /// Performs a action without needing to return.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected Task PostAsync(IMessageResult result) => MessageActionDelegate(result);

    /// <summary>
    /// Sets a result to reply on action.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <param name="mention">If the result should mention the user.</param>
    /// <returns></returns>
    protected IMessageResult Reply(MessageResult result, bool mention = false)
    {
        result.Type = mention ? MessageResultType.NoMentionReply : MessageResultType.Reply;
        return result;
    }

    /// <summary>
    /// Does a follow up to property of <see cref="MessageModule.NewestMessage">NewestMessage</see>
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected IMessageResult FollowUp(MessageResult result)
    {
        result.Type = MessageResultType.FollowUp;
        return result;
    }

    /// <summary>
    /// Edits a message to set content provided by result.
    /// </summary>
    /// <param name="result">The result.</param>
    /// <returns></returns>
    protected IMessageResult Edit(MessageResult result)
    {
        result.Type = MessageResultType.Edit;
        return result;
    }

    /// <summary>
    /// Creates a MessageResult with no particular task.
    /// </summary>
    /// <returns></returns>
    protected IMessageResult Empty() => EmptyResult;

    /// <summary>
    /// Sends a message.
    /// </summary>
    /// <param name="result"></param>
    /// <returns></returns>
    protected IMessageResult Send(MessageResult result)
    {
        result.Type = MessageResultType.Send;
        return result;
    }
}
