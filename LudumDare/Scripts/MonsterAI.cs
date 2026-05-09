using Godot;

public partial class MonsterAI : CharacterBody3D
{
    [ExportCategory("Monster Stats")]
    [Export] private float chaseSpeed = 150f;
    [Export] private float wanderSpeed = 100f;
    [Export] private float attackDistance = 10f;
    [Export] private AudioStreamPlayer3D myAudio;

    private NavigationAgent3D navAgent;
    private CharacterBody3D player;
    private LevelGenerate levelGenerate;
    private GameManager gameManager;

    public override void _Ready()
    {
        // Setup References
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        levelGenerate = (LevelGenerate)GetTree().GetFirstNodeInGroup("LevelGenerate");
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");

        GlobalPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Y); // Force a Y of 0 when spawning
    }

    public override void _PhysicsProcess(double delta)
    {
        // Check if the monster finished it's last path
        if (navAgent.IsNavigationFinished())
        {
            // Wander to a new point
            navAgent.TargetPosition = levelGenerate.GetRandomPointOnNavmesh();
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }

        // Check if player is in range
        if(GlobalPosition.DistanceTo(player.GlobalPosition) < attackDistance)
        {
            // Get the path to the player
            navAgent.TargetPosition = player.GlobalPosition;
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            // Set the Velocity to follow the path
            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * chaseSpeed * (float)delta;
        }
        else
        {
            // Continue moving toward the next wander path point
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            // Set the Velocity to follow the path
            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * wanderSpeed * (float)delta;
        }

        MoveAndSlide();

        // Check if touching the player
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
