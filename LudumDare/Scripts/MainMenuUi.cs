using Godot;
using System;

public partial class MainMenuUi : Control
{
    [Export] private string playScenePath;
    [Export] private Control mainMenu;
    [Export] private Control loadingMenu;
    [Export] private Control optionsMenu;

    public override void _Ready()
    {
        mainMenu.Show();
        loadingMenu.Hide();
        optionsMenu.Hide();

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void OnPlayButtonPressed()
    {
        mainMenu.Hide();
        // TODO: If time: animation
        loadingMenu.Show();
        GetTree().ChangeSceneToFile(playScenePath);
    }

    public void OnOptionsButtonPressed()
    {
        mainMenu.Hide();
        optionsMenu.Show();
    }

    public void OnQuitButtonPressed()
    {
        GetTree().Quit();
    }
}
