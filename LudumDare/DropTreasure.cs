using Godot;
using System;

public partial class DropTreasure : Area3D
{
    private PlayerController player;

    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
    }

    public void RecieveTreasure()
    {
        GD.Print("WE GOT A TREASURE!!!!!!");
    }

    public override void _PhysicsProcess(double delta)
    {
        player.SetTouchingExit(OverlapsBody(player));
    }
}
