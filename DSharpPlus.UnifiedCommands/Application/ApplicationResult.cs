using DSharpPlus.Entities;

namespace DSharpPlus.UnifiedCommands.Application;

public class ApplicationResult : IApplicationResult
{
    public ApplicationResultType Type { get; set; }
    public List<DiscordEmbed>? Embeds { get; set; }
    public string? Content { get; set; }

    public static implicit operator ApplicationResult(DiscordEmbed embed)
    {
        ApplicationResult result = new();
        result.Embeds ??= new List<DiscordEmbed> { embed };

        return result;
    }

    public static implicit operator ApplicationResult(DiscordEmbedBuilder builder)
    {
        ApplicationResult result = new();
        result.Embeds ??= new List<DiscordEmbed> { builder.Build() };

        return result;
    }

    public static implicit operator ApplicationResult(string content)
    {
        ApplicationResult result = new()
        {
            Content = content
        };

        return result;
    }
}
