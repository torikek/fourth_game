 using Godot;
using System;

[GlobalClass]
public partial class Player : CharacterBody3D
{
    private float gravity = ProjectSettings.GetSetting("physics/3d/default_gravity").AsSingle();
    [Export]
    private float Speed = 5.0f;
    [Export]
    private float MouseSensitivity = 0.002f;
    [Export]
    private Godot.Collections.Array<Node> Inventory = new Godot.Collections.Array<Node>();

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector3 velocity = Velocity;
        velocity.Y -= gravity * (float)delta;

        Vector2 input = Input.GetVector("left", "right", "forward", "backward");
        Vector3 movementDirection = Transform.Basis * new Vector3(input.X, 0, input.Y);
        velocity.X = movementDirection.X * Speed;
        velocity.Z = movementDirection.Z * Speed;
        
        Velocity = velocity;
        MoveAndSlide();
    }

    public override void _Process(double delta)
    {
        // Label label = GetNode<Label>("CanvasLayer/CameraLabel");
        // RayCast3D rayCast = GetNode<RayCast3D>("Camera3D/RayCast3D");
        rayCast.ForceRaycastUpdate();

        label.Text = rayCast.GetCollider() is Node collider
            ? $"Press 'E' to interact with {collider.Name}"
            : "";
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventMouseMotion mouseMotion)
        {
            RotateY(-mouseMotion.Relative.X * MouseSensitivity);

            Camera3D camera = GetNode<Camera3D>("Camera3D");
            camera.RotateX(-mouseMotion.Relative.Y * MouseSensitivity);
            Vector3 cameraRotation = camera.Rotation;
            cameraRotation.X = Mathf.Clamp(cameraRotation.X, Mathf.DegToRad(-70), Mathf.DegToRad(70));
            camera.Rotation = cameraRotation;
        }
        if (@event.IsActionPressed("quit"))
        {
            GetTree().Quit();
        }
    }
}
