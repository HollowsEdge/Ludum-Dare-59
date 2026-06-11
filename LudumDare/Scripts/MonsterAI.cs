using Godot;

public partial class MonsterAI : CharacterBody3D
{
    [ExportCategory("Monster Stats")]
    [Export] private float chaseSpeed = 150f;
    [Export] private float wanderSpeed = 100f;
    [Export] private float wanderRadiusNearPlayer = 50f;
    [Export] private float attackDistance = 10f;
    [Export] private float sawPlayerCooldown = 3f;
    [Export] private AudioStreamPlayer3D myAudio;
    [Export] private Node3D visionRaycaseStartPos;

    private NavigationAgent3D navAgent;
    private CharacterBody3D player;
    private LevelGenerate levelGenerate;
    private GameManager gameManager;
    private float currSawPlayerCooldown;

    public bool freezeAI = false;


    public override void _Ready()
    {
        // Setup References
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
        levelGenerate = (LevelGenerate)GetTree().GetFirstNodeInGroup("LevelGenerate");
        gameManager = (GameManager)GetTree().GetFirstNodeInGroup("GameManager");
        navAgent = GetNode<NavigationAgent3D>("NavigationAgent3D");
        GD.Print("dist 2 player " + GlobalPosition.DistanceTo(player.GlobalPosition));
        GlobalPosition = new Vector3(GlobalPosition.X, 0, GlobalPosition.Y); // Force a Y of 0 when spawning
    }

    public override void _Process(double delta)
    {
        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Check if monster is frozen
        if (freezeAI) return;

        if (currSawPlayerCooldown > 0)
            currSawPlayerCooldown -= (float)delta;
    }

    public override void _PhysicsProcess(double delta)
    {
        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Check if monster is frozen
        if (freezeAI) return;

        // Check if the monster finished it's last path
        if (navAgent.IsNavigationFinished())
        {
            // Wander to a new point
            navAgent.TargetPosition = levelGenerate.GetRandomPointOnNavmeshNearPlayer(wanderRadiusNearPlayer);
            Velocity = Vector3.Zero;
            MoveAndSlide();
            return;
        }


        // Raycast with offset
        var start = visionRaycaseStartPos.GlobalPosition;
        var end = start + start.DirectionTo(player.GlobalPosition + Vector3.Up) * attackDistance;
        var query = PhysicsRayQueryParameters3D.Create(start, end, 0b00000000_00000000_00000000_00000011);
        var result = GetWorld3D().DirectSpaceState.IntersectRay(query);

        // Check if player is in range
        if(result.TryGetValue("collider", out Variant hitCol))
        {
            if (((Node3D)hitCol).IsInGroup("Player"))
            {
                currSawPlayerCooldown = sawPlayerCooldown;
            }
        }

        if(currSawPlayerCooldown > 0)
        {
            // Get the path to the player
            navAgent.TargetPosition = player.GlobalPosition;
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            // Set the Velocity to follow the path
            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * chaseSpeed;
        }
        else
        {
            // Continue moving toward the next wander path point
            Vector3 nextPathPoint = navAgent.GetNextPathPosition();
            nextPathPoint.Y = 0;

            // Set the Velocity to follow the path
            Vector3 dir = GlobalPosition.DirectionTo(nextPathPoint);
            Velocity = dir * wanderSpeed;
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
