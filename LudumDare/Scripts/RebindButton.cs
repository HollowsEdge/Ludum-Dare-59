using Godot;
using Godot.Collections;
using System;

public partial class RebindButton : Button
{
    [Export] public AudioStreamPlayer buttonClick;
    [Export] public string actionName = "";
    [Export] public bool isControllerBinding = false;

    private bool waitingForInput = false;

    public override void _Ready()
    {
        ConfigFile config = new();
        Error err = config.Load("user://settings.cfg");

        if (isControllerBinding)
        {
            if (config.HasSectionKey("ControllerControls", actionName))
                RebindAction((InputEvent)config.GetValue("ControllerControls", actionName), false);
        }
        else
        {
            if (config.HasSectionKey("KeyboardControls", actionName))
            {
                Variant value = config.GetValue("KeyboardControls", actionName);

                RebindAction((InputEvent)value, false);
            }
        }   

        Pressed += StartRebind;
        UpdateDisplay();
    }

    private void StartRebind()
    {
        waitingForInput = true;
        Text = "Press input...";
        Icon = null;
        buttonClick?.Play();
    }

    public override void _Input(InputEvent @event)
    {
        if (!waitingForInput)
            return;

        InputEvent newEvent = null;

        if (isControllerBinding)
        {
            if (@event is InputEventJoypadButton joyButton && joyButton.Pressed)
                newEvent = joyButton;
            else if (@event is InputEventJoypadMotion joyMotion && Mathf.Abs(joyMotion.AxisValue) > 0.5f)
                newEvent = joyMotion;
        }
        else
        {
            if (@event is InputEventKey key && key.Pressed && !key.Echo)
                newEvent = key;
            else if (@event is InputEventMouseButton mouseButton && mouseButton.Pressed && !mouseButton.DoubleClick)
                newEvent = mouseButton;
        }

        if (newEvent == null)
            return;

        RebindAction(newEvent);

        waitingForInput = false;
        GetViewport().SetInputAsHandled();
    }

    private void RebindAction(InputEvent newEvent, bool saveRebind = true)
    {
        Array<InputEvent> events = InputMap.ActionGetEvents(actionName);

        foreach (InputEvent inputEvent in events)
        {
            bool remove = isControllerBinding ? (inputEvent is InputEventJoypadButton || inputEvent is InputEventJoypadMotion) : (inputEvent is InputEventKey || (inputEvent is InputEventMouseButton mouseButton && !mouseButton.DoubleClick));

            if (remove)
                InputMap.ActionEraseEvent(actionName, inputEvent);
        }
        InputMap.ActionAddEvent(actionName, newEvent);

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

    public void UpdateDisplay()
    {
        Text = "Unbound";

        foreach (InputEvent inputEvent in InputMap.ActionGetEvents(actionName))
        {
            if (isControllerBinding && inputEvent is InputEventJoypadButton joyButton)
            {
                Text = "";
                string path = $"res://Images/ControllerIcons/{joyButton.ButtonIndex}.png";
                if (ResourceLoader.Exists(path))
                    Icon = GD.Load<CompressedTexture2D>(path);
                else
                    GD.PushWarning("Controller button image does not exist. " + path);
                return;
            }
            else if (isControllerBinding && inputEvent is InputEventJoypadMotion joyMotion)
            {
                Text = "";
                string path = $"res://Images/ControllerIcons/{joyMotion.Axis}{(joyMotion.AxisValue > 0 ? '+' : '-')}.png";
                if (ResourceLoader.Exists(path))
                    Icon = GD.Load<CompressedTexture2D>(path);
                else
                    GD.PushWarning("Controller button image does not exist. " + path);
                return;
            }

            if (!isControllerBinding && inputEvent is InputEventKey key)
            {
                Text = key.AsTextPhysicalKeycode();
                return;
            }
            else if (!isControllerBinding && inputEvent is InputEventMouseButton mouseButton && !mouseButton.DoubleClick)
            {
                Text = mouseButton.AsText();
                return;
            }
        }
    }
}
