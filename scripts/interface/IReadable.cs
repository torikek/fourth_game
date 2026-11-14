/// предмет, поддерживающий открытие и закрытие текста
public interface IReadable : IInteractable
{
    // запрашивает показ текста предмета
    void OpenText();
    // обрабатывает завершение чтения
    void CloseText();
}
