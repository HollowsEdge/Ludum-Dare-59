using Godot;
using System;

public partial class DropTreasure : Area3D
{
    private PlayerController player;
    private GameManager gamemanager;

    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        gamemanager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
    }

    public void RecieveTreasure()
    {
        if (gamemanager != null)
            gamemanager.AddTreasure();
        else
            GD.PrintErr("DropTreasure: gamemanager is NULL Fix this!");
    }

    public override void _PhysicsProcess(double delta)
    {
        player.SetTouchingExit(OverlapsBody(player));
    }
}
