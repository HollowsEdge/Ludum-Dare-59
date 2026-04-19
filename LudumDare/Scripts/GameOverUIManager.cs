using Godot;
using System;

public partial class GameOverUIManager : Control
{
    [Export] private string mainMenuScenePath;

    public void OnMenuButtonPressed()
    {
        GetTree().ChangeSceneToFile(mainMenuScenePath);
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
