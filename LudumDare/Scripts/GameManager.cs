using Godot;
using System;

public partial class GameManager : Node
{
    private int totalTreasure = -1;

    private int collectedTreasure = 0;

    [Export] private string gameOverUIScenePath;
    [Export] private string gameWinUIScenePath;

    public void SetTotalTreasure(int amount)
    {
        totalTreasure = amount;
    }

    public void AddTreasure()
    {
        if (totalTreasure < 0)
            GD.PrintErr("GameManager: DIDN'T SET totalTreasure (-1)");

        collectedTreasure++;

        GD.Print($"GameManager: Total treasure {collectedTreasure} / {totalTreasure}");

        if (collectedTreasure >= totalTreasure)
            FinishGame(true);
    }

    public void FinishGame(bool win)
    {
        GetTree().ChangeSceneToFile(win ? gameWinUIScenePath : gameOverUIScenePath);
    }
}
