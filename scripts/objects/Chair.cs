using Godot;

/// <summary>Разрушаемый стул.</summary>
public partial class Chair : StaticBody3D, IDamageable
{
    [Export] public float MaxHealth { get; set; } = 10f;
    private float _health;

    public override void _Ready() => _health = MaxHealth;
    public void Interact()
    {
        // Для стула предусмотрена только атака.
    }

    public void ReceiveDamage(float damage)
    {
        if (damage <= 0 || IsQueuedForDeletion()) return;
        _health -= damage;
        if (_health <= 0) Die();
    }

    public void Die() => QueueFree();
}