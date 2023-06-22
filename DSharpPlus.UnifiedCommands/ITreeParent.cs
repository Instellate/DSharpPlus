namespace DSharpPlus.UnifiedCommands.Internals.Trees;

public interface ITreeParent<T> : ITreeChild<T>
{
    public List<ITreeChild<T>> List { get; set; }
}
