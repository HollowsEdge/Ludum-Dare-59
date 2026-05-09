using Godot;

public partial class MonsterGrabWall : CollisionShape3D
{
    [ExportCategory("References")]
    [Export] private Node3D grabHolder;
    [Export] private Mesh grabMesh;

    [ExportCategory("Stats")]
    [Export] private float grabDistance;
    [Export] private float rotSpeed = 10f;

    public override void _Ready()
    {
        // Create all grab meshes and set as children
        foreach (Node raycast in GetChildren())
            grabHolder.AddChild(new MeshInstance3D(){ Mesh = grabMesh });
    }

    public override void _Process(double delta)
    {
        // Slowly rotate for some movement
        Rotation = new Vector3(Rotation.X + Mathf.DegToRad(rotSpeed) * (float)delta, Rotation.Y, Rotation.Z);
    }

    public override void _PhysicsProcess(double delta)
    {
        // Loop through all of the grab meshes
        Godot.Collections.Array<Node> nodes = GetChildren();
        for (int i = 0; i < nodes.Count; i++)
        {
            Node3D currentGrab = grabHolder.GetChild<Node3D>(i);
            if(((RayCast3D)nodes[i]).IsColliding()) // Check if the grab mesh hits anything
            {
                // Find points for grab mesh position
                Vector3 hitPoint = ((RayCast3D)nodes[i]).GetCollisionPoint();
                Vector3 dir = GlobalPosition.DirectionTo(hitPoint);
                Vector3 middlePoint = hitPoint.Lerp(GlobalPosition, .5f);

                // Show the mesh and set the position
                currentGrab.Show();
                currentGrab.GlobalPosition = middlePoint;

                // Rotate to face the correct direction
                Vector3 y_axis = dir.Normalized();
                Vector3 x_axis = y_axis.Cross(Vector3.Forward).Normalized();
                Vector3 z_axis = x_axis.Cross(y_axis).Normalized();
                currentGrab.Basis = new Basis(x_axis, y_axis, z_axis);
            }
            else
            {
                currentGrab.Hide(); // Hide the node if it doesn't hit anything
            }
        }
    }
}
