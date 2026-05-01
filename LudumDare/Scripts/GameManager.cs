using Godot;
using System.Collections.Generic;

public partial class GameManager : Node
{
    [ExportCategory("Scene Paths")]
    [Export] private string gameOverUIScenePath;
    [Export] private string gameWinUIScenePath;

    // Game tracking
    private int totalTreasure = -1;
    private int collectedTreasure = 0;
    private List<Node3D> treasureChests = new();

    // References
    private CharacterBody3D player;

    public override void _Ready()
    {
        // Find references in scene
        player = (PlayerController)GetTree().GetFirstNodeInGroup("Player");
    }

    /// <summary>
    /// Gets the distance from the player to the closest chest in the scene.
    /// </summary>
    /// <returns>Distance as a float from the player to the closest treasure chest. If there are no chests left this returns -1f</returns>
    public float GetClosestChestDist()
    {
        // Default distance
        float lowestDist = -1;

        // Loops over every chest in the scene
        foreach (Node3D node in treasureChests)
        {
            float currDist = player.GlobalPosition.DistanceTo(node.GlobalPosition);
            // Check if this is the closest chest - if so keep track of the distance
            if (currDist < lowestDist || lowestDist < 0) // If < 0 then this is the closest
                lowestDist = currDist;
        }

        return lowestDist;
    }

    /// <summary>
    /// Sets the total amount of treasure that spawned in the cave.
    /// </summary>
    /// <param name="amount">Amount of treasure</param>
    public void SetTotalTreasure(int amount)
    {
        totalTreasure = amount;
    }

    /// <summary>
    /// Sets a new list of treasure nodes currently in the scene.
    /// </summary>
    /// <param name="treasureList">List of treasure nodes in the scene.</param>
    public void SetTreasureList(List<Node3D> treasureList)
    {
        treasureChests = treasureList;
    }

    /// <returns>Amount of chests currently in the cave as an int</returns>
    public int GetChestsCount()
    {
        return treasureChests.Count;
    }

    /// <returns>Total amount of chests that spawned in the cave as an int</returns>
    public int GetTotalChests()
    {
        return totalTreasure;
    }

    /// <summary>
    /// Manages what happens when the player has collected a chest. The node for the treasure parameter needs to currently exist in the scene.
    /// </summary>
    /// <param name="treasure">The treasure node to collect</param>
    public void AddTreasure(Node3D treasure)
    {
        if (totalTreasure < 0)
            GD.PrintErr("GameManager: DIDN'T SET totalTreasure (-1)");

        collectedTreasure++;

        // Remove the collected chest from the tracked chests in the scene
        treasureChests.Remove(treasure);

        GD.Print($"GameManager: Total treasure {collectedTreasure} / {totalTreasure}");

        // Check if the player has collected all of the treasure in the cave
        if (collectedTreasure >= totalTreasure)
            FinishGame(true);
    }

    /// <summary>
    /// Stops the game in a win or loss.
    /// </summary>
    /// <param name="win">If the game should end in a win (false for loss)</param>
    public void FinishGame(bool win)
    {
        GetTree().ChangeSceneToFile(win ? gameWinUIScenePath : gameOverUIScenePath);
    }
}
