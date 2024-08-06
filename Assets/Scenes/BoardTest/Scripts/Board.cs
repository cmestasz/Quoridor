using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class Board<T> : MonoBehaviour where T : MonoBehaviour
{
    [SerializeField] protected GameObject prefab;
    public T[,] tiles;
    public abstract int BoardSize { get; }
    protected const float BOARD_SCALE = 1;
    protected float offset;

    public void InitBoard()
    {
        tiles = new T[BoardSize, BoardSize];
        prefab.transform.localScale = new Vector3(BOARD_SCALE, BOARD_SCALE, 1);

        offset = -(BoardSize * BOARD_SCALE / 2.0f - BOARD_SCALE / 2.0f);
        for (int i = 0; i < BoardSize; i++)
        {
            for (int j = 0; j < BoardSize; j++)
            {
                float x = i * BOARD_SCALE + offset;
                float y = j * BOARD_SCALE + offset;
                T instance = Instantiate(prefab, new Vector3(x, y, transform.position.z), Quaternion.identity, transform).GetComponent<T>();
                tiles[i, j] = instance;
            }
        }
    }

    public T GetByRealPos(Vector3 pos)
    {
        Vector2Int relativePos = RealToRelativePos(pos);
        return tiles[relativePos.x, relativePos.y];
    }

    public T GetByRelativePos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= BoardSize || pos.y < 0 || pos.y >= BoardSize)
        {
            return default;
        }
        return tiles[pos.x, pos.y];
    }

    public Vector2Int RealToRelativePos(Vector3 pos)
    {
        int i = Mathf.RoundToInt((pos.x - offset) / BOARD_SCALE);
        int j = Mathf.RoundToInt((pos.y - offset) / BOARD_SCALE);
        return new Vector2Int(i, j);
    }

    public T GetNeighbor(int i, int j, Vector2Int dir)
    {
        int ni = i + dir.x;
        int nj = j + dir.y;
        if (ni < 0 || ni >= BoardSize || nj < 0 || nj >= BoardSize)
        {
            return default;
        }
        return tiles[ni, nj];
    }
}
