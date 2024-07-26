using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FenceBoard : MonoBehaviour
{
    public const int BOARD_SIZE = TileBoard.BOARD_SIZE - 1;
    private const float BOARD_SCALE = TileBoard.BOARD_SCALE;
    [SerializeField] private GameObject fencePrefab;
    public Fence[,] fences;
    private float offset;

    // Start is called before the first frame update
    void Start()
    {
        fences = new Fence[BOARD_SIZE, BOARD_SIZE];
        fencePrefab.transform.localScale = new Vector3(BOARD_SCALE, BOARD_SCALE, 1);

        offset = -(BOARD_SIZE * BOARD_SCALE / 2.0f - BOARD_SCALE / 2.0f);
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                float x = i * BOARD_SCALE + offset;
                float y = j * BOARD_SCALE + offset;
                GameObject fence = Instantiate(fencePrefab, new Vector3(x, y, 0), Quaternion.identity, transform);
                fences[i, j] = fence.GetComponent<Fence>();
            }
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public Fence GetByRealPos(Vector3 pos)
    {
        Vector2Int relativePos = RealToRelativePos(pos);
        return fences[relativePos.x, relativePos.y];
    }

    public Fence GetByRelativePos(Vector2Int pos)
    {
        if (pos.x < 0 || pos.x >= BOARD_SIZE || pos.y < 0 || pos.y >= BOARD_SIZE)
        {
            return null;
        }
        return fences[pos.x, pos.y];
    }

    public Vector2Int RealToRelativePos(Vector3 pos)
    {
        int i = Mathf.RoundToInt((pos.x - offset) / BOARD_SCALE);
        int j = Mathf.RoundToInt((pos.y - offset) / BOARD_SCALE);
        return new Vector2Int(i, j);
    }

    public Fence GetNeighbor(int i, int j, Vector2Int dir)
    {
        int ni = i + dir.x;
        int nj = j + dir.y;
        if (ni < 0 || ni >= BOARD_SIZE || nj < 0 || nj >= BOARD_SIZE)
        {
            return null;
        }
        return fences[ni, nj];
    }
}
