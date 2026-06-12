using Godot;

public partial class UIManger : Control
{
    [ExportCategory("References")]
    [Export] private string mainMenuSceneString;
    [Export] private Control pausedMenuRoot;
    [Export] private Control pausedMenu;
    [Export] private Control mainMenuFocusButton;
    [Export] private Control optionsMenu;
    [Export] private Control optionsMenuFocusButton;
    [Export] private AudioStreamPlayer audioButtonClick;

    enum CurrentMenu
    {
        Main,
        Options
    }
    private CurrentMenu currentMenu;

    public override void _Ready()
    {
        currentMenu = CurrentMenu.Main;

        // Hide the pause menu when the game loads
        pausedMenuRoot.Hide();
        pausedMenu.Show();
        optionsMenu.Hide();

        mainMenuFocusButton.GrabFocus();
    }

    /// <summary>
    /// Switches the UI to the options menu.
    /// </summary>
    public void OnOptionsPressed()
    {
        currentMenu = CurrentMenu.Options;

        // Switch UI to show options menu
        pausedMenu.Hide();
        optionsMenu.Show();
        audioButtonClick?.Play();

        if (LevelLoader.usingController)
            optionsMenuFocusButton.GrabFocus();
    }

    /// <summary>
    /// Switches the UI to the pause menu.
    /// </summary>
    public void OnOptionsBackPressed()
    {
        currentMenu = CurrentMenu.Main;

        // Switch UI to show pause menu
        pausedMenu.Show();
        optionsMenu.Hide();
        audioButtonClick?.Play();

        if(LevelLoader.usingController)
            mainMenuFocusButton.GrabFocus();
    }

    /// <summary>
    /// Hides the pause menu and resumes the game.
    /// </summary>
    public void OnResumeButtonPressed()
    {
        SetPaused(false);
        audioButtonClick?.Play();
    }

    /// <summary>
    /// Toggles the current pause state of the game.
    /// </summary>
    public void TogglePaused()
    {
        SetPaused(!GetTree().Paused);
    }

    /// <summary>
    /// Manages the pause state of the game.
    /// </summary>
    /// <param name="setPaused">true pauses the game, false to unpause</param>
    public void SetPaused(bool setPaused)
    {
        GetTree().Paused = setPaused;
        pausedMenuRoot.Visible = setPaused;
           
        Input.MouseMode = setPaused ? Input.MouseModeEnum.Visible : Input.MouseModeEnum.Captured;

        if (LevelLoader.usingController && setPaused)
        {
            optionsMenuFocusButton.GrabFocus();
            Input.MouseMode = Input.MouseModeEnum.Hidden;
        }
    }

    /// <summary>
    /// Loads the main menu
    /// </summary>
    public void OnMenuButtonPressed()
    {
        SetPaused(false);
        audioButtonClick?.Play();
        GetTree().ChangeSceneToFile(mainMenuSceneString);
    }

    /// <summary>
    /// Quits the game
    /// </summary>
    public void OnQuitButtonPressed()
    {
        audioButtonClick?.Play();
        GetTree().Quit();
    }

    public override void _Input(InputEvent @event)
    {
        bool originalUsingGamepad = LevelLoader.usingController;
        // Check if the event is part of a controller
        if (@event is InputEventJoypadButton joyButton && joyButton.Pressed)
        {
            if (!LevelLoader.usingController)
                LevelLoader.usingController = true;
        }
        else if (@event is InputEventJoypadMotion joyMotion && Mathf.Abs(joyMotion.AxisValue) > 0.2f)
        {
            if (!LevelLoader.usingController)
                LevelLoader.usingController = true;
        }
        else if (@event is InputEventMouseMotion)
        {
            LevelLoader.usingController = false;
        }
        else if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed)
        {
            LevelLoader.usingController = false;
        }
        else if (@event is InputEventKey keyButton && keyButton.Pressed)
        {
            LevelLoader.usingController = false;
        }

        if (originalUsingGamepad != LevelLoader.usingController)
        {
            if (LevelLoader.usingController)
            {
                if (GetTree().Paused)
                {
                    switch (currentMenu)
                    {
                        case CurrentMenu.Main:
                            mainMenuFocusButton.GrabFocus();
                            break;
                        case CurrentMenu.Options:
                            optionsMenuFocusButton.GrabFocus();
                            break;
                    }
                }
                
                Input.MouseMode = Input.MouseModeEnum.Hidden;
            }
            else
            {
                if (GetTree().Paused)
                {                    
                    // release focus of any up item currently focused
                    GetViewport().GuiReleaseFocus();
                    Input.MouseMode = Input.MouseModeEnum.Visible;
                }
                else
                {
                    Input.MouseMode = Input.MouseModeEnum.Captured;
                }
            }
        }
    }
}
