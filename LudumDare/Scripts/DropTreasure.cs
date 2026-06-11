using Godot;
using System.Threading.Tasks;

public partial class DropTreasure : Area3D
{
    [Export] private PackedScene treasureScene;
    [Export] private MeshInstance3D floorMeshNode;
    [Export] private AnimationPlayer animationPlayer;

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

    /// <summary>
    /// Play the animation for the player exiting the cave
    /// </summary>
    public async Task PlayExitAnimation()
    {
        // setup player
        await player.SetupExitAnim();

        // Enable exit animation camera
        Camera3D cam = GetNode<Camera3D>("../Camera3D");
        cam.Show();
        cam.Current = true;

        // Play animation
        animationPlayer.Play("CameraExit");
        await ToSignal(GetTree().CreateTimer(animationPlayer.CurrentAnimationLength + 0.1f), SceneTreeTimer.SignalName.Timeout);
    }
}
