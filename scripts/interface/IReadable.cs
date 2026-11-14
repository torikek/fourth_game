/// <summary>Предмет, поддерживающий открытие и закрытие текста.</summary>
public interface IReadable : IInteractable
{
    void OpenText();
    void CloseText();
}
