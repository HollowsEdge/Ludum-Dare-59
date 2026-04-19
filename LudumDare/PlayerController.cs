using Godot;
using System;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float walkSpeed = 5f;
    [Export] public float sprintSpeed = 5f;
    [Export] public float sensitivity = .5f;

    private float xRot;
    private Node3D cameraHolder;
    private float currSpeed;

    public override void _Ready()
    {
        cameraHolder = GetNode<Node3D>("CameraHolder");
        Input.MouseMode = Input.MouseModeEnum.Captured;
    }

    public override void _PhysicsProcess(double delta)
    {
        //TMP
        if(Input.IsKeyPressed(Key.Escape))
            Input.MouseMode = Input.MouseModeEnum.Visible;

        Vector2 input = Input.GetVector("move_left", "move_right", "move_down", "move_up").Normalized();

        currSpeed = Input.IsActionPressed("sprint") ? sprintSpeed : walkSpeed;

        Vector3 direction = (input.X * Basis.X + input.Y * -Basis.Z).Normalized();

        Velocity = direction * currSpeed * (float)delta;
        MoveAndSlide();


    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("interact"))
        {
            
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

}
