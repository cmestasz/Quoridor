using UnityEngine;
using static Constants;

public class Validations
{
    public static bool IsPassable(int row, int col, int destRow, int destCol, Fence[,] fences, Tile[,] tiles)
    {
        if (GetDistance(row, col, destRow, destCol) == 2)
        {
            int dirx = 0;
            int diry = 0;

            if (row < destRow)
                dirx = 1;
            else if (row > destRow)
                dirx = -1;

            if (col < destCol)
                diry = 1;
            else if (col > destCol)
                diry = -1;

            return
                (dirx != 0 && tiles[row + dirx, col].type == Tile.PLAYER && IsPassable(row, col, row + dirx, col, fences, tiles) && IsPassable(row + dirx, col, destRow, destCol, fences, tiles)) ||
                (diry != 0 && tiles[row, col + diry].type == Tile.PLAYER && IsPassable(row, col, row, col + diry, fences, tiles) && IsPassable(row, col + diry, destRow, destCol, fences, tiles));
        }

        bool vertical = row == destRow;
        int minRow = Mathf.Min(row, destRow);
        int minCol = Mathf.Min(col, destCol);
        int i1 = vertical ? row - 1 : minRow;
        int j1 = vertical ? minCol : col - 1;
        int i2 = vertical ? row : minRow;
        int j2 = vertical ? minCol : col;

        Fence fence1 = IsFenceInBounds(i1, j1) ? fences[i1, j1] : null;
        Fence fence2 = IsFenceInBounds(i2, j2) ? fences[i2, j2] : null;
        return
            (fence1 == null || !fence1.active || fence1.vertical == vertical) &&
            (fence2 == null || !fence2.active || fence2.vertical == vertical)
        ;
    }

    public static bool IsReachable(int row, int col, int destRow, int destCol)
    {
        return GetDistance(row, col, destRow, destCol) <= 2;
    }

    public static int GetDistance(int row, int col, int destRow, int destCol)
    {
        return Mathf.Abs(row - destRow) + Mathf.Abs(col - destCol);
    }

    public static bool IsTileInBounds(int row, int col)
    {
        return (row >= 0) && (row < TILE_BOARD_SIZE) && (col >= 0) && (col < TILE_BOARD_SIZE);
    }

    public static bool IsUnblocked(int row, int col, Tile[,] tiles, int player)
    {
        return tiles[row, col].type == Tile.EMPTY || tiles[row, col].player == player;
    }

    public static bool IsDestination(int row, int col, int destRow, int destCol)
    {
        return row == destRow && col == destCol;
    }

    public static bool IsFenceInBounds(int row, int col)
    {
        return (row >= 0) && (row < FENCE_BOARD_SIZE) && (col >= 0) && (col < FENCE_BOARD_SIZE);
    }
}