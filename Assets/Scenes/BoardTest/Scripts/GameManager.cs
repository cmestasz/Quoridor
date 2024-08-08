using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public TileBoard tileBoard;
    public FenceBoard fenceBoard;
    [SerializeField] private TMP_Text debugText;
    [SerializeField] private PreviewController previewController;
    [SerializeField] private Color[] playerColors;
    private AStar aStar;
    private int curr;
    private Vector2Int[] playerPositions = new Vector2Int[] { new(4, 1), new(4, 7) };
    private Vector2Int[] playerDestinations = new Vector2Int[] { new(4, 8), new(4, 0) };
    private PlayerStatus[] playersStatus = new PlayerStatus[] { new(), new() };
    private Fence lastFence;
    public Vector2Int[] HORIZONTAL_NEIGHBORS = new Vector2Int[] { new(1, 0), new(-1, 0) };
    public Vector2Int[] VERTICAL_NEIGHBORS = new Vector2Int[] { new(0, 1), new(0, -1) };
    private bool playing = true;

    // Start is called before the first frame update
    void Start()
    {
        tileBoard.Init();
        fenceBoard.Init();
        aStar = new AStar(tileBoard.tiles, fenceBoard.tiles);
        for (int i = 0; i < playerPositions.Length; i++)
        {
            Tile tile = tileBoard.GetByRelativePos(playerPositions[i]);
            tile.SetType(Tile.PLAYER);
            tile.SetColor(playerColors[i]);
            tile.player = i;
        }
        for (int i = 0; i < playerDestinations.Length; i++)
        {
            Tile tile = tileBoard.GetByRelativePos(playerDestinations[i]);
            tile.SetType(Tile.DESTINATION);
            tile.SetColor(playerColors[i]);
            tile.player = i;
        }
        previewController.UpdatePreview();
    }

    // Update is called once per frame
    void Update()
    {
        if (playing)
        {
            HandleToggles();
            HandleTurn();
        }
    }

    private void HandleToggles()
    {
        if (Input.GetKeyDown(KeyCode.B))
        {
            playersStatus[curr].building = !IsCurrentBuilding();
            UpdateDebugText();
            previewController.UpdatePreview();
        }
        if (IsCurrentBuilding() && Input.GetKeyDown(KeyCode.V))
        {
            playersStatus[curr].vertical = !IsCurrentVertical();
            UpdateDebugText();
            previewController.UpdatePreview();
        }
    }

    private void HandleTurn()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2Int relativePos;
            if (IsCurrentBuilding())
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
            fence.Build(IsCurrentVertical());
            lastFence = fence;
            if (PostValidateBuild())
            {
                PassTurn();
            }
        }
    }

    private void Move(Vector2Int dest)
    {
        if (IsMoveValid(dest))
        {
            if (dest == playerDestinations[curr])
            {
                tileBoard.GetByRelativePos(dest).SetType(Tile.PLAYER);
                tileBoard.GetByRelativePos(playerPositions[curr]).SetType(Tile.EMPTY);
                debugText.text = $"Player {curr + 1} wins!";
                EndGame();
                return;
            }

            tileBoard.Swap(playerPositions[curr], dest);
            playerPositions[curr] = dest;
            PassTurn();
        }
    }

    private void PassTurn()
    {
        curr = (curr + 1) % playerPositions.Length;
        previewController.UpdatePreview();
        UpdateDebugText();
    }

    private void UpdateDebugText()
    {
        debugText.text = $"Turn: {curr + 1}, Building: {IsCurrentBuilding()}, Vertical: {IsCurrentVertical()}";
    }

    private void EndGame()
    {
        previewController.EndGame();
        playing = false;
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
            Fence neighbor = fenceBoard.GetNeighbor(pos.x, pos.y, IsCurrentVertical() ? VERTICAL_NEIGHBORS[i] : HORIZONTAL_NEIGHBORS[i]);
            if (neighbor != null && neighbor.active && neighbor.vertical == IsCurrentVertical())
            {
                Debug.Log("Invalid position");
                return false;
            }
        }
        return true;
    }

    private bool PostValidateBuild()
    {
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (aStar.FindPath(playerPositions[i].x, playerPositions[i].y, playerDestinations[i].x, playerDestinations[i].y, i) == null)
            {
                Debug.Log($"Player {i + 1} is blocked");
                lastFence.Unbuild();
                return false;
            }
        }
        return true;
    }

    private bool IsMoveValid(Vector2Int dest)
    {
        Vector2Int current = playerPositions[curr];
        if (!IsTileInBounds(dest.x, dest.y))
        {
            Debug.Log("Out of bounds");
            return false;
        }
        if (!IsUnblocked(dest.x, dest.y, tileBoard.tiles, curr))
        {
            Debug.Log("Occupied");
            return false;
        }
        if (!IsReachable(current.x, current.y, dest.x, dest.y))
        {
            Debug.Log("Invalid move");
            return false;
        }
        if (!IsPassable(current.x, current.y, dest.x, dest.y, fenceBoard.tiles, tileBoard.tiles))
        {
            Debug.Log("Blocked");
            return false;
        }
        return true;
    }

    public bool IsCurrentBuilding()
    {
        return playersStatus[curr].building;
    }

    public bool IsCurrentVertical()
    {
        return playersStatus[curr].vertical;
    }

    public Vector2Int GetCurrentPosition()
    {
        return playerPositions[curr];
    }

    public List<Vector2Int> GetValidMoves()
    {
        Vector2Int current = playerPositions[curr];
        List<Vector2Int> validMoves = new();
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector2Int dest = new(current.x + i, current.y + j);
                if (IsTileInBounds(dest.x, dest.y) && IsUnblocked(dest.x, dest.y, tileBoard.tiles, curr) && IsReachable(current.x, current.y, dest.x, dest.y) && IsPassable(current.x, current.y, dest.x, dest.y, fenceBoard.tiles, tileBoard.tiles))
                {
                    validMoves.Add(dest);
                }
            }
        }
        return validMoves;
    }

    public static bool IsPassable(int row, int col, int destRow, int destCol, Fence[,] fences, Tile[,] tiles)
    {
        if (GetDistance(row, col, destRow, destCol) == 2)
        {
            int dirx = 0;
            int diry = 0;

            if (row < destRow)
                dirx = 1;
            else if (row > destRow)
                dirx = -1;

            if (col < destCol)
                diry = 1;
            else if (col > destCol)
                diry = -1;

            return
                (dirx != 0 && tiles[row + dirx, col].type == Tile.PLAYER && IsPassable(row + dirx, col, destRow, destCol, fences, tiles)) ||
                (diry != 0 && tiles[row, col + diry].type == Tile.PLAYER && IsPassable(row, col + diry, destRow, destCol, fences, tiles));
        }

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
        return GetDistance(row, col, destRow, destCol) <= 2;
    }

    public static int GetDistance(int row, int col, int destRow, int destCol)
    {
        return Mathf.Abs(row - destRow) + Mathf.Abs(col - destCol);
    }

    public static bool IsTileInBounds(int row, int col)
    {
        return (row >= 0) && (row < TileBoard.SIZE) && (col >= 0) && (col < TileBoard.SIZE);
    }

    public static bool IsUnblocked(int row, int col, Tile[,] tiles, int player)
    {
        return tiles[row, col].type == Tile.EMPTY || tiles[row, col].player == player;
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
