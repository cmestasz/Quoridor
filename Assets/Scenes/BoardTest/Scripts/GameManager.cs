using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private TileBoard tileBoard;
    [SerializeField] private FenceBoard fenceBoard;
    private int currentPlayer;
    private Vector2Int[] playerPositions;
    private Vector2Int[] playerDestinations;
    private bool building;
    private bool vertical;
    public Vector2Int[] HORIZONTAL_NEIGHBORS = new Vector2Int[] { new(1, 0), new(-1, 0) };
    public Vector2Int[] VERTICAL_NEIGHBORS = new Vector2Int[] { new(0, 1), new(0, -1) };

    // Start is called before the first frame update
    void Start()
    {
        playerPositions = new Vector2Int[]
        {
            new(4, 0),
            new(4, 8)
        };
        playerDestinations = new Vector2Int[]
        {
            new(4, 8),
            new(4, 0)
        };

        tileBoard.Init();
        fenceBoard.Init();
        for (int i = 0; i < playerPositions.Length; i++)
        {
            tileBoard.tiles[playerPositions[i].x, playerPositions[i].y].SetType(Tile.PLAYER);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            building = !building;
            Debug.Log("Building: " + building);
        }
        if (building && Input.GetKeyDown(KeyCode.V))
        {
            vertical = !vertical;
            Debug.Log("Vertical: " + vertical);
        }
        HandleTurn();
    }

    private void HandleTurn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int relativePos;
            if (building)
            {
                relativePos = fenceBoard.RealToRelativePos(mousePos);
                Build(relativePos);
            }
            else
            {
                relativePos = tileBoard.RealToRelativePos(mousePos);
                Move(relativePos);
            }
        }
    }

    private void Build(Vector2Int pos)
    {
        if (IsBuildValid(pos))
        {
            Fence fence = fenceBoard.GetByRelativePos(pos);
            fence.Build(vertical);
            currentPlayer = (currentPlayer + 1) % playerPositions.Length;
            Debug.Log("Turn: " + (currentPlayer + 1));
        }
    }

    private void Move(Vector2Int dest)
    {
        if (IsMoveValid(dest))
        {
            tileBoard.Swap(playerPositions[currentPlayer], dest);
            playerPositions[currentPlayer] = dest;
            if (playerPositions[currentPlayer] == playerDestinations[currentPlayer])
            {
                Debug.Log("Player " + (currentPlayer + 1) + " wins!");
            }
            currentPlayer = (currentPlayer + 1) % playerPositions.Length;
            Debug.Log("Turn: " + (currentPlayer + 1));
        }
    }

    private bool IsBuildValid(Vector2Int pos)
    {
        Fence fence = fenceBoard.GetByRelativePos(pos);
        if (fence == null)
        {
            Debug.Log("Invalid position");
            return false;
        }
        if (fence.active)
        {
            Debug.Log("Already built");
            return false;
        }
        for (int i = 0; i < 2; i++)
        {
            Fence neighbor = fenceBoard.GetNeighbor(pos.x, pos.y, vertical ? VERTICAL_NEIGHBORS[i] : HORIZONTAL_NEIGHBORS[i]);
            if (neighbor != null && neighbor.active && neighbor.vertical == vertical)
            {
                Debug.Log("Invalid position");
                return false;
            }
        }
        return true;
    }

    private bool IsMoveValid(Vector2Int dest)
    {
        Vector2Int current = playerPositions[currentPlayer];
        if (!IsTileInBounds(dest.x, dest.y))
        {
            Debug.Log("Out of bounds");
            return false;
        }
        if (!IsUnblocked(dest.x, dest.y, tileBoard.tiles))
        {
            Debug.Log("Occupied");
            return false;
        }
        if (!IsReachable(current.x, current.y, dest.x, dest.y))
        {
            Debug.Log("Invalid move");
            return false;
        }
        if (!IsPassable(current.x, current.y, dest.x, dest.y, fenceBoard.tiles))
        {
            Debug.Log("Blocked");
            return false;
        }
        return true;
    }

    public static bool IsPassable(int row, int col, int destRow, int destCol, Fence[,] fences)
    {
        bool vertical = row == destRow;
        int minRow = Mathf.Min(row, destRow);
        int minCol = Mathf.Min(col, destCol);
        int i1 = vertical ? row - 1 : minRow;
        int j1 = vertical ? minCol : col - 1;
        int i2 = vertical ? row : minRow;
        int j2 = vertical ? minCol : col;

        Fence fence1 = IsFenceInBounds(i1, j1) ? fences[i1, j1] : null;
        Fence fence2 = IsFenceInBounds(i2, j2) ? fences[i2, j2] : null;
        return
            (fence1 == null || !fence1.active || fence1.vertical == vertical) &&
            (fence2 == null || !fence2.active || fence2.vertical == vertical)
        ;
    }

    public static bool IsReachable(int row, int col, int destRow, int destCol)
    {
        return Mathf.Abs(row - destRow) + Mathf.Abs(col - destCol) == 1;
    }

    public static bool IsTileInBounds(int row, int col)
    {
        return (row >= 0) && (row < TileBoard.SIZE) && (col >= 0) && (col < TileBoard.SIZE);
    }

    public static bool IsUnblocked(int row, int col, Tile[,] tiles)
    {
        return tiles[row, col].type == Tile.EMPTY;
    }

    public static bool IsDestination(int row, int col, int destRow, int destCol)
    {
        return row == destRow && col == destCol;
    }

    public static bool IsFenceInBounds(int row, int col)
    {
        return (row >= 0) && (row < FenceBoard.SIZE) && (col >= 0) && (col < FenceBoard.SIZE);
    }

}
