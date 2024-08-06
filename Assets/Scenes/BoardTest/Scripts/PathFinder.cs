using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathFinder : MonoBehaviour
{
    [SerializeField] private TileBoard tileBoard;
    [SerializeField] private FenceBoard fenceBoard;
    private const int BOARD_SIZE = TileBoard.SIZE;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            AStar aStar = new AStar(tileBoard.tiles, fenceBoard.tiles);
            List<Vector2Int> path = aStar.FindPath(0, 0, BOARD_SIZE - 1, BOARD_SIZE - 1);
        }
    }
}
