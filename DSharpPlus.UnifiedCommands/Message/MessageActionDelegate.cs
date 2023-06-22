using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message;

/// <summary>
/// Delegates that handles processing of a result without needing to return it.
/// </summary>
/// <param name="result">The result that will get converted into a action</param>
/// <returns>A task that represents. Awaiting it might return a null message. The message will only be null if the API endpoint does not return a message</returns>
public delegate Task<DiscordMessage?> MessageActionDelegate(IMessageResult result);