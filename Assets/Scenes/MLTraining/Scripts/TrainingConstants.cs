using UnityEngine;

public static class TrainingConstants
{
    public static Vector2Int[] moves = new Vector2Int[]
    {
        new(0, 2),
        new(-1, 1),
        new(0, 1),
        new(1, 1),
        new(-2, 0),
        new(-1, 0),
        new(0, 0),
        new(1, 0),
        new(2, 0),
        new(-1, -1),
        new(0, -1),
        new(1, -1),
        new(0, -2)
    };
}