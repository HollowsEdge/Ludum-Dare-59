using Godot;
using System;

public partial class DropTreasure : Area3D
{
    public void RecieveTreasure()
    {
        GD.Print("WE GOT A TREASURE!!!!!!");
    }

    public override void _PhysicsProcess(double delta)
    {
        var bodies = GetOverlappingBodies();
        if()
    }
}
