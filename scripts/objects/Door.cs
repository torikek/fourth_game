using Godot;

/// <summary>Дверь открывается одной анимацией и разрушается от урона.</summary>
public partial class Door : Node3D, IDamageable
{
    [Export] public float MaxHealth { get; set; } = 50f;
    [Export] public bool AutoCloseAfterPassage { get; set; }
    [Export] public float AutoCloseDelay { get; set; } = 0.4f;
    [Export] public bool LockAfterAutoClose { get; set; }
    public bool IsLocked { get; private set; }
    private bool _lockOnAnimationFinished;
    private Area3D _passageArea;
    private readonly System.Collections.Generic.Dictionary<Player, float> _entrySides = new();
    private bool _closePending;
    private double _closeCountdown;
    private float _health;
    private bool _isOpen;
    private bool _destroyed;
    private AnimationPlayer _animationPlayer;

    public override void _Ready()
    {
        _health = MaxHealth;
        _animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        _animationPlayer.AnimationFinished += OnAnimationFinished;
        _passageArea = GetNode<Area3D>("PassageArea");
        _passageArea.BodyEntered += OnPassageEntered;
        _passageArea.BodyExited += OnPassageExited;
    }

    public void Interact()
    {
        if (_destroyed || IsLocked || _animationPlayer.IsPlaying()) return;
        _closePending = false;
        _entrySides.Clear();
        if (_isOpen) _animationPlayer.PlayBackwards("Open");
        else _animationPlayer.Play("Open");
        _isOpen = !_isOpen;
        // E can be pressed after the player has already entered the trigger.
        if (_isOpen && AutoCloseAfterPassage)
        {
            foreach (Node3D body in _passageArea.GetOverlappingBodies())
                OnPassageEntered(body);
        }
    }

    private void OnPassageEntered(Node3D body)
    {
        if (!AutoCloseAfterPassage || _destroyed || !_isOpen || body is not Player player) return;
        _entrySides[player] = Mathf.Sign(ToLocal(player.GlobalPosition).Z);
        // Returning to the doorway postpones a pending close.
        _closeCountdown = AutoCloseDelay;
    }

    private void OnPassageExited(Node3D body)
    {
        if (body is not Player player || !_entrySides.Remove(player, out float entrySide)) return;
        if (!AutoCloseAfterPassage || _destroyed || !_isOpen) return;
        float exitSide = Mathf.Sign(ToLocal(player.GlobalPosition).Z);
        if (entrySide * exitSide >= 0) return;
        _closePending = true;
        _closeCountdown = AutoCloseDelay;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!_closePending || _destroyed || !_isOpen || !AutoCloseAfterPassage) return;
        foreach (Node3D body in _passageArea.GetOverlappingBodies())
        {
            if (body is Player) return;
        }
        if (_animationPlayer.IsPlaying()) return;
        _closeCountdown -= delta;
        if (_closeCountdown > 0) return;
        _closePending = false;
        _isOpen = false;
        _lockOnAnimationFinished = LockAfterAutoClose;
        _animationPlayer.PlayBackwards("Open");
    }

    private void OnAnimationFinished(StringName animation)
    {
        if (animation != "Open" || !_lockOnAnimationFinished || _destroyed) return;
        _lockOnAnimationFinished = false;
        IsLocked = true;
    }

    public void ReceiveDamage(float damage)
    {
        if (_destroyed || damage <= 0) return;
        ObjectSound.Hit(this);
        _health -= damage;
        if (_health <= 0) Die();
    }

    public void Die()
    {
        if (_destroyed) return;
        _destroyed = true;
        ObjectSound.Break(this);
        _lockOnAnimationFinished = false;
        IsLocked = false;
        _closePending = false;
        _entrySides.Clear();
        _animationPlayer.Stop();
        // Полотно удаляется вместе с коллизиями; рама остаётся.
        var hinge = GetNode<Node3D>("DoorHinge");
        DestructionEffect.Spawn(hinge);
        hinge.QueueFree();
    }
}
