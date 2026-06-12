using Godot;

public partial class CameraItemLag : Node3D
{
    [ExportCategory("References")]
    [Export] private Camera3D camera;
    [Export] private PlayerController player;

    [ExportCategory("Sway")]
    [Export] public float SwayStrength = 0.2f;
    [Export] public float ReturnSpeed = 15f;

    [ExportCategory("Bobbing")]
    [Export] public float IdleBobSpeed = 0.5f;
    [Export] public float SprintBobSpeed = 3f;

    [Export] public float IdleBobAmount = 0.01f;
    [Export] public float SprintBobAmount = 0.02f;

    private Vector3 swayRotation;
    private Vector3 basePosition;

    private float bobTimer = 0f;

    private Basis lastCameraBasis;
    private Vector3 currentOffset;

    public override void _Ready()
    {
        lastCameraBasis = camera.GlobalBasis;
        basePosition = Position;
    }

    public override void _Process(double delta)
    {
        ItemSway(delta);
        ItemBob(delta);
    }

    /// <summary>
    /// Handles rotating the item with a delay to the camera direction.
    /// </summary>
    /// <param name="delta">delta time</param>
    private void ItemSway(double delta)
    {
        Basis currentBasis = camera.GlobalBasis;

        Basis deltaBasis = lastCameraBasis.Inverse() * currentBasis;
        Vector3 deltaEuler = deltaBasis.GetEuler();

        lastCameraBasis = currentBasis;

        swayRotation.X += -deltaEuler.X * SwayStrength;
        swayRotation.Y += -deltaEuler.Y * SwayStrength;

        swayRotation = swayRotation.Lerp(Vector3.Zero, (float)delta * ReturnSpeed);

        Rotation = swayRotation;
    }

    /// <summary>
    /// Handles item bobbing depending on player speed
    /// </summary>
    /// <param name="delta">delta time</param>
    private void ItemBob(double delta)
    {
        float speed = new Vector2(player.Velocity.X, player.Velocity.Z).Length();
        float intensity = Mathf.Clamp(speed / player.GetMaxSpeed(), 0f, 1f);

        // Choose bob settings based on movement state
        float bobSpeed = Mathf.Lerp(IdleBobSpeed, SprintBobSpeed, intensity);
        float bobAmount = Mathf.Lerp(IdleBobAmount, SprintBobAmount, intensity);

        if (speed > 0.1f)
            bobTimer += (float)delta * bobSpeed;
        else
            bobTimer += (float)delta * IdleBobSpeed * 0.5f; // 0.5f for making bob slower when completely still

        // Sin wave bob
        float bobX = Mathf.Sin(bobTimer) * bobAmount;
        float bobY = Mathf.Cos(bobTimer * 2f) * bobAmount;

        Vector3 targetPos = new(bobX, bobY, 0);

        currentOffset = currentOffset.Lerp(targetPos, (float)delta * 10f);
        Position = basePosition + currentOffset;
    }
}