using Godot;

/// книга показывает собственный текст и остаётся в мире
public partial class Book : StaticBody3D, IReadable
{
    // многострочный текст книги задаётся в инспекторе
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "Текст книги";

    // взаимодействие с книгой открывает её текст
    public void Interact() => OpenText();

    // находим игрока через общую группу и просим показать текст
    public void OpenText()
    {
        if (GetTree().GetFirstNodeInGroup("reading_host") is IReadingHost host)
            host.ShowText(this, Text);
    }

    public void CloseText()
    {
        // книгу можно читать повторно
    }
}
