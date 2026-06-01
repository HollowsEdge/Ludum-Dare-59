using Godot;
using System.Collections.Generic;

public partial class PlayerController : CharacterBody3D
{
    [ExportCategory("Move Speed")]
    [Export] public float walkSpeed = 5f;
    [Export] public float sprintSpeed = 5f;
    [Export] public float carryingMulti = .9f;

    [ExportCategory("Camera")]
    [Export] public float sensitivity = .5f;

    [ExportCategory("References")]
    [Export] public RayCast3D ray;
    [Export] public Node3D carryingPos;
    [Export] public PackedScene footStepScene;
    [Export] public ScannerTool scanner;
    [Export] public OptionsMenu optionsMenu;

    [ExportCategory("Footstep Audio")]
    [Export] private AudioStreamPlayer3D footstepsAudio;
    [Export] public float footstepDelayWalk = .5f;
    [Export] public float footstepDelayRun = .3f;
   
    // Camera and movement
    private float currSpeed;
    private float xRot;
    private Node3D cameraHolder;

    // Footstep Audio
    private float currFootstepAudioDelay;
    private float stepOriginalVol;

    // Treasure and exit tracking
    private bool touchingExit = false;
    private Area3D exitArea;
    private Node3D carryingTreasure;
    private Node3D carryingTreasureCollider1;
    private Node3D carryingTreasureCollider2;
    private List<Node3D> footStepList = new();

    // References
    private LevelGenerate levelGenerate;
    private UIManger uIManger;

    // Other
    private bool init = false;

    public override void _Ready()
    {
        // Find References
        cameraHolder = GetNode<Node3D>("CameraHolder");
        uIManger = (UIManger)GetTree().GetFirstNodeInGroup("UIManager");
        levelGenerate = (LevelGenerate)GetTree().GetFirstNodeInGroup("LevelGenerate");

        // Set original footstep audio
        stepOriginalVol = footstepsAudio.VolumeDb;

        // Set up options update
        optionsMenu.OnOptionsChanged += UpdateOptions;

        // Load data from the save file
        ConfigFile config = new();
        Error err = config.Load("user://settings.cfg");

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
            return;

        // Set saved settings
        sensitivity = (float)config.GetValue("Player", "Sensitivity", sensitivity);
    }

    /// <summary>
    /// Runs when the player is initialized
    /// </summary>
    public void SpawnPlayer()
    {
        // Hide and lock the cursor to the center of the screen
        Input.MouseMode = Input.MouseModeEnum.Captured;
        init = true;
    }

    public override void _Process(double delta)
    {
        // Don't run if not initialized (most likely level not finished generating)
        if (!init) return;

        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Check if carrying treasure
        if (carryingTreasure != null)
        {
            // Place visible path back to exit based on navmesh
            ClearFootsteps();

            foreach (Vector3 point in levelGenerate.GetNavigationPath(GlobalPosition, exitArea.GlobalPosition))
            {
                Node3D newFootStep = footStepScene.Instantiate<Node3D>();
                footStepList.Add(newFootStep);
                GetTree().Root.AddChild(newFootStep);
                newFootStep.GlobalPosition = new(point.X, 0, point.Z);
            }
        }

        // Reduce footstep audio times
        if(currFootstepAudioDelay > 0)
            currFootstepAudioDelay -= (float)delta;
    }

    /// <summary>
    /// Clears the path back to the exit by removing all extra nodes
    /// </summary>
    private void ClearFootsteps()
    {
        foreach (var item in footStepList)
            item.QueueFree();
        footStepList.Clear();
    }

    public override void _PhysicsProcess(double delta)
    {
        // Don't run if not initialized (most likely level not finished generating)
        if (!init) return;

        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Check if player tried to pause the game
        if (Input.IsActionJustPressed("pause"))
            uIManger.TogglePaused();

        // Get player input in vector2 and convert into world direction
        Vector2 input = Input.GetVector("move_left", "move_right", "move_down", "move_up").Normalized(); 
        Vector3 direction = (input.X * Basis.X + input.Y * -Basis.Z).Normalized();

        // Set speed depending on if player is sprinting or carrying a treasure chest
        currSpeed = Input.IsActionPressed("sprint") ? sprintSpeed : walkSpeed; 
        currSpeed *= carryingTreasure != null ? carryingMulti : 1;

        // Move the player
        Velocity = direction * currSpeed * (float)delta;
        MoveAndSlide();

        // Restart footstep audio if player starts sprinting
        if(Input.IsActionJustPressed("sprint"))
            currFootstepAudioDelay = 0;

        // Play footstep audio with some variation
        if (currFootstepAudioDelay <= 0 && input != Vector2.Zero)
        {
            footstepsAudio.PitchScale = (float)GD.RandRange(0.8, 1.2);
            footstepsAudio.VolumeDb = (float)GD.RandRange(stepOriginalVol - 1, stepOriginalVol + 1);
            footstepsAudio.PanningStrength = (float)GD.RandRange(0.9, 1.1);

            footstepsAudio.Play();
            currFootstepAudioDelay = Input.IsActionPressed("sprint") ? footstepDelayRun : footstepDelayWalk;
        }

        // Camera - controller
        Vector2 controllerCamera = Input.GetVector("look_left", "look_right", "look_down", "look_up").Normalized();
        if (!controllerCamera.IsZeroApprox())
        {
            Vector2 mouseInput = controllerCamera * sensitivity * 1.5f;

            xRot += mouseInput.Y;
            xRot = Mathf.Clamp(xRot, -90, 90);

            cameraHolder.RotationDegrees = Vector3.Right * xRot;
            RotationDegrees -= Vector3.Up * mouseInput.X;
        }
    }

