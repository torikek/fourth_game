using Godot;
using System;

/// общее окно текста сообщает игроку о завершении чтения
public partial class ReadingPanel : CanvasLayer
{
    // передаём запрос закрытия игроку, который завершает чтение
    public event Action CloseRequested;

    // связываем кнопку с событием закрытия и скрываем окно до первого чтения
    public override void _Ready()
    {
        GetNode<Button>("Panel/Margin/VBox/Close").Pressed += () => CloseRequested?.Invoke();
        Hide();
    }

    // показываем текст с начала и переводим фокус на кнопку закрытия
    public void ShowText(string text)
    {
        var label = GetNode<RichTextLabel>("Panel/Margin/VBox/Text");
        label.Text = text;
        label.ScrollToLine(0);
        Show();
        GetNode<Button>("Panel/Margin/VBox/Close").GrabFocus();
    }
}
