using Godot;
using System;

public partial class GameOverUIManager : Control
{
    [Export] private string mainMenuScenePath;

    public override void _Ready()
    {
        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToFile(mainMenuScenePath);
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
