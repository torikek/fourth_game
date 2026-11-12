using Godot;
using System;

public interface IInteractable 
{
    float Health;
    void ReceiveDamage(){}
    void Die() {}
}
