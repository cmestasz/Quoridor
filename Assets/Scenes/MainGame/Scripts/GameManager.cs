using System.Collections.Generic;
using UnityEngine;
using static Validations;
using static Constants;
using System;

public class GameManager : MonoBehaviour
{
    public TileBoard tileBoard;
    public FenceBoard fenceBoard;
    [SerializeField] private PreviewController previewController;
    [SerializeField] private Color[] playerColors;
    private AStar aStar;
    public int turn;
    private Vector2Int[] playerPositions;
    private PlayerStatus[] playersStatus;
    private bool playing = true;

    void Start()
    {
        tileBoard.Init();
        fenceBoard.Init();
        previewController.Init();
        aStar = new AStar(tileBoard.tiles, fenceBoard.tiles);
        StartGame();
    }

    public void StartGame()
    {
        playing = true;
        tileBoard.Clear();
        fenceBoard.Clear();

        playerPositions = PLAYER_POSITIONS.Clone() as Vector2Int[];
        playersStatus = new PlayerStatus[] { new(), new() };

        for (int i = 0; i < playerPositions.Length; i++)
        {
            Tile tile = tileBoard.GetByRelativePos(playerPositions[i]);
            tile.SetType(Tile.PLAYER);
            tile.SetColor(playerColors[i]);
            tile.player = i;
        }
        for (int i = 0; i < PLAYER_DESTINATIONS.Length; i++)
        {
            Tile tile = tileBoard.GetByRelativePos(PLAYER_DESTINATIONS[i]);
            tile.SetType(Tile.DESTINATION);
            tile.SetColor(playerColors[i]);
            tile.player = i;
        }
        previewController.UpdatePreview();
    }

    public bool BuildCurrent(Vector2Int pos)
    {
        return Build(pos, turn, IsCurrentVertical(), false);
    }

    public bool MoveCurrent(Vector2Int dest)
    {
        return Move(dest, turn);
    }

    public bool Build(Vector2Int pos, int player, bool vertical, bool validation)
    {
        if (IsBuildValid(pos, player, vertical))
        {
            Fence fence = fenceBoard.GetByRelativePos(pos);
            fence.Build(vertical);
            if (!validation)
            {
                playersStatus[player].fences--;
                PassTurn();
            }
        }
        return false;
    }

