using Godot;
using System;

public partial class OptionsMenu : Control
{
    [Export] Label sensText;
    [Export] Slider sensSlider;

    public delegate void OnOptionsChangedEventHandler();
    public event OnOptionsChangedEventHandler OnOptionsChanged;

    public override void _Ready()
    {
        var config = new ConfigFile();
        // Load data from a file.
        Error err = config.Load("user://settings.cfg");

        // If the file didn't load, ignore it.
        if (err != Error.Ok)
        {
            return;
        }

        sensSlider.Value = (float)config.GetValue("Player", "Sensitivity");
    }

    public void OnMainMenuButtonPressed()
    {
        SaveGame();
        OnOptionsChanged?.Invoke();
    }

    public void SetSensText(float newSens)
    {
        sensText.Text = newSens.ToString();
    }

    public void SaveGame()
    {
        // Create new ConfigFile object.
        var config = new ConfigFile();

        // Store some values.
        config.SetValue("Player", "Sensitivity", sensSlider.Value);

        config.Save("user://settings.cfg");
    }
}
