using Godot;

public partial class UIManger : Control
{
    [ExportCategory("References")]
    [Export] private string mainMenuSceneString;
    [Export] private Control pausedMenu;
    //[Export] private Control inGameOptions;
    [Export] private AudioStreamPlayer audioButtonClick;

    public override void _Ready()
    {
        // Hide the pause menu when the game loads
        pausedMenu.Hide();
        //inGameOptions.Hide();
    }

    /// <summary>
    /// Switches the UI to the options menu.
    /// </summary>
    public void OnOptionsPressed()
    {
        // Switch UI to show options menu
        pausedMenu.Hide();
        //inGameOptions.Show();
        audioButtonClick?.Play();
    }

    /// <summary>
    /// Switches the UI to the pause menu.
    /// </summary>
    public void OnOptionsBackPressed()
    {
        // Switch UI to show pause menu
        pausedMenu.Show();
        //inGameOptions.Hide();
        audioButtonClick?.Play();
    }

    /// <summary>
    /// Hides the pause menu and resumes the game.
    /// </summary>
    public void OnResumeButtonPressed()
    {
        SetPaused(false);
        audioButtonClick?.Play();
    }

    /// <summary>
    /// Toggles the current pause state of the game.
    /// </summary>
    public void TogglePaused()
    {
        SetPaused(!GetTree().Paused);
    }

    /// <summary>
    /// Manages the pause state of the game.
    /// </summary>
    /// <param name="setPaused">true pauses the game, false to unpause</param>
    public void SetPaused(bool setPaused)
    {
        GetTree().Paused = setPaused;
        pausedMenu.Visible = setPaused;

        Input.MouseMode = setPaused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }

    /// <summary>
    /// Loads the main menu
    /// </summary>
    public void OnMenuButtonPressed()
    {
        SetPaused(false);
        audioButtonClick?.Play();
        GetTree().ChangeSceneToFile(mainMenuSceneString);
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void OnQuitButtonPressed()
    {
        audioButtonClick?.Play();
        GetTree().Quit();
    }
}
