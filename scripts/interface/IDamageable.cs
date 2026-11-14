/// <summary>Предмет, который получает урон и может быть уничтожен.</summary>
public interface IDamageable : IInteractable
{
    void ReceiveDamage(float damage);
    void Die();
}
