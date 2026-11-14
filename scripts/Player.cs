using Godot;

[GlobalClass]
public partial class Player : CharacterBody3D, IReadingHost
{
    // используем гравитацию проекта и настраиваемые параметры движения
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    [Export]
    private float Speed = 5.0f;
    [Export]
    private float MouseSensitivity = 0.002f;
    [Export]
    // храним идентификаторы предметов для проверки повторного подбора
    private Godot.Collections.Array<string> Inventory = new Godot.Collections.Array<string>();

    [Export]
    private float AttackDamage = 10.0f;

    // сохраняем ссылки на камеру, луч выбора предмета и интерфейс
    private Camera3D _camera;
    private RayCast3D _rayCast;
    private Label _interactionLabel;
    private Node _targetNode;
    private IInteractable _target;
    private ReadingPanel _readingPanel;
    private Label _notesLabel;
    // запоминаем источник текста и состояние чтения
    private IReadable _readingSource;
    private bool _isReading;

    // находим узлы игрока, создаём окно чтения и захватываем курсор
    public override void _Ready()
    {
        _camera = GetNode<Camera3D>("Camera3D");
        _rayCast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        _interactionLabel = GetNode<Label>("CanvasLayer/CameraLabel");
        _notesLabel = GetNode<Label>("CanvasLayer/LabelItemsNumber");
        _readingPanel = GD.Load<PackedScene>("res://scenes/ReadingPanel.tscn").Instantiate<ReadingPanel>();
        AddChild(_readingPanel);
        _readingPanel.CloseRequested += CloseReading;
        // читаемые предметы находят игрока через общую группу
        AddToGroup("reading_host");
        _notesLabel.Text = Inventory.Count.ToString();
        // исключаем самого игрока из проверки лучом
        _rayCast.AddException(this);
        _interactionLabel.Text = "";
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    // применяем гравитацию и перемещение с учётом столкновений
    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        velocity.Y -= gravity * (float)delta;

        // во время чтения отключаем движение, сохраняя действие гравитации
        Vector2 input = _isReading ? Vector2.Zero : Input.GetVector("left", "right", "forward", "backward");
        // переводим направление ввода в систему координат повёрнутого игрока
        Vector3 movementDirection = Transform.Basis * new Vector3(input.X, 0, input.Y);
        velocity.X = movementDirection.X * Speed;
        velocity.Z = movementDirection.Z * Speed;
        
        Velocity = velocity;
        MoveAndSlide();
    }
    // обновляем цель взаимодействия и подсказку каждый кадр
    public override void _Process(double delta)
    {
        UpdateInteractionTarget();
    }

    // коллайдер импортированной модели может быть дочерним узлом предмета
    private void UpdateInteractionTarget()
    {
        _target = null;
        _targetNode = null;
        _interactionLabel.Text = "";
        if (_isReading) return;
        _rayCast.ForceRaycastUpdate();

        // поднимаемся от коллайдера к родителям в поисках доступного взаимодействия
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

    // обрабатываем чтение, обзор мышью, выход, взаимодействие и атаку
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
            // ограничиваем наклон камеры, чтобы не переворачивать обзор
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

        // проверяем луч заново, чтобы действие соответствовало текущему прицелу
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
        // наносим урон только предметам с поддержкой разрушения
        else if (attackPressed && _target is IDamageable damageable)
        {
            damageable.ReceiveDamage(AttackDamage);
        }
    }

    // окно общее, но последствия закрытия определяет читаемый предмет
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

    // возвращаем управление и уведомляем существующий предмет о завершении чтения
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

    // добавляем уникальный предмет, обновляем счётчик и сбрасываем текущую цель
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
