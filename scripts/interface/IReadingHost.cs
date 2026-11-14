/// <summary>Связь читаемых предметов с игроком.</summary>
public interface IReadingHost
{
    bool ShowText(IReadable source, string text);
    void CollectItem(ICollectible item);
}