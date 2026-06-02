using Godot;

public partial class DropTreasure : Area3D
{
    [Export] private PackedScene treasureScene;
    [Export] private MeshInstance3D floorMeshNode;

    // References
    private PlayerController player;
    private GameManager gameManager;
    private int treasurePlaced;

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
        treasurePlaced++;
        if (gameManager != null)
            gameManager.AddTreasure(treasure);
        else
            GD.PrintErr("DropTreasure: gameManager is NULL Fix this!");

        // Visually place treasure on floor
        Node3D treasureNode = treasureScene.Instantiate<Node3D>();
        treasureNode.Name = "Treasure " + treasurePlaced;
        floorMeshNode.AddChild(treasureNode);
        Vector2 floorSize = ((PlaneMesh)floorMeshNode.Mesh).Size * 0.5f;
        // Reduce size so treasure isn't off floor
        floorSize.X -= 0.5f;
        floorSize.Y -= 0.5f;
        treasureNode.Position += new Vector3((float)GD.RandRange(-floorSize.X, floorSize.X), 0, (float)GD.RandRange(-floorSize.Y, floorSize.Y));
    }

    public override void _PhysicsProcess(double delta)
    {
        // Check if player is touching this exit area
        player.SetTouchingExit(OverlapsBody(player));
    }
}
