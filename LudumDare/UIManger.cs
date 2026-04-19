using Godot;

public partial class UIManger : Control
{
    [Export] private PackedScene mainMenuScene;

    public void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToPacked(mainMenuScene);
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
