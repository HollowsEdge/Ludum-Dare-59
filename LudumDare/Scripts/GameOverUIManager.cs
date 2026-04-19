using Godot;
using System;

public partial class GameOverUIManager : Control
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
