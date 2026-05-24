using Godot;

public partial class MainMenuUI : Control
{
    [ExportCategory("Scene Paths")]
    [Export] private string playScenePath;

    [ExportCategory("UI Nodes")]
    [Export] private Control mainMenu;
    [Export] private Control loadingMenu;
    [Export] private Control optionsMenu;
    [Export] private Control creditsMenu;
    [Export] private Control difficultySelectionMenu;

    [Export] private SpinBox seedSpinBox;
    [Export] private SpinBox chestNumSpinBox;
    [Export] private SpinBox monsterNumSpinBox;

    [ExportCategory("Audio")]
    [Export] private AudioStreamPlayer audioButtonClick;

    public override void _Ready()
    {
        GD.Randomize();

        // Show only the main menu
        mainMenu.Show();
        loadingMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Hide();
        difficultySelectionMenu.Hide();

        // Make sure the mouse is not locked to the screen
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    /// <summary>
    /// Switches the UI to the difficulty selection menu
    /// </summary>
    public void OnPlayButtonPressed()
    {
        mainMenu.Hide();
        difficultySelectionMenu.Show();
        audioButtonClick.Play();

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

        // Show the loading screen
        audioButtonClick.Play();
        mainMenu.Hide();
        difficultySelectionMenu.Hide();
        loadingMenu.Show();

        // Make sure to show the load menu (Setup proper level load async later)
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        // Change the scene to the game
        GetTree().ChangeSceneToFile(playScenePath);
    }

    /// <summary>
    /// Switches the UI to the options menu
    /// </summary>
    public void OnOptionsButtonPressed()
    {
        mainMenu.Hide();
        optionsMenu.Show();
        audioButtonClick.Play();
    }

    /// <summary>
    /// Switches the UI to the credits menu
    /// </summary>
    public void OnCreditsButtonPressed()
    {
        mainMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Show();
        audioButtonClick.Play();
    }

    /// <summary>
    /// Switches the UI to the main menu
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        mainMenu.Show();
        difficultySelectionMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Hide();
        audioButtonClick.Play();
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    public void OnQuitButtonPressed()
    {
        audioButtonClick.Play();
        GetTree().Quit();
    }
}
