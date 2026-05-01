using Godot;

public partial class MonsterAI : CharacterBody3D
{
    [Export] private float chaseSpeed = 150f;
    [Export] private float wanderSpeed = 100f;
    [Export] private float attackDistance = 10f;
    [Export] private AudioStreamPlayer3D myAudio;

    private NavigationAgent3D navAgent;
    private CharacterBody3D player;
    private LevelGenerate levelGenerate;
    private GameManager gameManager;
    private float originalVol;

    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        levelGenerate = (LevelGenerate)GetTree().GetFirstNodeInGroup("LevelGenerate");
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        navAgent.TargetPosition = player.GlobalPosition;
        GlobalPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Y);
        originalVol = myAudio.VolumeDb;
    }

    public override void _PhysicsProcess(double delta)
    {
        if (navAgent.IsNavigationFinished())
        {
            navAgent.TargetPosition = levelGenerate.GetRandomPointOnNavmesh();
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        if(GlobalPosition.DistanceTo(player.GlobalPosition) < attackDistance)
        {
            navAgent.TargetPosition = player.GlobalPosition;
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * chaseSpeed * (float)delta;
        }
        else
        {
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * wanderSpeed * (float)delta;
        }

        MoveAndSlide();

        for (int i = 0; i < GetSlideCollisionCount(); i++)
        {
            if (((Node3D)GetSlideCollision(i).GetCollider()).IsInGroup("Player"))
            {
                // LOSE THE GAME
                gameManager.FinishGame(false);
            }
        }
    }
}
