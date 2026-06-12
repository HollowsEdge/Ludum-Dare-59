using Godot;
using System.Collections.Generic;

public partial class OptionsMenu : Control
{
    [ExportCategory("UI References")]
    [Export] private Control mainGraphicsMenu;
    [Export] private Control controlsMenu;
    [Export] private OptionButton displayButton;
    [Export] private Label resolutionScaleText;
    [Export] private Slider resolutionScaleSlider;
    [Export] private OptionButton scalingModeButton;
    [Export] private OptionButton upscalingButton;
    [Export] private OptionButton resolutionButton;
    [Export] private OptionButton vSyncButton;
    [Export] private Label sensText;
    [Export] private Slider sensSlider;
    [Export] private Label audioText;
    [Export] private Slider audioSlider;

    [ExportCategory("Audio")]
    [Export] private AudioStreamPlayer audioButtonClick;

    // Events
    public delegate void OnOptionsChangedEventHandler();
    public event OnOptionsChangedEventHandler OnOptionsChanged;
    private Dictionary<string, Vector2I> resolutions = new()
    {
        {"3840x2160", new(3840,2160)},
        {"2560x1440", new(2560,1440)},
        {"1920x1080", new(1920,1080)},
        {"1366x768", new(1366,768)},
        {"1536x864", new(1536,864)},
        {"1280x720", new(1280,720)},
        {"1440x900", new(1440,900)},
        {"1600x900", new(1600,900)},
        {"1024x600", new(1024,600)},
        {"800x600",  new(800,600) }
    };
    int busIndex = AudioServer.GetBusIndex("Master");

    public override void _Ready()
    {
        OnTabChanged(0);
        resolutionButton.Clear();
        int i = 0;
        foreach (string key in resolutions.Keys)
        {
            if (resolutions[key].X > DisplayServer.ScreenGetSize().X || resolutions[key].Y > DisplayServer.ScreenGetSize().Y)
                continue;
            resolutionButton.AddItem(key, i);
            i++;
        }

        // Load data from the save file
        ConfigFile config = new();
        Error err = config.Load("user://settings.cfg");

        // Setup previous saved settings
        displayButton.Select((int)config.GetValue("Player", "Display", 1)); // Default borderless
        resolutionScaleSlider.Value = (float)config.GetValue("Player", "ResolutionScale", 100);
        scalingModeButton.Select((int)config.GetValue("Player", "ScalingMode", (int)Viewport.Scaling3DModeEnum.Bilinear));
        upscalingButton.Select((int)config.GetValue("Player", "Upscaling", 0));
        resolutionButton.Select((int)config.GetValue("Player", "Resolution", 0));
        vSyncButton.Select((int)config.GetValue("Player", "vSync", (int)DisplayServer.WindowGetVsyncMode()));
        sensSlider.Value = (float)config.GetValue("Player", "Sensitivity", 1f);
        audioSlider.Value = (float)config.GetValue("Player", "Audio", 100f);

        ApplySettings();

        // Update incompatable settings
        resolutionButton.Disabled = displayButton.Selected <= 1; // Disable if in fullscreen
        resolutionScaleSlider.Editable = displayButton.Selected <= 1 && scalingModeButton.Selected == 0; // Enable if in fullscreen or fsr
        upscalingButton.Disabled = scalingModeButton.Selected == 0;
    }

    /// <summary>
    /// Returns to the main menu when the button is pressed. Also saves and applies current settings.
    /// </summary>
    public void OnMainMenuButtonPressed()
    {
        // Apply current settings
        ApplySettings();
        SaveGame();
        OnOptionsChanged?.Invoke();
    }

