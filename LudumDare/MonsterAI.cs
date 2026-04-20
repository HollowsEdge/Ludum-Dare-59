using Godot;
using System;

public partial class MonsterAI : CharacterBody3D
{
    [Export] private float speed = 150f;

    private NavigationAgent3D navAgent;
    private CharacterBody3D player;

    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        navAgent.TargetPosition = player.GlobalPosition;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (navAgent.IsNavigationFinished())
        {
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        navAgent.TargetPosition = player.GlobalPosition;
        Vector3 nextPathPoint = navAgent.GetNextPathPosition();

        Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
        Velocity = dir * speed * (float)delta;
        MoveAndSlide();
    }
}
