using Godot;
using System.Collections.Generic;

public partial class ScannerTool : Node3D
{
    [Export] private int gridY = 50;
    [Export] private int gridX = 50;
    [Export] private int spacing = 2;
    [Export] private float lineWidth = .1f;
    [Export] private Mesh visualMesh;
    [Export] private Material sphereMat;


    [Export] private bool debugRaycastPoints = false;

    private List<Node> previousPointMeshes = new();
    private (Vector3 position, Vector3 normal)[,] previousPointPositions;

    private MultiMeshInstance3D multiMeshInstance3D = new();
    private MultiMesh multimesh = new();

    public override void _Ready()
    {
        previousPointPositions = new (Vector3 position, Vector3 normal)[gridX, gridY];
        GetTree().Root.CallDeferred("add_child", multiMeshInstance3D);
        multiMeshInstance3D.Multimesh = multimesh;
        multimesh.TransformFormat = MultiMesh.TransformFormatEnum.Transform3D;
        multimesh.Mesh = visualMesh;
        multimesh.InstanceCount = gridX * gridY;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (Input.IsKeyPressed(Key.Q)) // TODO: Switch to mouse click
        {
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
                    var query = PhysicsRayQueryParameters3D.Create(origin, end, 0b00000000_00000000_00000000_00000001);

                    var result = spaceState.IntersectRay(query);
                    previousPointPositions[x, y] = ((Vector3)result["position"], (Vector3)result["normal"]);
                    multimesh.SetInstanceTransform(currIndex, new Transform3D(new Basis(), (Vector3)result["position"]));


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

            GD.Print(previousPointMeshes.Count);
        }
        
    }
}