    public bool Move(Vector2Int dest, int player)
    {
        if (IsMoveValid(player, dest))
        {
            if (dest == PLAYER_DESTINATIONS[player])
            {
                playerPositions[player] = dest;
                EndGame(player);
            }

            tileBoard.Swap(playerPositions[player], dest);
            playerPositions[player] = dest;
            tileBoard.GetByRelativePos(dest).player = player;
            PassTurn();
            return true;
        }
        return false;
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
            playersStatus[turn].building = !IsCurrentBuilding();
            previewController.UpdatePreview();
        }
        if (IsCurrentBuilding() && Input.GetKeyDown(KeyCode.V))
        {
            playersStatus[turn].vertical = !IsCurrentVertical();
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
                BuildCurrent(relativePos);
            }
            else
            {
                relativePos = tileBoard.RealToRelativePos(mousePos);
                MoveCurrent(relativePos);
            }
        }
    }

    private void PassTurn()
    {
        turn = (turn + 1) % playerPositions.Length;
        previewController.UpdatePreview();
    }

    private void EndGame(int winner)
    {
        Debug.Log("Player " + winner + " wins!");
        previewController.EndGame();
        playing = false;
    }


    private bool IsMoveValid(int player, Vector2Int dest)
    {
        Vector2Int playerPos = playerPositions[player];
        if (playerPos == dest)
        {
            //Debug.Log("Same tile");
            return true;
        }
        if (!IsTileInBounds(dest.x, dest.y))
        {
            //Debug.Log("Tile out of bounds");
            return false;
        }
        if (!IsUnblocked(dest.x, dest.y, tileBoard.tiles, player))
        {
            //Debug.Log("Tile blocked");
            return false;
        }
        if (!IsReachable(playerPos.x, playerPos.y, dest.x, dest.y))
        {
            //Debug.Log("Tile unreachable");
            return false;
        }
        if (!IsPassable(playerPos.x, playerPos.y, dest.x, dest.y, fenceBoard.tiles, tileBoard.tiles))
        {
            //Debug.Log("Tile impassable");
            return false;
        }
        return true;
    }

    private bool IsBuildValid(Vector2Int pos, int player, bool vertical)
    {
        if (GetPlayerFences(player) <= 0)
        {
            //Debug.Log("No fences left");
            return false;
        }
        Fence fence = fenceBoard.GetByRelativePos(pos);
        if (fence == null)
        {
            //Debug.Log("Fence out of bounds");
            return false;
        }
        if (fence.active)
        {
            //Debug.Log("Fence already built");
            return false;
        }
        for (int i = 0; i < 2; i++)
        {
            Fence neighbor = fenceBoard.GetNeighbor(pos.x, pos.y, vertical ? VERTICAL_NEIGHBORS[i] : HORIZONTAL_NEIGHBORS[i]);
            if (neighbor != null && neighbor.active && neighbor.vertical == vertical)
            {
                //Debug.Log("Fence blocking");
                return false;
            }
        }
        fence.Build(vertical);
        if (!IsPostBuildValid())
        {
            fence.Unbuild();
            //Debug.Log("Fence blocking");
            return false;
        }
        fence.Unbuild();

        return true;
    }

    private bool IsPostBuildValid()
    {
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (aStar.FindPath(playerPositions[i].x, playerPositions[i].y, PLAYER_DESTINATIONS[i].x, PLAYER_DESTINATIONS[i].y, i) == null)
            {
                return false;
            }
        }
        return true;
    }

    public Vector2Int GetPlayerPosition(int player)
    {
        return playerPositions[player];
    }

    private bool IsPlayerVertical(int player)
    {
        return playersStatus[player].vertical;
    }

    private bool IsPlayerBuilding(int player)
    {
        return playersStatus[player].building;
    }

    private int GetPlayerFences(int player)
    {
        return playersStatus[player].fences;
    }

    public bool IsCurrentBuilding()
    {
        return IsPlayerBuilding(turn);
    }

    public bool IsCurrentVertical()
    {
        return IsPlayerVertical(turn);
    }

    public HashSet<Vector2Int> GetCurrentValidMoves()
    {
        return GetValidMoves(turn);
    }

    public HashSet<Tuple<Vector2Int, bool>> GetCurrentValidBuilds()
    {
        return GetValidBuilds(turn);
    }

    public HashSet<Vector2Int> GetValidMoves(int player)
    {
        Vector2Int current = playerPositions[player];
        HashSet<Vector2Int> validMoves = new();
        foreach (Vector2Int move in MOVES)
        {
            Vector2Int dest = new(current.x + move.x, current.y + move.y);
            if (move == Vector2Int.zero || IsMoveValid(player, dest))
            {
                validMoves.Add(dest);
            }
        }
        return validMoves;
    }

    public HashSet<Tuple<Vector2Int, bool>> GetValidBuilds(int player)
    {
        if (GetPlayerFences(player) <= 0)
        {
            return new();
        }
        HashSet<Tuple<Vector2Int, bool>> validBuilds = new();
        for (int i = 0; i < fenceBoard.BoardSize; i++)
        {
            for (int j = 0; j < fenceBoard.BoardSize; j++)
            {
                Vector2Int pos = new(i, j);
                if (IsBuildValid(pos, player, true))
                {
                    validBuilds.Add(new(pos, true));
                }
                if (IsBuildValid(pos, player, false))
                {
                    validBuilds.Add(new(pos, false));
                }
            }
        }
        return validBuilds;
    }
}
