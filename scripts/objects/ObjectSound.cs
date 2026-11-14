using Godot;

/// <summary>воспроизводит объёмный звук независимо от времени жизни предмета</summary>
public static class ObjectSound
{
    // выбираем звук удара по предмету
    public static void Hit(Node3D source) => Play(source, "res://resources/audio/hitting.wav");
    // выбираем звук разрушения предмета
    public static void Break(Node3D source) => Play(source, "res://resources/audio/breaking.wav");

    // загружаем аудио и создаём отдельный источник объёмного звука
    private static void Play(Node3D source, string path)
    {
        var stream = GD.Load<AudioStream>(path);
        if (stream == null) return;

        var audio = new AudioStreamPlayer3D
        {
            Name = "ObjectSound",
            Stream = stream,
            VolumeDb = -5f,
            MaxDistance = 15f
        };
        // оставляем звук в сцене, чтобы удаление предмета не прервало воспроизведение
        source.GetTree().CurrentScene.AddChild(audio);
        audio.GlobalPosition = source.GlobalPosition;
        // освобождаем источник после окончания воспроизведения
        audio.Finished += () => audio.QueueFree();
        audio.Play();
    }
}
