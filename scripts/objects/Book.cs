using Godot;

/// <summary>Книга показывает собственный текст и остаётся в мире.</summary>
public partial class Book : StaticBody3D, IReadable
{
    [Export(PropertyHint.MultilineText)]
    public string Text { get; set; } = "Текст книги";

    public void Interact() => OpenText();

    public void OpenText()
    {
        if (GetTree().GetFirstNodeInGroup("reading_host") is IReadingHost host)
            host.ShowText(this, Text);
    }

    public void CloseText()
    {
        // Книгу можно читать повторно.
    }
}