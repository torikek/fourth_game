using Godot;
using System;

public partial class Door : Node3D//, IDamageble
{
    private float Health = 50f;
    private AnimationPlayer animationPlayer;
    public void ReceiveDamage(float damage)
    {
        Health -=damage;
    }
    public void Die()
    {
        
    }
}
