using UnityEngine;

public static class Constants
{
    /* safe to modify */
    public const int TILE_BOARD_SIZE = 9;
    public const int FENCE_BOARD_SIZE = TILE_BOARD_SIZE - 1;
    public const float BOARD_SCALE = 1;


    /* can be modified, as long as... */
    // only 2 players (agents were trained with 2 players)
    // playerPositions[i] is not a destination
    public static readonly Vector2Int[] PLAYER_POSITIONS = new Vector2Int[] { new(4, 1), new(4, 7) };
    public static readonly Vector2Int[] PLAYER_DESTINATIONS = new Vector2Int[] { new(4, 8), new(4, 0) };



    /* do not modify */
    public static readonly Vector2Int[] HORIZONTAL_NEIGHBORS = new Vector2Int[] { new(1, 0), new(-1, 0) };
    public static readonly Vector2Int[] VERTICAL_NEIGHBORS = new Vector2Int[] { new(0, 1), new(0, -1) };
    public static readonly Vector2Int[] MOVES = new Vector2Int[]
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