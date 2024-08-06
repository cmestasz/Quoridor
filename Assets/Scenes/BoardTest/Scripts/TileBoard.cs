using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : Board<Tile>
{
    public const int SIZE = 9;
    public override int BoardSize => SIZE;

    public void Init()
    {
        InitBoard();
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                Tile tile = tiles[i, j];
                tile.label.text = $"{i}, {j}";
            }
        }
    }

    public void Swap(Vector2Int pos1, Vector2Int pos2)
    {
        Tile tile1 = tiles[pos1.x, pos1.y];
        Tile tile2 = tiles[pos2.x, pos2.y];
        int type = tile1.type;
        tile1.SetType(tile2.type);
        tile2.SetType(type);
    }
}
