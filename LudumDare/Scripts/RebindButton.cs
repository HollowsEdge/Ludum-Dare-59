using Godot;
using Godot.Collections;

public partial class RebindButton : Button
{
    [Export] public AudioStreamPlayer buttonClick;
    [Export] public string actionName = "";
    [Export] public bool isControllerBinding = false;

    private bool waitingForInput = false;

    public override void _Ready()
    {
        // Load settings save file
        ConfigFile config = new();
        Error err = config.Load("user://settings.cfg");

        if (isControllerBinding)
        {
            // Load controller rebinds
            if (config.HasSectionKey("ControllerControls", actionName))
            {
                Variant value = config.GetValue("ControllerControls", actionName);
                RebindAction((InputEvent)value, false);
            }   
        }
        else
        {
            // Load Keyboard rebinds
            if (config.HasSectionKey("KeyboardControls", actionName))
            {
                Variant value = config.GetValue("KeyboardControls", actionName);
                RebindAction((InputEvent)value, false);
            }
        }   

        Pressed += StartRebind;
        UpdateDisplay();
    }

    /// <summary>
    /// Starts waiting for a button press to rebind actionName
    /// </summary>
    private void StartRebind()
    {
        waitingForInput = true;
        Text = "Press input...";
        Icon = null;
        buttonClick?.Play();
    }

    public override void _Input(InputEvent @event)
    {
        // Skip if not waiting for rebind
        if (!waitingForInput)
            return;

        // Attempt to get valid input event
        InputEvent newEvent = null;
        if (isControllerBinding)
        {
            if (@event is InputEventJoypadButton joyButton && joyButton.Pressed)
                newEvent = joyButton;
            else if (@event is InputEventJoypadMotion joyMotion && Mathf.Abs(joyMotion.AxisValue) > 0.5f) // using Mathf.Abs > 0.5f so accidental tap doesn't count
                newEvent = joyMotion;
        }
        else
        {
            if (@event is InputEventKey key && key.Pressed && !key.Echo)
                newEvent = key;
            else if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !mouseButton.DoubleClick)
                newEvent = mouseButton;
        }

        // Retrurn if event was not found
        if (newEvent == null)
            return;

        // Rebind the action to the new event
        RebindAction(newEvent);

        waitingForInput = false;
        GetViewport().SetInputAsHandled();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="newEvent">Event use as rebind</param>
    /// <param name="saveRebind">Should the action be saved - default: true</param>
    private void RebindAction(InputEvent newEvent, bool saveRebind = true)
    {
        Array<InputEvent> events = InputMap.ActionGetEvents(actionName);

        foreach (InputEvent inputEvent in events)
        {
            // Check if event is a controller event
            bool remove = isControllerBinding ? (inputEvent is InputEventJoypadButton || inputEvent is InputEventJoypadMotion) : (inputEvent is InputEventKey || (inputEvent is InputEventMouseButton mouseButton && !mouseButton.DoubleClick));

            // Remove old event
            if (remove)
                InputMap.ActionEraseEvent(actionName, inputEvent);
        }
        InputMap.ActionAddEvent(actionName, newEvent); // Add the new event

        // Save the rebind
        if (saveRebind)
        {
            ConfigFile config = new();
            Error err = config.Load("user://settings.cfg");

            if (isControllerBinding)
                config.SetValue("ControllerControls", actionName, newEvent);
            else
                config.SetValue("KeyboardControls", actionName, newEvent);
            config.Save("user://settings.cfg");
        }

        UpdateDisplay();
    }

    /// <summary>
    /// Updates the UI displayed to the current InputEvent
    /// </summary>
    public void UpdateDisplay()
    {
        // Default text
        Text = "Unbound";

        foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
        {
            if (isControllerBinding && inputEvent is InputEventJoypadButton joyButton)
            {
                // Load controller image
                Text = "";
                string path = $"res://Images/ControllerIcons/{joyButton.ButtonIndex}.png";
                if (ResourceLoader.Exists(path))
                    Icon = GD.Load<CompressedTexture2D>(path);
                else
                {
                    // Use text as fallback if image doesn't exist
                    Icon = null;
                    Text = joyButton.ButtonIndex.ToString();
                    GD.PushWarning("Controller button image does not exist. " + path);
                }
                return;
            }
            else if (isControllerBinding && inputEvent is InputEventJoypadMotion joyMotion)
            {
                // Load controller image 
                Text = "";
                string path = $"res://Images/ControllerIcons/{joyMotion.Axis}{(joyMotion.AxisValue > 0 ? '+' : '-')}.png";
                if (ResourceLoader.Exists(path))
                    Icon = GD.Load<CompressedTexture2D>(path);
                else
                {
                    // Use text as fallback if image doesn't exist
                    Icon = null;
                    Text = joyMotion.Axis.ToString() + (joyMotion.AxisValue > 0 ? '+' : '-');
                    GD.PushWarning("Controller button image does not exist. " + path);
                }
                return;
            }

            if (!isControllerBinding && inputEvent is InputEventKey key)
            {
                // Set text to key name
                Text = key.AsTextPhysicalKeycode();
                return;
            }
            else if (!isControllerBinding && inputEvent is InputEventMouseButton mouseButton && !mouseButton.DoubleClick)
            {
                // Set text to mouse button name
                Text = mouseButton.AsText();
                return;
            }
        }
    }
}