    /// <summary>
    /// Applies display settings using DisplayServer
    /// </summary>
    public void ApplySettings()
    {
        DisplayServer.WindowSetMode((DisplayServer.WindowMode)displayButton.GetSelectedId());
        DisplayServer.WindowSetVsyncMode((DisplayServer.VSyncMode)vSyncButton.GetSelectedId());

        // if in windowed mode
        if(displayButton.Selected > 1)
        {
            GetWindow().Size = resolutions[resolutionButton.GetItemText(resolutionButton.Selected)];

            // Center the window to the monitor
            Vector2I centerScreen = DisplayServer.ScreenGetPosition() + DisplayServer.ScreenGetSize() / 2;
            Vector2I windowSize = GetWindow().GetSizeWithDecorations();
            GetWindow().Position = centerScreen - windowSize / 2;
        }
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
        // Load save file
        ConfigFile config = new();
        config.Load("user://settings.cfg");

        // Store settings
        config.SetValue("Player", "Display", displayButton.Selected);
        config.SetValue("Player", "ResolutionScale", resolutionScaleSlider.Value);
        config.SetValue("Player", "ScalingMode", scalingModeButton.Selected);
        config.SetValue("Player", "Upscaling", upscalingButton.Selected);
        config.SetValue("Player", "Resolution", resolutionButton.Selected);
        config.SetValue("Player", "vSync", vSyncButton.Selected);
        config.SetValue("Player", "Sensitivity", sensSlider.Value);
        config.SetValue("Player", "Audio", audioSlider.Value);

        // Save to file
        config.Save("user://settings.cfg");
    }

    /// <summary>
    /// Changes the game resolution scale.
    /// </summary>
    /// <param name="value">New slider value</param>
    void OnScaleSliderChanged(float value) {

        float resolutionScale = value / 100f;
        string resolutionText = Mathf.RoundToInt(GetWindow().Size.X * resolutionScale) + "x" + Mathf.RoundToInt(GetWindow().Size.Y * resolutionScale);
        resolutionScaleText.Text = value + "% - " + resolutionText;
        GetViewport().Scaling3DScale = resolutionScale;
    }

    /// <summary>
    /// Manages settings when display is changed
    /// </summary>
    public void OnDisplayChanged(int _)
    {
        if(displayButton.Selected <= 1)
            resolutionButton.Select(0);
        else
            resolutionScaleSlider.Value = 100;
            
        resolutionButton.Disabled = displayButton.Selected <= 1;
        resolutionScaleSlider.Editable = displayButton.Selected <= 1 && scalingModeButton.Selected == 0; // Enable if in fullscreen or FSR

    }

    /// <summary>
    /// Sets the scaling mode of the game.
    /// </summary>
    /// <param name="index">Selected option index</param>
    public void OnScalingModeChanged(int index)
    {
        upscalingButton.Disabled = scalingModeButton.Selected == 0;
        resolutionScaleSlider.Editable = displayButton.Selected <= 1 && scalingModeButton.Selected == 0; // Enable if in fullscreen or FSR

        if (index == 1)
            OnUpscaleValueChanged(upscalingButton.Selected);
        else
            resolutionScaleSlider.Value = 100;
        GetViewport().Scaling3DMode = index == 1 ? Viewport.Scaling3DModeEnum.Fsr2 : Viewport.Scaling3DModeEnum.Bilinear;
    }

    /// <summary>
    /// Updates resolution based on recomended presets for FSR 2
    /// </summary>
    /// <param name="index">Selected option index</param>
    public void OnUpscaleValueChanged(int index)
    {
        switch (index)
        {
            case 0: // Ultra Quality
                resolutionScaleSlider.Value = 77f;
                break;
            case 1: // Quality
                resolutionScaleSlider.Value = 67f;
                break;
            case 2: // Balanced
                resolutionScaleSlider.Value = 59f;
                break;
            case 3: // Performance
                resolutionScaleSlider.Value = 50f;
                break;
        }
    }

    /// <summary>
    /// Sets the audio volume to the target value.
    /// </summary>
    /// <param name="value">The new audio precent to display.</param>
    public void OnAudioSliderChanged(float value)
    {
        audioText.Text = value.ToString() + "%";
        AudioServer.SetBusVolumeLinear(busIndex, value/100);
    }

    /// <summary>
    /// Returns the current value of the player sensitivity slider.
    /// </summary>
    public double GetSensitivityValue()
    {
        return sensSlider.Value;
    }

    /// <summary>
    /// Switchs the options menu.
    /// </summary>
    /// <param name="tab">Index of selected tab</param>
    public void OnTabChanged(int tab)
    {
        mainGraphicsMenu.Visible = tab == 0;
        controlsMenu.Visible = tab == 1;
    }
}
