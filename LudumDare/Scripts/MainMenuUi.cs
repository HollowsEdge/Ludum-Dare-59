using Godot;
using System;

public partial class MainMenuUi : Control
{
    [Export] private string playScenePath;
    [Export] private Control mainMenu;
    [Export] private Control loadingMenu;
    [Export] private Control optionsMenu;
    [Export] private Control creditsMenu;
    [Export] private Control difficultySelection;
    [Export] private AudioStreamPlayer3D audioButtonClick;

    public override void _Ready()
    {
        mainMenu.Show();
        loadingMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Hide();
        difficultySelection.Hide();

        Input.MouseMode = Input.MouseModeEnum.Visible;
    }

    public void OnPlayButtonPressed()
    {
        mainMenu.Hide();
        difficultySelection.Show();
        audioButtonClick.Play();
    }

    public void PlayWithDifficulty(int difficulty)
    {
        // Create new ConfigFile object.
        var config = new ConfigFile();

        // Store some values.
        config.SetValue("Game", "Difficulty", difficulty);

        config.Save("user://game.cfg");

        audioButtonClick.Play();
        mainMenu.Hide();
        difficultySelection.Hide();
        loadingMenu.Show();
        GetTree().ChangeSceneToFile(playScenePath);
    }

    public void OnOptionsButtonPressed()
    {
        mainMenu.Hide();
        optionsMenu.Show();
        audioButtonClick.Play();
    }

    public void OnCreditsButtonPressed()
    {
        mainMenu.Hide();
        optionsMenu.Hide();
        creditsMenu.Show();
        audioButtonClick.Play();
    }

    public void OnMainMenuButtonPressed()
    {
        mainMenu.Show();
        difficultySelection.Hide();
        optionsMenu.Hide();
        creditsMenu.Hide();
        audioButtonClick.Play();
    }

    public void OnQuitButtonPressed()
    {
        audioButtonClick.Play();
        GetTree().Quit();
    }
}
