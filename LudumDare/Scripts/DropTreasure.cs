using Godot;

public partial class DropTreasure : Area3D
{
    // References
    private PlayerController player;
    private GameManager gameManager;

    public override void _Ready()
    {
        // Find references in scene
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
    }

    /// <summary>
    /// Handles droping a treasure in this area.
    /// </summary>
    /// <param name="treasure">The treasure chest to collect.</param>
    public void RecieveTreasure(Node3D treasure)
    {
        if (gameManager != null)
            gameManager.AddTreasure(treasure);
        else
            GD.PrintErr("DropTreasure: gameManager is NULL Fix this!");
    }

    public override void _PhysicsProcess(double delta)
    {
        // Check if player is touching this exit area
        player.SetTouchingExit(OverlapsBody(player));
    }
}
