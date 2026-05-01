using Godot;

public partial class GameOverUIManager : Control
{
    [ExportCategory("Scene Paths")]
    [Export] private string mainMenuScenePath;

    public override void _Ready()
    {
        // Make sure the mouse is not locked to the screen
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    /// <summary>
    /// Changes the current scene to the main menu.
    /// </summary>
    public void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToFile(mainMenuScenePath);
    }

    /// <summary>
    /// Exits the game.
    /// </summary>
    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
