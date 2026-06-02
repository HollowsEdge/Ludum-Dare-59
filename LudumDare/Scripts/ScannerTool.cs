using Godot;
using System.Collections.Generic;

public partial class ScannerTool : Node3D
{
    [ExportCategory("Gameplay")]
    [Export] private float timeBtwScans = 2;
    [Export] private float scanFadeTime = 1.5f;

    [ExportCategory("References")]
    [Export] private AudioStreamPlayer3D audioButtonBeep;
    [Export] private Label3D distText;
    [Export] private Label3D treasureText;
    [Export] private ProgressBar cooldownUIBar;

    [ExportCategory("Grid")]
    [Export] private int gridY = 50;
    [Export] private int gridX = 50;
    [Export] private int spacing = 2;
    [Export] private float scanTime = 2;
    [Export] private Mesh visualMesh;
    [Export] private Color defaultColor = new(1, 1, 1);
    [Export] private Color monsterColor = new(1, 0, 0);
    [Export] private Color treasureColor = new(0, 1, 0);

    // Other
    private MultiMeshInstance3D multiMeshInstance3D = new();
    private MultiMesh multimesh = new();
    private GameManager gameManager;
    private float currTimeBtwScans = 0;

    bool init = false;

    public override async void _Ready()
    {
        // Wait until level is finished loading
        while (LevelLoader.isLoading) await ToSignal(GetTree(), "process_frame");

        // Get the gameManager
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");

        // Add a multimesh to display the dots in the scene
        GetTree().CurrentScene.CallDeferred("add_child", multiMeshInstance3D);
        multiMeshInstance3D.Name = "ScannerMultiMesh";
        multiMeshInstance3D.Multimesh = multimesh;
        multiMeshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multimesh.Mesh = visualMesh;
        multimesh.UseColors = true;
        init = true;
    }

    public override void _Process(double delta)
    {
        // Make sure ready function ran first
        if (!init) return;

        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Decrease scan cooldown
        if (currTimeBtwScans > 0)
            currTimeBtwScans -= (float)delta;

        float closestChest = gameManager.GetClosestChestDist(); // Always find the closest chest

        // Set UI on scanner
        distText.Text = closestChest < 0 ? "None" : Mathf.FloorToInt(closestChest) + "m";
        treasureText.Text = (gameManager.GetTotalChests() - gameManager.GetChestsCount()) + "  /  " + gameManager.GetTotalChests();
        cooldownUIBar.Value = (currTimeBtwScans / timeBtwScans) * 100;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        if (Input.IsActionJustPressed("scan") && currTimeBtwScans <= 0)
        {
            audioButtonBeep.Play();
            currTimeBtwScans = timeBtwScans;
            multimesh.InstanceCount = gridX * gridY;

            int currIndex = 0;
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    // Calculate offset for raycast
                    int offsetX = (x - gridX / 2) * spacing;
                    int offsetY = (y - gridY / 2) * spacing;

                    // Get variables for raycast
                    var spaceState = GetWorld3D().DirectSpaceState;
                    var cam = GetNode<Camera3D>("../../Camera3D");
                    var mousePos = GetViewport().GetMousePosition();

                    // Raycast with offset
                    var origin = cam.ProjectRayOrigin(mousePos);
                    var end = origin + cam.ProjectRayNormal(mousePos + new Vector2(offsetX, offsetY)) * 10000;
                    var query = PhysicsRayQueryParameters3D.Create(origin, end, 0b00000000_00000000_00000000_00001101);
                    var result = spaceState.IntersectRay(query);

                    // Set the color of the current point
                    Color newColor = defaultColor;

                    if (((Node3D)result["collider"]).IsInGroup("Monster"))
                        newColor = monsterColor;

                    if (((Node3D)result["collider"]).IsInGroup("Treasure"))
                        newColor = treasureColor;

                    // Setup the multimesh
                    multimesh.SetInstanceColor(currIndex, newColor);
                    multimesh.SetInstanceTransform(currIndex, new Transform3D(Basis.Identity, (Vector3)result["position"]));
                    
                    currIndex++;
                }

            }
        }
    }
}
