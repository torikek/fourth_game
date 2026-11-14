/// редмет, который получает урон и может быть уничтожен
public interface IDamageable : IInteractable
{
    // принимает величину урона для обновления состояния предмета
    void ReceiveDamage(float damage);
    // выполняет разрушение предмета
    void Die();
}
