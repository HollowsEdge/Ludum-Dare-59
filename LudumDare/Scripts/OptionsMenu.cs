using Godot;

public partial class OptionsMenu : Control
{
    [ExportCategory("UI References")]
    [Export] private Label sensText;
    [Export] private Slider sensSlider;
    [Export] private CheckButton fullscreenButton;

    // Events
    public delegate void OnOptionsChangedEventHandler();
    public event OnOptionsChangedEventHandler OnOptionsChanged;

    public override void _Ready()
    {
        // Load data from the save file
        ConfigFile config = new();
        Error err = config.Load("user://settings.cfg");

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
            return;

        // Setup previous saved settings
        sensSlider.Value = (float)config.GetValue("Player", "Sensitivity");
        fullscreenButton.ButtonPressed = (bool)config.GetValue("Player", "Fullscreen");
        DisplayServer.WindowSetMode(fullscreenButton.ButtonPressed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }

    /// <summary>
    /// Returns to the main menu when the button is pressed. Also saves and applies current settings.
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        // Apply current settings
        DisplayServer.WindowSetMode(fullscreenButton.ButtonPressed ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
        SaveGame();
        OnOptionsChanged?.Invoke();
    }

    /// <summary>
    /// Sets the text of the sensitivity to the target value.
    /// </summary>
    /// <param name="newSens">The new sensitivity to display.</param>
    public void SetSensText(float newSens)
    {
        sensText.Text = newSens.ToString();
    }

    /// <summary>
    /// Saves all options data to a config file
    /// </summary>
    public void SaveGame()
    {
        // Create new save file
        ConfigFile config = new();

        // Store settings
        config.SetValue("Player", "Sensitivity", sensSlider.Value);
        config.SetValue("Player", "Fullscreen", fullscreenButton.ButtonPressed);

        // Save to file
        config.Save("user://settings.cfg");
    }

    /// <summary>
    /// Returns the current value of the player sensitivity slider.
    /// </summary>
    public double GetSensitivityValue()
    {
        return sensSlider.Value;
    }
}
