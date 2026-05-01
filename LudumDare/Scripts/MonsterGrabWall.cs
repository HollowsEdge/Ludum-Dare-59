using Godot;
using System;
using System.Collections.Generic;

public partial class MonsterGrabWall : CollisionShape3D
{
    [Export] private Node3D grabHolder;
    [Export] private Mesh grabMesh;
    [Export] private float grabDistance;
    [Export] private float rotSpeed = 10f;

    public override void _Ready()
    {
        foreach (Node raycast in GetChildren())
            grabHolder.AddChild(new MeshInstance3D(){ Mesh = grabMesh });
    }

    public override void _Process(double delta)
    {
        Rotation = new Vector3(Rotation.X + Mathf.DegToRad(rotSpeed) * (float)delta, Rotation.Y, Rotation.Z);
    }

    public override void _PhysicsProcess(double delta)
    {
        Godot.Collections.Array<Node> nodes = GetChildren();
        for (int i = 0; i < nodes.Count; i++)
        {
            Node3D currentGrab = grabHolder.GetChild<Node3D>(i);
            if(((RayCast3D)nodes[i]).IsColliding())
            {
                Vector3 hitPoint = ((RayCast3D)nodes[i]).GetCollisionPoint();
                float dist = hitPoint.DistanceTo(GlobalPosition);
                Vector3 dir = GlobalPosition.DirectionTo(hitPoint);
                //Vector3 minGrabPoint = dir * grabDistance;
                Vector3 middlePoint = hitPoint.Lerp(GlobalPosition, .5f);
                currentGrab.Show();
                currentGrab.GlobalPosition = middlePoint;

                Vector3 y_axis = dir.Normalized();
                Vector3 x_axis = y_axis.Cross(Vector3.Forward).Normalized();
                Vector3 z_axis = x_axis.Cross(y_axis).Normalized();
                currentGrab.Basis = new Basis(x_axis, y_axis, z_axis);

                //Mathf.Lerp(, dist);
            }
            else
            {
                currentGrab.Hide();
            }
        }
    }
}
