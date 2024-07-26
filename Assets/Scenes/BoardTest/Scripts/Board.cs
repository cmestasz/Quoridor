using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Board : MonoBehaviour
{
    public const int BOARD_SIZE = 9;
    private const int BOARD_SCALE = 1;
    [SerializeField] private GameObject tilePrefab;
    private Tile[,] tiles;

    // Start is called before the first frame update
    void Start()
    {
        tiles = new Tile[BOARD_SIZE, BOARD_SIZE];
        tilePrefab.transform.localScale = new Vector3(BOARD_SCALE, 1, BOARD_SCALE);

        float offset = BOARD_SIZE * BOARD_SCALE / 2.0f - BOARD_SCALE / 2.0f;
        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                float x = i * BOARD_SCALE - offset;
                float y = j * BOARD_SCALE - offset;
                Tile tile = Instantiate(tilePrefab, new Vector3(x, y, 0), Quaternion.identity, transform).GetComponent<Tile>();
                tiles[i, j] = tile;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AStar aStar = new AStar(tiles);
            List<Vector2> path = aStar.FindPath(0, 0, BOARD_SIZE - 1, BOARD_SIZE - 1);
        }
    }
}
