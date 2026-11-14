using Godot;

///записка собирается только после закрытия текста
public partial class Note : Area3D, IReadable, ICollectible
{
    // идентификатор определяет уникальность записки в инвентаре
    [Export] public string ItemId { get; set; } = "note";
    // содержимое записки редактируется как многострочный текст
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "Текст записки";

    // сохраняем игрока и защищаемся от повторного чтения или подбора
    private IReadingHost _host;
    private bool _reading;
    private bool _collected;

    // взаимодействие сначала открывает текст записки
    public void Interact() => OpenText();

    // начинаем чтение только если игрок смог открыть окно
    public void OpenText()
    {
        if (_collected || _reading) return;
        _host = GetTree().GetFirstNodeInGroup("reading_host") as IReadingHost;
        _reading = _host != null && _host.ShowText(this, Text);
    }

    // после завершения чтения просим игрока добавить записку в инвентарь
    public void CloseText()
    {
        if (!_reading || _collected) return;
        _reading = false;
        _host.CollectItem(this);
    }

    // успешный подбор скрывает записку и ставит её в очередь на удаление
    public void Collect()
    {
        if (_collected) return;
        _collected = true;
        Hide();
        QueueFree();
    }
}
