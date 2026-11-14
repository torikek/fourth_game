using Godot;

/// <summary>Записка собирается только после закрытия текста.</summary>
public partial class Note : Area3D, IReadable, ICollectible
{
    [Export] public string ItemId { get; set; } = "note";
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "Текст записки";

    private IReadingHost _host;
    private bool _reading;
    private bool _collected;

    public void Interact() => OpenText();

    public void OpenText()
    {
        if (_collected || _reading) return;
        _host = GetTree().GetFirstNodeInGroup("reading_host") as IReadingHost;
        _reading = _host != null && _host.ShowText(this, Text);
    }

    public void CloseText()
    {
        if (!_reading || _collected) return;
        _reading = false;
        _host.CollectItem(this);
    }

    public void Collect()
    {
        if (_collected) return;
        _collected = true;
        Hide();
        QueueFree();
    }
}