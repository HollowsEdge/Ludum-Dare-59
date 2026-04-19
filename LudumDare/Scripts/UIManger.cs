using Godot;

public partial class UIManger : Control
{
    [Export] private PackedScene mainMenuScene;
    [Export] private Control pausedMenu;

    public override void _Ready()
    {
        pausedMenu.Hide();
    }

    public void OnResumeButtonPressed()
    {
        SetPaused(false);
    }

    public void TogglePaused()
    {
        SetPaused(!GetTree().Paused);
    }

    public void SetPaused(bool setPaused)
    {
        GetTree().Paused = setPaused;
        pausedMenu.Visible = setPaused;

        Input.MouseMode = setPaused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;
    }

    public void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToPacked(mainMenuScene);
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
