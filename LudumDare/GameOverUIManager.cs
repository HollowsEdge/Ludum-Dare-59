using Godot;
using System;

public partial class GameOverUIManager : Control
{
    public void OnMenuButtonPressed()
    {

    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
