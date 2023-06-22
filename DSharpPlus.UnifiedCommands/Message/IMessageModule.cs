using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Message;

public interface IMessageModule
{
    public DiscordClient Client { get; set; }
    public DiscordMessage Message { get; set; }
    public MessageActionDelegate MessageActionDelegate { get; set; }
}