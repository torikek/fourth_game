/// <summary>Предмет, поддерживающий подбор.</summary>
public interface ICollectible : IInteractable
{
    string ItemId { get; }
    // Выполняет последствия успешного добавления предмета в инвентарь.
    void Collect();
}
