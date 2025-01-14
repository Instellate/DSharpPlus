using System.Threading.Tasks;
using DSharpPlus.Entities;
using DSharpPlus.UnifiedCommands;
using DSharpPlus.UnifiedCommands.Message.Errors;
using Remora.Results;
using Microsoft.Extensions.Logging;

namespace DSharpPlus.Test;

public class UnifiedCommandsErrorHandler : IErrorHandler
{
    public Task HandleInteractionErrorAsync(IResultError error, DiscordInteraction interaction, DiscordClient client)
    {
        client.Logger.LogError("Failed interaction with error: {Error}", error);
        return Task.CompletedTask;
    }

    public Task HandleMessageErrorAsync(IResultError error, DiscordMessage message, DiscordClient client)
    {
        client.Logger.LogError("Failed message command with error: {Error}", error);


        return error is FailedConversionError e
            ? message.Channel.SendMessageAsync($"Option `{e.Name}` is invalid.")
            : Task.CompletedTask;
    }
}
