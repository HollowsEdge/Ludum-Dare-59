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

    [ExportCategory("Audio")]
    [Export] private AudioStreamPlayer3D audioButtonClick;

    public override void _Ready()
    {
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
    }

    /// <summary>
    /// Starts the game with a chosen difficulty level. 0 - Peaceful, 1 - Easy, 2 - Medium, 3 - Hard
    /// </summary>
    public void PlayWithDifficulty(int difficulty)
    {
        // Create new ConfigFile object.
        var config = new ConfigFile();

        // Store and save difficulty selection
        config.SetValue("Game", "Difficulty", difficulty);

        config.Save("user://game.cfg");

        // Show the loading screen
        audioButtonClick.Play();
        mainMenu.Hide();
        difficultySelectionMenu.Hide();
        loadingMenu.Show();

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
