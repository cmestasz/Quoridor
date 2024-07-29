using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class AStar
{
    private Tile[,] tiles;
    private Fence[,] fences;
    private int tileBoardSize;
    private Vector2Int[] neighbors;

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

    private bool IsTileInBounds(int row, int col)
    {
        return GameManager.IsTileInBounds(row, col);
    }

    private bool IsValid(int row, int col, int destRow, int destCol)
    {
        return GameManager.IsPassable(row, col, destRow, destCol, fences);
    }

    private bool IsUnblocked(int row, int col)
    {
        return GameManager.IsUnblocked(row, col, tiles);
    }

    private bool IsDestination(int row, int col, int destRow, int destCol)
    {
        return GameManager.IsDestination(row, col, destRow, destCol);
    }

    private float GetHValue(int row, int col, int destRow, int destCol)
    {
        return Math.Abs(row - destRow) + Math.Abs(col - destCol);
    }

    public List<Vector2> FindPath(int srcRow, int srcCol, int destRow, int destCol)
    {
        for (int x = 0; x < tileBoardSize; x++)
        {
            for (int y = 0; y < tileBoardSize; y++)
            {
                tiles[x, y].ResetState();
            }
        }

        if (!IsTileInBounds(srcRow, srcCol) || !IsTileInBounds(destRow, destCol))
        {
            return null;
        }

        if (!IsUnblocked(srcRow, srcCol) || !IsUnblocked(destRow, destCol))
        {
            return null;
        }

        if (IsDestination(srcRow, srcCol, destRow, destCol))
        {
            return null;
        }

        bool[,] closedList = new bool[tileBoardSize, tileBoardSize];

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

                if (IsTileInBounds(row, col) && IsValid(i, j, row, col) && !closedList[row, col])
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
            }
        }
        return null;
    }

    private List<Vector2> BuildPath(Tile[,] tiles, int destRow, int destCol)
    {
        List<Vector2> path = new();
        Tile dest = tiles[destRow, destCol];
        while (dest.prev != null)
        {
            path.Add(new Vector2(dest.transform.position.x, dest.transform.position.y));
            Debug.DrawLine(dest.transform.position, dest.prev.transform.position, Color.red, 3.0f);
            dest = dest.prev;
        }
        path.Reverse();
        return path;
    }
}
