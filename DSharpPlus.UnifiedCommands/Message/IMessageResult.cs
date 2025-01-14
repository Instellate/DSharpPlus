using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// The class used for result handling. Will be updated with more options for better extensibility.
/// </summary>
public interface IMessageResult
{
    public MessageResultType Type { get; set; }
    public DiscordMessageBuilder Builder { get; set; }
}
