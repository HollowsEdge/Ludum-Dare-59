using Godot;

public partial class UIManger : Control
{
    [Export] private string mainMenuSceneString;
    [Export] private Control pausedMenu;
    //[Export] private Control inGameOptions;
    [Export] private AudioStreamPlayer audioButtonClick;


    public override void _Ready()
    {
        if(audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
        pausedMenu.Hide();
        //inGameOptions.Hide();
    }

    public void OnOptionsPressed()
    {
        pausedMenu.Hide();
        //inGameOptions.Show();
        if (audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
        audioButtonClick.Play();

    }

    public void OnOptionsBackPressed()
    {
        pausedMenu.Show();
        //inGameOptions.Hide();
        if (audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
        audioButtonClick.Play();
    }

    public void OnResumeButtonPressed()
    {
        SetPaused(false);
        if (audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
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
        if (audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
        audioButtonClick.Play();
        GetTree().ChangeSceneToFile(mainMenuSceneString);
    }

    public void OnQuitButtonPressed()
    {
        if (audioButtonClick == null)
        {
            audioButtonClick = new();
            AddChild(audioButtonClick);
        }
        audioButtonClick.Play();
        GetTree().Quit();
    }
}
