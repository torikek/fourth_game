using Godot;
using System.Collections.Generic;

/// создаёт разлёт осколков разрушенного предмета и автоматически удаляет эффект
public partial class DestructionEffect : Node3D
{
    // для каждого осколка храним геометрию, скорость перемещения и вращения
    private readonly List<(MeshInstance3D Mesh, Vector3 Velocity, Vector3 Spin)> _shards = new();
    private float _age;
    // ограничиваем время жизни эффекта в секундах
    private const float Lifetime = 0.85f;

    // создаём отдельный эффект в сцене до удаления исходного предмета
    public static void Spawn(Node3D source)
    {
        var meshes = new List<MeshInstance3D>();
        CollectMeshes(source, meshes);
        if (meshes.Count == 0) return;

        var effect = new DestructionEffect { Name = "DestructionEffect" };
        source.GetTree().CurrentScene.AddChild(effect);
        // выбираем точки в геометрии предмета, чтобы осколки появлялись на месте его частей
        for (int i = 0; i < 32; i++)
        {
            var original = meshes[i % meshes.Count];
            var bounds = original.GetAabb();
            // выбираем случайную точку внутри локальных границ исходной геометрии
            var local = bounds.Position + new Vector3(
                GD.Randf() * bounds.Size.X, GD.Randf() * bounds.Size.Y, GD.Randf() * bounds.Size.Z);
            float size = (float)GD.RandRange(0.025, 0.065);
            var shard = new MeshInstance3D
            {
                Mesh = new BoxMesh { Size = new Vector3(size, size * 0.55f, size * 1.5f) },
                MaterialOverride = original.MaterialOverride ?? original.GetActiveMaterial(0)
            };
            // переносим точку в мировые координаты и задаём случайную ориентацию
            effect.AddChild(shard);
            shard.GlobalPosition = original.ToGlobal(local);
            shard.Rotation = new Vector3(GD.Randf(), GD.Randf(), GD.Randf()) * Mathf.Tau;
            float angle = GD.Randf() * Mathf.Tau;
            float speed = (float)GD.RandRange(0.7, 1.8);
            // разбрасываем осколки вверх и в стороны со случайным вращением
            effect._shards.Add((shard,
                new Vector3(Mathf.Cos(angle) * speed, (float)GD.RandRange(1.0, 2.5), Mathf.Sin(angle) * speed),
                new Vector3(GD.Randf() - 0.5f, GD.Randf() - 0.5f, GD.Randf() - 0.5f) * 14f));
        }
    }

    // рекурсивно собираем видимую геометрию предмета и его дочерних узлов
    private static void CollectMeshes(Node node, List<MeshInstance3D> meshes)
    {
        if (node is MeshInstance3D mesh && mesh.Mesh != null && mesh.IsVisibleInTree())
            meshes.Add(mesh);
        foreach (Node child in node.GetChildren()) CollectMeshes(child, meshes);
    }

    // обновляем движение осколков и удаляем эффект по истечении времени жизни
    public override void _Process(double delta)
    {
        float dt = (float)delta;
        _age += dt;
        if (_age >= Lifetime)
        {
            QueueFree();
            return;
        }
        for (int i = 0; i < _shards.Count; i++)
        {
            var shard = _shards[i];
            // имитируем падение под действием гравитации без физических тел
            shard.Velocity += Vector3.Down * 9.8f * dt;
            shard.Mesh.Position += shard.Velocity * dt;
            shard.Mesh.Rotation += shard.Spin * dt;
            // плавно уменьшаем осколки в конце времени жизни
            shard.Mesh.Scale = Vector3.One * Mathf.Clamp((Lifetime - _age) / 0.25f, 0.001f, 1f);
            _shards[i] = shard;
        }
    }
}
