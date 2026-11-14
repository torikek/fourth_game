/// связь читаемых предметов с игроком
public interface IReadingHost
{
    // сообщает, удалось ли открыть текст, и сохраняет его источник для закрытия
    bool ShowText(IReadable source, string text);
    // передаёт предмет игроку для добавления в инвентарь
    void CollectItem(ICollectible item);
}
