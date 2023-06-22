using Microsoft.Extensions.DependencyInjection;

namespace DSharpPlus.UnifiedCommands.Message.Internals;

public class MessageModuleData
{
    public ObjectFactory Factory { get; set; } = null!;
}
