using Godot;
using System;

public partial class GameManager : Node
{
    private int totalTreasure = -1;

    private int collectedTreasure = 0;

    public void SetTotalTreasure(int amount)
    {
        totalTreasure = amount;
    }

    public void AddTreasure()
    {
        if (totalTreasure < 0)
            GD.PrintErr("GameManager: DIDN'T SET totalTreasure (-1)");

        collectedTreasure++;

        if (collectedTreasure >= totalTreasure)
            FinishGame();
    }

    public void FinishGame()
    {
        // Stop systems and show game over
    }
}
