using UnityEngine;
using static Constants;

public class TileBoard : Board<Tile>
{
    public override int BoardSize => TILE_BOARD_SIZE;

    public void Init()
    {
        InitBoard();
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                Tile tile = tiles[i, j];
                //tile.label.text = $"{i}, {j}";
            }
        }
    }

    public void Swap(Vector2Int pos1, Vector2Int pos2)
    {
        Tile tile1 = tiles[pos1.x, pos1.y];
        Tile tile2 = tiles[pos2.x, pos2.y];
        int type = tile1.type;
        Color color = tile1.color;
        tile1.SetType(tile2.type);
        tile2.SetType(type);
        tile1.SetColor(tile2.color);
        tile2.SetColor(color);
    }

    public void Clear()
    {
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                Tile tile = tiles[i, j];
                tile.ResetState();
                tile.SetType(Tile.EMPTY);
                tile.SetColor(Color.white);
            }
        }
    }
}
