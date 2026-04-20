using Godot;
using System.Collections.Generic;

public partial class PlayerController : CharacterBody3D
{
    [Export] public float walkSpeed = 5f;
    [Export] public float sprintSpeed = 5f;
    [Export] public float carryingMulti = .9f;
    [Export] public float sensitivity = .5f;
    [Export] public RayCast3D ray;
    [Export] public Node3D carryingPos;
    [Export] public PackedScene footStepScene;
    [Export] public ScannerTool scanner;
    //[Export] public OptionsMenu optionsMenu;
    [Export] private AudioStreamPlayer3D footstepsAudio;
    [Export] public float footstepDelayWalk = .5f;
    [Export] public float footstepDelayRun = .3f;

    private float xRot;
    private Node3D cameraHolder;
    private float currSpeed;
    private float currFootstepDelay;
    private float stepOriginalVol;

    private Node3D carryingTreasure;
    private Node3D carryingTreasureCollider;
    private bool touchingExit = false;

    private Area3D exitArea;
    private LevelGenerate levelGenerate;

    private UIManger uIManger;
    private List<Node3D> footStepList = new();

    public bool init = false;

    public override void _Ready()
    {
        cameraHolder = GetNode<Node3D>("CameraHolder");
        Input.MouseMode = Input.MouseModeEnum.Captured;
        uIManger = (UIManger)GetTree().GetFirstNodeInGroup("UIManager");
        levelGenerate = (LevelGenerate)GetTree().GetFirstNodeInGroup("LevelGenerate");
        stepOriginalVol = footstepsAudio.VolumeDb;
        //optionsMenu.OnOptionsChanged -= UpdateOptions;
        UpdateOptions();
    }

    public override void _Process(double delta)
    {
        if (!init)
            return;

        if(carryingTreasure != null)
        {
            ClearFootsteps();

            foreach (Vector3 point in levelGenerate.GetNavigationPath(GlobalPosition, exitArea.GlobalPosition))
            {
                Node3D newFootStep = footStepScene.Instantiate<Node3D>();
                footStepList.Add(newFootStep);
                GetTree().Root.AddChild(newFootStep);
                newFootStep.GlobalPosition = new(point.X, 0, point.Z);
            }
        }

        if(currFootstepDelay > 0)
        {
            currFootstepDelay -= (float)delta;
        }
    }

    private void ClearFootsteps()
    {
        foreach (var item in footStepList)
            item.QueueFree();
        footStepList.Clear();
    }

    public override void _PhysicsProcess(double delta)
    {
        if (!init)
            return;
        if (Input.IsKeyPressed(Key.Escape))
            uIManger.TogglePaused();

        Vector2 input = Input.GetVector("move_left", "move_right", "move_down", "move_up").Normalized();

        currSpeed = Input.IsActionPressed("sprint") ? sprintSpeed : walkSpeed;

        Vector3 direction = (input.X * Basis.X + input.Y * -Basis.Z).Normalized();

        currSpeed *= carryingTreasure != null ? carryingMulti : 1;

        Velocity = direction * currSpeed * (float)delta;
        MoveAndSlide();

        if(Input.IsActionJustPressed("sprint"))
            currFootstepDelay = 0;

        if (currFootstepDelay <= 0 && input != Vector2.Zero)
        {
            footstepsAudio.PitchScale = (float)GD.RandRange(0.8, 1.2);
            footstepsAudio.VolumeDb = (float)GD.RandRange(stepOriginalVol - 1, stepOriginalVol + 1);
            footstepsAudio.PanningStrength = (float)GD.RandRange(0.9, 1.1);
            footstepsAudio.Play();
            currFootstepDelay = Input.IsActionPressed("sprint") ? footstepDelayRun : footstepDelayWalk;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!init)
            return;
        if (@event.IsActionPressed("interact"))
        {
            if (carryingTreasure == null)
            {
                // Pick up
                GodotObject hitObject = ray.GetCollider();
                carryingTreasure = ((Node3D)hitObject).GetParent<Node3D>();
                // Check if it is treasure
                if (carryingTreasure.Name.ToString().Contains("Treasure"))
                {
                    scanner.Hide();
                    carryingTreasure.GlobalPosition = carryingPos.GlobalPosition;
                    carryingTreasure.Reparent(carryingPos);
                    carryingTreasureCollider = carryingTreasure.GetNode<Node3D>("ColliderHolderBody");
                    carryingTreasure.RemoveChild(carryingTreasureCollider);
                    carryingTreasure.RotationDegrees = Vector3.Zero;
                    carryingTreasure.GetNode<AudioStreamPlayer3D>("AudioStreamPlayer3D").Play();
                    //GD.Print("Picked up : " + carryingTreasure);
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
                    carryingTreasure.AddChild(carryingTreasureCollider);
                    carryingTreasureCollider = null;
                    carryingTreasure = null;
                }      
            }
        }

        // Camera
        if (@event is InputEventMouseMotion mouseDelta)
        {
            Vector2 mouseInput = mouseDelta.Relative * sensitivity * 0.03f;

            xRot -= mouseInput.Y;
            xRot = Mathf.Clamp(xRot, -90, 90);

            cameraHolder.RotationDegrees = Vector3.Right * xRot;
            RotationDegrees -= Vector3.Up * mouseInput.X;
        }
    }

    public void SetTouchingExit(bool touching)
    {
        touchingExit = touching;
    }

    public void SetTouchingArea(Area3D area)
    {
        exitArea = area;
    }

    private void UpdateOptions()
    {
        var config = new ConfigFile();
        // Load data from a file.
        Error err = config.Load("user://settings.cfg");

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
        {
            return;
        }

        sensitivity = (float)config.GetValue("Player", "Sensitivity");
    }

    public override void _ExitTree()
    {
        //optionsMenu.OnOptionsChanged -= UpdateOptions;
    }
}