    public override void _Input(InputEvent @event)
    {
        // Don't run if not initialized (most likely level not finished generating)
        if (!init) return;

        // Make sure game isn't still loading
        if (LevelLoader.isLoading) return;

        // Check if the player pressed the interact button
        if (@event.IsActionPressed("interact"))
        {
            if (carryingTreasure == null)
            {
                // Pick up
                carryingTreasure = (Node3D)ray.GetCollider();
                if (carryingTreasure == null)
                    return;
                // Check if it is treasure
                if (carryingTreasure.Name.ToString().Contains("Treasure"))
                {
                    scanner.Hide();
                    carryingTreasure.GlobalPosition = carryingPos.GlobalPosition;
                    carryingTreasure.Reparent(carryingPos);
                    ((RigidBody3D)carryingTreasure).Freeze = true;
                    carryingTreasureCollider1 = carryingTreasure.GetNode<Node3D>("CollisionShape3D");
                    carryingTreasure.RemoveChild(carryingTreasureCollider1);
                    carryingTreasureCollider2 = carryingTreasure.GetNode<Node3D>("CollisionShape3D2");
                    carryingTreasure.RemoveChild(carryingTreasureCollider2);
                    carryingTreasure.RotationDegrees = Vector3.Zero;
                    carryingTreasure.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
                }
                else
                {
                    carryingTreasure = null;
                }
            }
            else
            {
                scanner.Show();
                ClearFootsteps();
                carryingTreasure.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
                ((RigidBody3D)carryingTreasure).Freeze = false;
                // Check if standing in recieve zone
                if (exitArea.OverlapsBody(GetNode<PlayerController>(GetPath())))
                {
                    // Drop
                    ((DropTreasure)exitArea).RecieveTreasure(carryingTreasure);
                    GD.Print("SUCCESS : " + carryingTreasure);
                    carryingTreasure.QueueFree();
                    carryingTreasure = null;
                }
                else
                {
                    // Drop
                    carryingTreasure.Reparent(GetTree().Root);
                    // TODO: Add some force forward
                    GD.Print("Dropped : " + carryingTreasure);
                    carryingTreasure.AddChild(carryingTreasureCollider1);
                    carryingTreasure.AddChild(carryingTreasureCollider2);
                    carryingTreasureCollider1 = null;
                    carryingTreasureCollider2 = null;
                    carryingTreasure = null;
                }      
            }
        }

        // Camera Mouse
        if (@event is InputEventMouseMotion mouseDelta)
        {
            Vector2 mouseInput = mouseDelta.Relative * sensitivity * 0.03f;

            xRot -= mouseInput.Y;
            xRot = Mathf.Clamp(xRot, -90, 90);

            cameraHolder.RotationDegrees = Vector3.Right * xRot;
            RotationDegrees -= Vector3.Up * mouseInput.X;
        }
    }

    /// <summary>
    /// Sets whether or not the player is touching the exit.
    /// </summary>
    /// <param name="touching">True if the player is touching the exit</param>
    public void SetTouchingExit(bool touching)
    {
        touchingExit = touching;
    }

    /// <summary>
    /// Sets the area the player is currently touching.
    /// </summary>
    /// <param name="area">The area the player is touching</param>
    public void SetTouchingArea(Area3D area)
    {
        exitArea = area;
    }

    /// <summary>
    /// Update options related to the player
    /// </summary>
    private void UpdateOptions()
    {
        sensitivity = (float)optionsMenu.GetSensitivityValue();
    }

    public override void _ExitTree()
    {
        // Unsubscribe from the options update event when this node leavs the tree
        optionsMenu.OnOptionsChanged -= UpdateOptions;
    }
}
