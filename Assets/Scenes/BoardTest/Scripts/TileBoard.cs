using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileBoard : Board<Tile>
{
    public const int SIZE = 9;
    public override int boardSize => SIZE;

    // Start is called before the first frame update
    void Start()
    {
        InitBoard();
        for (int i = 0; i < boardSize; i++)
        {
            for (int j = 0; j < boardSize; j++)
            {
                Tile tile = tiles[i, j];
                tile.label.text = $"{i}, {j}";
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
