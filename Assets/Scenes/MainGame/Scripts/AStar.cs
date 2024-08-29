using System;
using System.Collections.Generic;
using UnityEngine;
using static Validations;
using static Constants;

public class AStar
{
    private readonly Tile[,] tiles;
    private readonly Fence[,] fences;
    private readonly Vector2Int[] neighbors;

    public AStar(Tile[,] tiles, Fence[,] fences)
    {
        this.tiles = tiles;
        this.fences = fences;
        neighbors = new Vector2Int[]
            {
                new(-1, 0),
                new(1, 0),
                new(0, -1),
                new(0, 1)
            };
    }

    private float GetHValue(int row, int col, int destRow, int destCol)
    {
        return Math.Abs(row - destRow) + Math.Abs(col - destCol);
    }

    public List<Vector2Int> FindPath(int srcRow, int srcCol, int destRow, int destCol, int player)
    {
        for (int x = 0; x < TILE_BOARD_SIZE; x++)
        {
            for (int y = 0; y < TILE_BOARD_SIZE; y++)
            {
                tiles[x, y].ResetState();
            }
        }

        if (IsDestination(srcRow, srcCol, destRow, destCol))
        {
            return null;
        }

        bool[,] closedList = new bool[TILE_BOARD_SIZE, TILE_BOARD_SIZE];

        int i = srcRow, j = srcCol;
        tiles[i, j].f = 0.0f;
        tiles[i, j].g = 0.0f;
        tiles[i, j].h = 0.0f;

        Comparer<double> comparer = Comparer<double>.Create((a, b) => a.CompareTo(b));
        PriorityQueue<double, Vector2Int> openList = new(comparer);
        openList.Enqueue(0.0f, new Vector2Int(i, j));

        while (openList.GetCount() > 0)
        {
            KeyValuePair<double, Vector2Int> p = openList.Dequeue();

            i = p.Value.x;
            j = p.Value.y;
            closedList[i, j] = true;

            foreach (Vector2Int neighbor in neighbors)
            {
                int row = i + neighbor.x;
                int col = j + neighbor.y;

                if (IsTileInBounds(row, col) && !IsUnblocked(row, col, tiles, player) && !closedList[row, col])
                {
                    closedList[row, col] = true;
                    foreach (Vector2Int specialNeighbor in neighbors)
                    {
                        int specialRow = row + specialNeighbor.x;
                        int specialCol = col + specialNeighbor.y;
                        List<Vector2Int> specialPath = ManageNeighbor(i, j, specialRow, specialCol, destRow, destCol, closedList, openList);
                        if (specialPath != null)
                            return specialPath;
                    }
                }

                List<Vector2Int> path = ManageNeighbor(i, j, row, col, destRow, destCol, closedList, openList);
                if (path != null)
                    return path;
            }
        }
        return null;
    }

    private List<Vector2Int> ManageNeighbor(int i, int j, int row, int col, int destRow, int destCol, bool[,] closedList, PriorityQueue<double, Vector2Int> openList)
    {
        if (IsTileInBounds(row, col) && IsPassable(i, j, row, col, fences, tiles) && !closedList[row, col])
        {
            if (IsDestination(row, col, destRow, destCol))
            {
                tiles[row, col].prev = tiles[i, j];
                return BuildPath(tiles, destRow, destCol);
            }

            float gNew = tiles[i, j].g + 1.0f;
            float hNew = GetHValue(row, col, destRow, destCol);
            float fNew = gNew + hNew;

            if (tiles[row, col].f == float.MaxValue || tiles[row, col].f > fNew)
            {
                openList.Enqueue(fNew, new Vector2Int(row, col));
                tiles[row, col].f = fNew;
                tiles[row, col].g = gNew;
                tiles[row, col].h = hNew;
                tiles[row, col].prev = tiles[i, j];
            }
        }
        return null;
    }

    private List<Vector2Int> BuildPath(Tile[,] tiles, int destRow, int destCol)
    {
        List<Vector2Int> path = new();
        Tile dest = tiles[destRow, destCol];
        int i = 0;
        while (dest.prev != null)
        {
            Color color = i % 2 == 0 ? Color.red : Color.blue;
            path.Add(new Vector2Int((int)dest.transform.position.x, (int)dest.transform.position.y));
            //Debug.DrawLine(dest.transform.position, dest.prev.transform.position, color, 3.0f);
            dest = dest.prev;
            i++;
        }
        return path;
    }
}
