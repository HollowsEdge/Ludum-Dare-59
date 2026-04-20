using Godot;

public partial class UIManger : Control
{
    [Export] private string mainMenuSceneString;
    [Export] private Control pausedMenu;
    //[Export] private Control inGameOptions;
    [Export] private AudioStreamPlayer3D audioButtonClick;


    public override void _Ready()
    {
        pausedMenu.Hide();
        //inGameOptions.Hide();
    }

    public void OnOptionsPressed()
    {
        pausedMenu.Hide();
        //inGameOptions.Show();
        audioButtonClick.Play();
    }

    public void OnOptionsBackPressed()
    {
        pausedMenu.Show();
        //inGameOptions.Hide();
        audioButtonClick.Play();
    }

    public void OnResumeButtonPressed()
    {
        SetPaused(false);
        audioButtonClick.Play();
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
        SetPaused(false);
        audioButtonClick.Play();
        GetTree().ChangeSceneToFile(mainMenuSceneString);
    }

    public void OnQuitButtonPressed()
    {
        audioButtonClick.Play();
        GetTree().Quit();
    }
}
