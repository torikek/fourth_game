using Godot;

/// разрушаемый стул
public partial class Chair : StaticBody3D, IDamageable
{
    // начальный запас здоровья задаётся в инспекторе
    [Export] public float MaxHealth { get; set; } = 10f;
    private float _health;

    // при появлении стула устанавливаем полный запас здоровья
    public override void _Ready() => _health = MaxHealth;
    public void Interact()
    {
        // для стула предусмотрена только атака
    }

    // игнорируем недопустимый урон и удаляемый предмет, затем проверяем разрушение
    public void ReceiveDamage(float damage)
    {
        if (damage <= 0 || IsQueuedForDeletion()) return;
        ObjectSound.Hit(this);
        _health -= damage;
        if (_health <= 0) Die();
    }

    // воспроизводим звук и осколки перед отложенным удалением стула
    public void Die()
    {
        if (IsQueuedForDeletion()) return;
        ObjectSound.Break(this);
        DestructionEffect.Spawn(this);
        QueueFree();
    }
}
