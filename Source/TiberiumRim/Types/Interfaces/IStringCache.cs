namespace TR.Interfaces;

public interface IStringCache
{
    public string[] CachedStrings { get; set; }
    public void UpdateString(int index, string newString);
    public string CachedString(int index);
}