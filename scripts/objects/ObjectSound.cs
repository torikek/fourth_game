using Godot;

/// <summary>Plays positional sounds independently of the object's lifetime.</summary>
public static class ObjectSound
{
    public static void Hit(Node3D source) => Play(source, "res://resources/audio/hitting.wav");
    public static void Break(Node3D source) => Play(source, "res://resources/audio/breaking.wav");

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
        source.GetTree().CurrentScene.AddChild(audio);
        audio.GlobalPosition = source.GlobalPosition;
        audio.Finished += () => audio.QueueFree();
        audio.Play();
    }
}
