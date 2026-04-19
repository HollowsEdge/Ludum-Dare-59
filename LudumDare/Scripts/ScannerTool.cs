using Godot;
using System.Collections.Generic;

public partial class ScannerTool : Node3D
{
    [Export] private int gridY = 50;
    [Export] private int gridX = 50;
    [Export] private int spacing = 2;

    [Export] private bool debugRaycastPoints = false;

    private List<Node> previousPointMeshes = new();
    private Vector3[,] previousPointPositions;

    public override void _Ready()
    {
        previousPointPositions = new Vector3[gridX, gridY];
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
                    previousPointPositions[x, y] = (Vector3)result["position"];

                    if (debugRaycastPoints)
                    {
                        SphereMesh point = new()
                        {
                            Radius = .01f,
                            Height = .01f * 2
                        };

                        MeshInstance3D mesh = new()
                        {
                            Mesh = point,
                            Layers = 0b00000000_00000000_10000000_00000000
                        };
                        previousPointMeshes.Add(mesh);
                        GetTree().Root.AddChild(mesh);
                        mesh.GlobalPosition = (Vector3)result["position"];
                    }
                }

            }

            GD.Print(previousPointMeshes.Count);
        }
        
    }
}
