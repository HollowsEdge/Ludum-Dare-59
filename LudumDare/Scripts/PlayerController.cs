using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float walkSpeed = 5f;
    [Export] public float sprintSpeed = 5f;
    [Export] public float carryingMulti = .9f;
    [Export] public float sensitivity = .5f;
    [Export] public RayCast3D ray;
    [Export] public Node3D carryingPos;

    private float xRot;
    private Node3D cameraHolder;
    private float currSpeed;

    private Node3D carryingTreasure;
    private bool touchingExit = false;

    private Area3D exitArea;

    private UIManger uIManger;

    public override void _Ready()
    {
        cameraHolder = GetNode<Node3D>("CameraHolder");
        Input.MouseMode = Input.MouseModeEnum.Captured;
        uIManger = (UIManger)GetTree().GetFirstNodeInGroup("UIManager");
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsKeyPressed(Key.Escape))
            uIManger.TogglePaused();

        Vector2 input = Input.GetVector("move_left", "move_right", "move_down", "move_up").Normalized();

        currSpeed = Input.IsActionPressed("sprint") ? sprintSpeed : walkSpeed;

        Vector3 direction = (input.X * Basis.X + input.Y * -Basis.Z).Normalized();

        currSpeed *= carryingTreasure != null ? carryingMulti : 1;

        Velocity = direction * currSpeed * (float)delta;
        MoveAndSlide();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            if (carryingTreasure == null)
            {
                // Pick up
                GodotObject hitObject = ray.GetCollider();
                carryingTreasure = ((Node3D)hitObject).GetParent<Node3D>();
                // Check if it is treasure
                if (carryingTreasure.Name.ToString().Contains("Treasure"))
                {
                    carryingTreasure.GlobalPosition = carryingPos.GlobalPosition;
                    carryingTreasure.Reparent(carryingPos);
                    carryingTreasure.RotationDegrees = Vector3.Zero;
                    GD.Print("Picked up : " + carryingTreasure);
                }
                else
                {
                    carryingTreasure = null;
                }
            }
            else
            {
                // Check if standing in recieve zone
                if (exitArea.OverlapsBody(GetNode<PlayerController>(GetPath())))
                {
                    // Drop
                    carryingTreasure.QueueFree();
                    ((DropTreasure)exitArea).RecieveTreasure();
                    GD.Print("SUCCESS : " + carryingTreasure);
                    carryingTreasure = null;
                }
                else
                {
                    // Drop
                    carryingTreasure.Reparent(GetTree().Root);
                    // TODO: Add some force forward
                    GD.Print("Dropped : " + carryingTreasure);
                    carryingTreasure = null;
                }      
            }
        }

        // Camera
        if (@event is InputEventMouseMotion mouseDelta)
        {
            Vector2 mouseInput = mouseDelta.Relative * sensitivity * 0.01f;

            xRot -= mouseInput.Y;
            xRot = Mathf.Clamp(xRot, -90, 90);

            cameraHolder.RotationDegrees = Vector3.Right * xRot;
            RotationDegrees -= Vector3.Up * mouseInput.X;
        }
    }

    public void SetTouchingExit(bool touching)
    {
        touchingExit = touching;
    }

    public void SetTouchingArea(Area3D area)
    {
        exitArea = area;
    }
}
