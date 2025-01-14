namespace DSharpPlus.UnifiedCommands.Message.Internals;

internal class MessageParameterData
{
    public Type Type { get; set; } = null!;
    public Type ConverterType { get; set; } = null!;
    public object Value { get; set; } = string.Empty;
    public string? ShorthandOptionName { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool CanBeNull { get; set; } = false;
    public bool IsPositionalArgument { get; set; } = true;
    public bool HasDefaultValue { get; set; } = false;
    public bool WillConsumeRestOfArguments { get; set; } = false;
}
