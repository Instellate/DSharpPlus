using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.CH.Message;
using DSharpPlus.CH.Message.Permission;

namespace DSharpPlus.Test;

[MessageModule]
public class CHCommandModuleTest : MessageCommandModule
{
    private readonly string _str;

    public CHCommandModuleTest(string str)
    {
        _str = str;
    }

    [MessageCommand("test-sync")]
    public IMessageCommandModuleResult TestSync()
    {
        return Reply("Sync works.");
    }

    [MessageCommand("test-async")]
    public async Task<IMessageCommandModuleResult> TestAsync()
    {
        await PostAsync(Reply("Async works"));
        return Empty();
    }

    [MessageCommand("test-arg-opt")]
    public IMessageCommandModuleResult TestArgOpt(string argument, [MessageOption("option", "o")] string? option)
    {
        return Reply(option != null ? $"Argument was {argument} and option was {option}" : $"Argument was {argument} and option wasn't provided.");
    }

    [MessageCommand("test-permissions")]
    [MessagePermission(Permissions.Administrator)]
    public IMessageCommandModuleResult TestPermissions()
    {
        return Reply("You are a admin.");
    }

    [MessageCommand("test-di")]
    public IMessageCommandModuleResult TestDI()
    {
        return Reply($"DI gave me value `{_str}`.");
    }
}