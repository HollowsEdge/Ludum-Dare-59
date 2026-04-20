using Godot;
using System.Collections.Generic;

public partial class ScannerTool : Node3D
{
    [ExportCategory("Gameplay")]
    [Export] private float timeBtwScans = 2;
    [Export] private float scanFadeTime = 1.5f;

    [ExportCategory("References")]
    [Export] private AudioStreamPlayer3D audioButtonBeep;


    [ExportCategory("Grid")]
    [Export] private int gridY = 50;
    [Export] private int gridX = 50;
    [Export] private int spacing = 2;
    [Export] private float scanTime = 2;
    [Export] private Mesh visualMesh;
    [Export] private Color defaultColor = new(1, 1, 1);
    [Export] private Color monsterColor = new(1, 0, 0);
    [Export] private Color treasureColor = new(0, 1, 0);

    [ExportCategory("Debug")]
    [Export] private bool debugRaycastPoints = false;
    [Export] private Material sphereMat;

    private List<Node> previousPointMeshes = new();
    private MultiMeshInstance3D multiMeshInstance3D = new();
    private MultiMesh multimesh = new();
    private Label3D distText;
    private Label3D treasureText;
    private GameManager gameManager;
    private float currTimeBtwScans = 0;

    public override void _Ready()
    {
        GetTree().Root.CallDeferred("add_child", multiMeshInstance3D);
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
        multiMeshInstance3D.Multimesh = multimesh;
        multiMeshInstance3D.CastShadow = GeometryInstance3D.ShadowCastingSetting.Off;
        multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multimesh.Mesh = visualMesh;
        multimesh.UseColors = true;
        distText = GetNode<Label3D>("DistanceText");
        treasureText = GetNode<Label3D>("TreasureText");
    }

    public override void _Process(double delta)
    {
        if(currTimeBtwScans > 0)
            currTimeBtwScans -= (float)delta;

        float closestChest = gameManager.GetClosestChestDist();

        distText.Text = closestChest < 0 ? "None" : Mathf.FloorToInt(closestChest) + "m";
        treasureText.Text = (gameManager.GetTotalChests() - gameManager.GetChestsCount()) + "  /  " + gameManager.GetTotalChests();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsActionPressed("scan") && currTimeBtwScans <= 0) // TODO: Switch to mouse click
        {
            audioButtonBeep.Play();
            currTimeBtwScans = timeBtwScans;
            multimesh.InstanceCount = gridX * gridY;
            if (debugRaycastPoints)
            {
                foreach (var item in previousPointMeshes)
                    item.QueueFree();
                previousPointMeshes.Clear();
            }
            int currIndex = 0;
            for (int x = 0; x < gridX; x++)
            {
                for (int y = 0; y < gridY; y++)
                {
                    var spaceState = GetWorld3D().DirectSpaceState;
                    var cam = GetNode<Camera3D>("../Camera3D");
                    var mousePos = GetViewport().GetMousePosition();

                    int offsetX = (x - gridX / 2) * spacing;
                    int offsetY = (y - gridY / 2) * spacing;

                    var origin = cam.ProjectRayOrigin(mousePos);
                    var end = origin + cam.ProjectRayNormal(mousePos + new Vector2(offsetX, offsetY)) * 10000;
                    var query = PhysicsRayQueryParameters3D.Create(origin, end, 0b00000000_00000000_00000000_00001101);
                    var result = spaceState.IntersectRay(query);

                    Color newColor = defaultColor;

                    if (((Node3D)result["collider"]).IsInGroup("Monster"))
                        newColor = monsterColor;

                    if (((Node3D)result["collider"]).IsInGroup("Treasure"))
                        newColor = treasureColor;

                    multimesh.SetInstanceColor(currIndex, newColor);
                    multimesh.SetInstanceTransform(currIndex, new Transform3D(Basis.Identity, (Vector3)result["position"]));

                    if (debugRaycastPoints)
                    {
                        SphereMesh point = new()
                        {
                            Radius = .02f,
                            Height = .02f * 2
                        };

                        MeshInstance3D mesh = new()
                        {
                            Mesh = point,
                            Layers = 0b00000000_00000000_10000000_00000000,
                            MaterialOverride = sphereMat
                        };
                        previousPointMeshes.Add(mesh);
                        GetTree().Root.AddChild(mesh);
                        mesh.GlobalPosition = (Vector3)result["position"];
                    }
                    
                    currIndex++;
                }

            }

            //GD.Print(previousPointMeshes.Count);
        }
        
    }
}
