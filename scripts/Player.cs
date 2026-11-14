using Godot;

[GlobalClass]
public partial class Player : CharacterBody3D, IReadingHost
{
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    [Export]
    private float Speed = 5.0f;
    [Export]
    private float MouseSensitivity = 0.002f;
    [Export]
    private Godot.Collections.Array<string> Inventory = new Godot.Collections.Array<string>();

    [Export]
    private float AttackDamage = 10.0f;

    private Camera3D _camera;
    private RayCast3D _rayCast;
    private Label _interactionLabel;
    private Node _targetNode;
    private IInteractable _target;
    private ReadingPanel _readingPanel;
    private Label _notesLabel;
    private IReadable _readingSource;
    private bool _isReading;

    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _rayCast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        _interactionLabel = GetNode<Label>("CanvasLayer/CameraLabel");
        _notesLabel = GetNode<Label>("CanvasLayer/LabelItemsNumber");
        _readingPanel = GD.Load<PackedScene>("res://scenes/ReadingPanel.tscn").Instantiate<ReadingPanel>();
        AddChild(_readingPanel);
        _readingPanel.CloseRequested += CloseReading;
        AddToGroup("reading_host");
        _notesLabel.Text = Inventory.Count.ToString();
        _rayCast.AddException(this);
        _interactionLabel.Text = "";
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        velocity.Y -= gravity * (float)delta;

        Vector2 input = _isReading ? Vector2.Zero : Input.GetVector("left", "right", "forward", "backward");
        Vector3 movementDirection = Transform.Basis * new Vector3(input.X, 0, input.Y);
        velocity.X = movementDirection.X * Speed;
        velocity.Z = movementDirection.Z * Speed;
        
        Velocity = velocity;
        MoveAndSlide();
    }
    public override void _Process(double delta)
    {
        UpdateInteractionTarget();
    }

    // Коллайдер импортированной модели может быть дочерним узлом предмета.
    private void UpdateInteractionTarget()
    {
        _target = null;
        _targetNode = null;
        _interactionLabel.Text = "";
        if (_isReading) return;
        _rayCast.ForceRaycastUpdate();

        Node candidate = _rayCast.GetCollider() as Node;
        while (GodotObject.IsInstanceValid(candidate))
        {
            if (candidate.IsQueuedForDeletion())
            {
                return;
            }

            if (candidate is IInteractable interactable)
            {
                _target = interactable;
                _targetNode = candidate;
                _interactionLabel.Text = candidate is Door door && door.IsLocked
                    ? "Дверь заперта. ЛКМ — сломать дверь"
                    : $"Press 'E' to interact with {candidate.Name}";
                return;
            }

            candidate = candidate.GetParent();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (_isReading)
        {
            if (@event.IsActionPressed("quit") || @event.IsActionPressed("interact"))
            {
                CloseReading();
                GetViewport().SetInputAsHandled();
            }
            return;
        }
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            _camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            Vector3 cameraRotation = _camera.Rotation;
            cameraRotation.X = Mathf.Clamp(cameraRotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(70));
            _camera.Rotation = cameraRotation;
        }
        if (@event.IsActionPressed("quit"))
        {
            GetTree().Quit();
            return;
        }

        bool interactPressed = @event.IsActionPressed("interact");
        bool attackPressed = @event.IsActionPressed("attack");
        if (!interactPressed && !attackPressed)
        {
            return;
        }

        // Проверяем луч заново, чтобы действие соответствовало текущему прицелу.
        UpdateInteractionTarget();
        if (_target == null || !GodotObject.IsInstanceValid(_targetNode)
            || _targetNode.IsQueuedForDeletion())
        {
            return;
        }

        if (interactPressed)
        {
            _target.Interact();
        }
        else if (attackPressed && _target is IDamageable damageable)
        {
            damageable.ReceiveDamage(AttackDamage);
        }
    }

    // Окно общее, но последствия закрытия определяет читаемый предмет.
    public bool ShowText(IReadable source, string text)
    {
        if (_isReading) return false;
        _readingSource = source;
        _isReading = true;
        _interactionLabel.Text = "";
        _readingPanel.ShowText(text);
        Input.MouseMode = Input.MouseModeEnum.Visible;
        return true;
    }

    private void CloseReading()
    {
        if (!_isReading) return;
        IReadable source = _readingSource;
        _readingSource = null;
        _isReading = false;
        _readingPanel.Hide();
        Input.MouseMode = Input.MouseModeEnum.Captured;
        if (source is GodotObject instance && GodotObject.IsInstanceValid(instance))
            source.CloseText();
    }

    public void CollectItem(ICollectible item)
    {
        if (Inventory.Contains(item.ItemId)) return;
        Inventory.Add(item.ItemId);
        _notesLabel.Text = Inventory.Count.ToString();
        item.Collect();
        _target = null;
        _targetNode = null;
        _interactionLabel.Text = "";
    }
}
