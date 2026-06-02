using Godot;
using System;

public partial class CameraItemLag : Node3D
{
    [Export] private Camera3D camera;

    [Export] public float SwayStrength = 0.2f;
    [Export] public float ReturnSpeed = 15f;

    private Vector3 swayRotation;
    private Basis lastCameraBasis;

    public override void _Ready()
    {
        lastCameraBasis = camera.GlobalBasis;
    }

    public override void _Process(double delta)
    {
        Basis currentBasis = camera.GlobalBasis;

        // Rotation difference since last frame
        Basis deltaBasis = lastCameraBasis.Inverse() * currentBasis;
        Vector3 deltaEuler = deltaBasis.GetEuler();

        lastCameraBasis = currentBasis;

        // Add sway opposite to camera movement
        swayRotation.X += -deltaEuler.X * SwayStrength;
        swayRotation.Y += -deltaEuler.Y * SwayStrength;

        // Spring back toward zero
        swayRotation = swayRotation.Lerp(Vector3.Zero, (float)delta * ReturnSpeed);

        Rotation = swayRotation;
    }
}
