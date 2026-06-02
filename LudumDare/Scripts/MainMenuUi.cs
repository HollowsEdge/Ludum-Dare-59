using Godot;
using System;

public partial class MainMenuUI : Control
{
    [ExportCategory("UI Nodes")]
    [Export] private Control mainMenu;
    [Export] private Control mainMenuFocusButton;
    [Export] private Control optionsMenu;
    [Export] private Control optionsFocusButton;
    [Export] private Control creditsMenu;
    [Export] private Control creditsFocusButton;
    [Export] private Control difficultySelectionMenu;
    [Export] private Control difficultySelectionFocusButton;

    [Export] private SpinBox seedSpinBox;
    [Export] private SpinBox chestNumSpinBox;
    [Export] private SpinBox monsterNumSpinBox;

    [ExportCategory("Audio")]
    [Export] private AudioStreamPlayer audioButtonClick;

    [ExportCategory("Animation")]
    [Export] private AnimationPlayer introCameraAnimation;

    // References
    private LevelLoader levelLoader;

    enum CurrentMenu
    {
        Main,
        Options,
        Credits,
        Difficulty
    }
    private CurrentMenu currentMenu;

    public override void _Ready()
    {
        currentMenu = CurrentMenu.Main;
        GD.Randomize();

        // Show only the main menu
        mainMenu.Show();
        optionsMenu.Hide();
        creditsMenu.Hide();
        difficultySelectionMenu.Hide();

        // Make sure the mouse is not locked to the screen
        Input.MouseMode = Input.MouseModeEnum.Visible;
        levelLoader = GetTree().Root.GetNode<LevelLoader>("LevelLoader");
    }

    /// <summary>
    /// Switches the UI to the difficulty selection menu
    /// </summary>
    public void OnDifficultyButtonPressed()
    {
        currentMenu = CurrentMenu.Difficulty;

        mainMenu.Hide();
        difficultySelectionMenu.Show();
        audioButtonClick.Play();
        if (LevelLoader.usingController)
            difficultySelectionFocusButton.GrabFocus();

        // Load the game settings from save file
        ConfigFile config = new();
        Error err = config.Load("user://game.cfg");

        // If the file didn't load, ignore it.
        if (err == Error.Ok)
        {
            seedSpinBox.SetValueNoSignal((int)config.GetValue("Game", "LastSeed", 0));
            chestNumSpinBox.SetValueNoSignal((int)config.GetValue("Game", "TreasureAmount", 3));
            monsterNumSpinBox.SetValueNoSignal((int)config.GetValue("Game", "MonsterCount", 1));
        }
    }

    /// <summary>
    /// Starts the game with a chosen difficulty settings
    /// </summary>
    public async void PlayGame()
    {
        // Create new ConfigFile object.
        var config = new ConfigFile();

        // Store and save difficulty selection
        config.SetValue("Game", "TreasureAmount", (int)chestNumSpinBox.Value);
        config.SetValue("Game", "MonsterCount", (int)monsterNumSpinBox.Value);

        int targetSeed = (int)seedSpinBox.Value;
        if (targetSeed != 0)
        {
            GD.Seed((ulong)targetSeed);
        }
        else
        {
            GD.Randomize();
            targetSeed = GD.RandRange(1, int.MaxValue);
        }

        config.SetValue("Game", "LastSeed", targetSeed);

        config.Save("user://game.cfg");

        audioButtonClick.Play();

        // Play intro animation and wait for it to finish (but cut slightly short so camera is still "Falling")
        difficultySelectionMenu.Hide();
        introCameraAnimation.Play("Intro");
        await ToSignal(GetTree().CreateTimer(introCameraAnimation.CurrentAnimationLength - 0.1f), SceneTreeTimer.SignalName.Timeout);

        // Change the scene to the game
        levelLoader.LoadGame();
    }

    /// <summary>
    /// Switches the UI to the options menu
    /// </summary>
    public void OnOptionsButtonPressed()
    {
        currentMenu = CurrentMenu.Options;

        mainMenu.Hide();
        optionsMenu.Show();
        audioButtonClick.Play();

        if(LevelLoader.usingController)
            optionsFocusButton.GrabFocus();
    }

    /// <summary>
    /// Switches the UI to the credits menu
    /// </summary>
    public void OnCreditsButtonPressed()
    {
        currentMenu = CurrentMenu.Credits;

        mainMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Show();
        audioButtonClick.Play();

        if (LevelLoader.usingController)
            creditsFocusButton.GrabFocus();
    }

    /// <summary>
    /// Switches the UI to the main menu
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        currentMenu = CurrentMenu.Main;

        mainMenu.Show();
        difficultySelectionMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Hide();
        audioButtonClick.Play();
        if (LevelLoader.usingController)
            mainMenuFocusButton.GrabFocus();
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    public void OnQuitButtonPressed()
    {
        audioButtonClick.Play();
        GetTree().Quit();
    }

    public override void _Input(InputEvent @event)
    {
        bool originalUsingGamepad = LevelLoader.usingController;
        // Check if the event is part of a controller
        if (@event is InputEventJoypadButton joyEvent)
        {
            if (joyEvent.Pressed && !LevelLoader.usingController)
                LevelLoader.usingController = true;
        }
        else if(@event is InputEventJoypadMotion)
        {
            if (!LevelLoader.usingController)
                LevelLoader.usingController = true;
        }
        else if (@event is InputEventMouseMotion)
        {
            LevelLoader.usingController = false;
        }
        else if (@event is InputEventMouseButton mouseButton)
        {
            if(mouseButton.Pressed)
                LevelLoader.usingController = false;
        }
        else if (@event is InputEventKey keyButton)
        {
            if (keyButton.Pressed)
                LevelLoader.usingController = false;
        }

        if(originalUsingGamepad != LevelLoader.usingController)
        {
            if (LevelLoader.usingController)
            {
                switch (currentMenu)
                {
                    case CurrentMenu.Main:
                        mainMenuFocusButton.GrabFocus();
                        break;
                    case CurrentMenu.Options:
                        optionsFocusButton.GrabFocus();
                        break;
                    case CurrentMenu.Credits:
                        creditsFocusButton.GrabFocus();
                        break;
                    case CurrentMenu.Difficulty:
                        difficultySelectionFocusButton.GrabFocus();
                        break;
                }
                Input.MouseMode = Input.MouseModeEnum.Hidden;
            }
            else
            {
                // release focus of any up item currently focused
                GetViewport().GuiReleaseFocus();
                Input.MouseMode = Input.MouseModeEnum.Visible;
            }
        }
    }
}
