using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TileBoard tileBoard;
    [SerializeField] private FenceBoard fenceBoard;
    private int currentPlayer;
    private Vector2Int[] playerPositions;

    // Start is called before the first frame update
    void Start()
    {
        playerPositions = new Vector2Int[]
        {
            new(4, 0),
            new(4, 8)
        };
        currentPlayer = 0;

        for (int i = 0; i < playerPositions.Length; i++)
        {
            tileBoard.tiles[playerPositions[i].x, playerPositions[i].y].SetType(Tile.PLAYER);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
