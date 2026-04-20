using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    [Export] private string gameOverUIScenePath;
    [Export] private string gameWinUIScenePath;

    private int totalTreasure = -1;
    private int collectedTreasure = 0;

    private List<Node3D> treasureChests = new();
    private CharacterBody3D player;

    public override void _Ready()
    {
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
    }

    public void SetTotalTreasure(int amount)
    {
        totalTreasure = amount;
    }

    public void SetTreasureList(List<Node3D> treasureList)
    {
        treasureChests = treasureList;
    }

    public float GetClosestChestDist()
    {
        float lowestDist = -1;

        foreach (Node3D node in treasureChests)
        {
            float currDist = player.GlobalPosition.DistanceTo(node.GlobalPosition);
            if (currDist < lowestDist || lowestDist < 0)
                lowestDist = currDist;
        }

        return lowestDist;
    }

    public int GetChestsCount()
    {
        return treasureChests.Count;
    }

    public int GetTotalChests()
    {
        return totalTreasure;
    }

    public void AddTreasure(Node3D treasure)
    {
        if (totalTreasure < 0)
            GD.PrintErr("GameManager: DIDN'T SET totalTreasure (-1)");

        collectedTreasure++;

        treasureChests.Remove(treasure);

        GD.Print($"GameManager: Total treasure {collectedTreasure} / {totalTreasure}");

        if (collectedTreasure >= totalTreasure)
            FinishGame(true);
    }

    public void FinishGame(bool win)
    {
        GetTree().ChangeSceneToFile(win ? gameWinUIScenePath : gameOverUIScenePath);
    }
}
