using Godot;
using System;

public partial class LevelLoader : Node
{
    [Export] private string gameScenePath;
    [Export] private Control loadingMenu;
    [Export] private ProgressBar progressBar;

    int unloadMainMenuLoadWeight = 5;
    int loadGameSceneLoadWeight = 10;
    int mapGenWeight = 85;

    private int currentLoadPrecentage = 0;
    private int currentGameLoadPrecentage = 0;

    public static bool isLoading = false;
    public static bool usingController = false;

    public override void _Ready()
    {
        loadingMenu.Hide(); // Hide loading menu
    }

    public async void LoadGame()
    {
        // Show Loading Menu
        loadingMenu.Show();
        isLoading = true;
        await ToSignal(GetTree(), "process_frame");

        // Unload main menu
        GetTree().CurrentScene.QueueFree();
        currentLoadPrecentage = unloadMainMenuLoadWeight;
        progressBar.SetValueNoSignal(currentLoadPrecentage);
        await ToSignal(GetTree(), "process_frame");

        // Load game scene
        PackedScene gameScene = GD.Load<PackedScene>(gameScenePath);
        LevelGenerate loadedGameScene = gameScene.Instantiate<LevelGenerate>();
        loadedGameScene.OnLoadPrecentChanged += OnGameLoadPrecentChanged;

        GetTree().Root.AddChild(loadedGameScene);
        GetTree().CurrentScene = loadedGameScene;

        currentLoadPrecentage += loadGameSceneLoadWeight;
        progressBar.SetValueNoSignal(currentLoadPrecentage);
        await ToSignal(GetTree(), "process_frame");

        // Wait for game to be loaded ( < half a second)
        int frames = 0;
        while (currentGameLoadPrecentage < 100)
        {
            frames++;
            progressBar.SetValueNoSignal(currentLoadPrecentage);
            await ToSignal(GetTree(), "process_frame");
        }

        progressBar.SetValueNoSignal(currentLoadPrecentage);

        // Wait total of 1/2 second
        await ToSignal(GetTree().CreateTimer(((Engine.GetFramesPerSecond() / 2) - frames) / Engine.GetFramesPerSecond()), SceneTreeTimer.SignalName.Timeout);

        progressBar.SetValueNoSignal(currentLoadPrecentage);
        await ToSignal(GetTree(), "process_frame");

        // Hide the loading menu
        loadedGameScene.OnLoadPrecentChanged -= OnGameLoadPrecentChanged;

        // Linger 1/2 second on the loading bar when it is at 100%
        await ToSignal(GetTree().CreateTimer(0.5f), SceneTreeTimer.SignalName.Timeout);

        loadingMenu.Hide();
        isLoading = false;
    }

    void OnGameLoadPrecentChanged(int currPrecent)
    {
        currentGameLoadPrecentage = currPrecent;
        currentLoadPrecentage = unloadMainMenuLoadWeight + loadGameSceneLoadWeight + (int)(currentGameLoadPrecentage * (mapGenWeight / 100.0f));
    }
}
