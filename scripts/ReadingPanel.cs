using Godot;
using System;

/// <summary>Общее окно текста сообщает игроку о завершении чтения.</summary>
public partial class ReadingPanel : CanvasLayer
{
    public event Action CloseRequested;

    public override void _Ready()
    {
        GetNode<Button>("Panel/Margin/VBox/Close").Pressed += () => CloseRequested?.Invoke();
        Hide();
    }

    public void ShowText(string text)
    {
        var label = GetNode<RichTextLabel>("Panel/Margin/VBox/Text");
        label.Text = text;
        label.ScrollToLine(0);
        Show();
        GetNode<Button>("Panel/Margin/VBox/Close").GrabFocus();
    }
}