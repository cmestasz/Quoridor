using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ContinuousGameManager : MonoBehaviour
{
    public TileBoard tileBoard;
    public FenceBoard fenceBoard;
    [SerializeField] private Color[] playerColors;
    [SerializeField] private AgentManager agentManager;
    private AStar aStar;
    private Vector2Int[] playerPositions;
    private Vector2Int[] playerDestinations;
    private PlayerStatus[] playersStatus;
    private Fence lastFence;
    public Vector2Int[] HORIZONTAL_NEIGHBORS = new Vector2Int[] { new(1, 0), new(-1, 0) };
    public Vector2Int[] VERTICAL_NEIGHBORS = new Vector2Int[] { new(0, 1), new(0, -1) };
    public bool playing = false;
    public int winner = -1;

    void Awake()
    {
        tileBoard.Init();
        fenceBoard.Init();
        aStar = new AStar(tileBoard.tiles, fenceBoard.tiles);
    }

    public void StartGame()
    {
        playing = true;
        winner = -1;
        tileBoard.Clear();
        fenceBoard.Clear();
        playerPositions = new Vector2Int[] { new(4, 1), new(4, 7) };
        playerDestinations = new Vector2Int[] { new(4, 8), new(4, 0) };
        playersStatus = new PlayerStatus[] { new(), new() };
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
    }

    public bool Build(Vector2Int pos, int player, bool vertical, bool validation)
    {
        if (IsBuildValid(pos, player))
        {
            Fence fence = fenceBoard.GetByRelativePos(pos);
            fence.Build(vertical);
            lastFence = fence;

            if (PostValidateBuild())
            {
                if (!validation)
                {
                    playersStatus[player].fences--;
                }
                return true;
            }
        }
        return false;
    }

    public bool Move(Vector2Int dest, int player)
    {
        if (IsMoveValid(player, dest))
        {
            if (dest == playerDestinations[player])
            {
                EndGame(player);
            }

            tileBoard.Swap(playerPositions[player], dest);
            playerPositions[player] = dest;
            return true;
        }
        return false;
    }

    private bool IsBuildValid(Vector2Int pos, int player)
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
            Fence neighbor = fenceBoard.GetNeighbor(pos.x, pos.y, IsPlayerVertical(player) ? VERTICAL_NEIGHBORS[i] : HORIZONTAL_NEIGHBORS[i]);
            if (neighbor != null && neighbor.active && neighbor.vertical == IsPlayerVertical(player))
            {
                //Debug.Log("Fence blocking");
                return false;
            }
        }
        return true;
    }

    private bool IsMoveValid(int player, Vector2Int dest)
    {
        Vector2Int playerPos = playerPositions[player];
        if (!IsTileInBounds(dest.x, dest.y))
        {
            //Debug.Log("Tile out of bounds");
            return false;
        }
        if (!IsUnblocked(dest.x, dest.y, player))
        {
            //Debug.Log("Tile blocked");
            return false;
        }
        if (!IsReachable(playerPos.x, playerPos.y, dest.x, dest.y))
        {
            //Debug.Log("Tile unreachable");
            return false;
        }
        if (!IsPassable(playerPos.x, playerPos.y, dest.x, dest.y))
        {
            //Debug.Log("Tile impassable");
            return false;
        }
        return true;
    }

    private bool PostValidateBuild()
    {
        if (!IsPostBuildValid())
        {
            //Debug.Log("Blocking players path");
            lastFence.Unbuild();
            return false;
        }
        return true;
    }

    private bool IsPostBuildValid()
    {
        for (int i = 0; i < playerPositions.Length; i++)
        {
            if (aStar.FindPath(playerPositions[i].x, playerPositions[i].y, playerDestinations[i].x, playerDestinations[i].y, i) == null)
            {
                return false;
            }
        }
        return true;
    }

    public void EndGame(int winner)
    {
        playing = false;
        this.winner = winner;
    }

    public Vector2Int GetPlayerPosition(int player)
    {
        return playerPositions[player];
    }

    public Vector2Int GetPlayerDestination(int player)
    {
        return playerDestinations[player];
    }

    public PlayerStatus GetPlayerStatus(int player)
    {
        return playersStatus[player];
    }

    public List<Vector2Int> GetValidMoves(int player)
    {
        Vector2Int current = playerPositions[player];
        List<Vector2Int> validMoves = new();
        for (int i = -2; i <= 2; i++)
        {
            for (int j = -2; j <= 2; j++)
            {
                Vector2Int dest = new(current.x + i, current.y + j);
                if (IsTileInBounds(dest.x, dest.y) && IsUnblocked(dest.x, dest.y, player) && IsReachable(current.x, current.y, dest.x, dest.y) && IsPassable(current.x, current.y, dest.x, dest.y))
                {
                    validMoves.Add(dest);
                }
            }
        }
        return validMoves;
    }

    public List<Tuple<Vector2Int, bool>> GetValidBuilds(int player)
    {
        if (GetPlayerFences(player) <= 0)
        {
            return new();
        }
        List<Tuple<Vector2Int, bool>> validBuilds = new();
        for (int i = 0; i < fenceBoard.BoardSize; i++)
        {
            for (int j = 0; j < fenceBoard.BoardSize; j++)
            {
                Vector2Int pos = new(i, j);
                if (Build(pos, player, true, true))
                {
                    if (IsPostBuildValid())
                    {
                        validBuilds.Add(new(pos, true));
                    }
                    lastFence.Unbuild();
                }
                if (Build(pos, player, false, true))
                {
                    if (IsPostBuildValid())
                    {
                        validBuilds.Add(new(pos, false));
                    }
                    lastFence.Unbuild();
                }
            }
        }
        return validBuilds;
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

    private bool IsReachable(int row, int col, int destRow, int destCol)
    {
        return GameManager.IsReachable(row, col, destRow, destCol);
    }

    private bool IsPassable(int row, int col, int destRow, int destCol)
    {
        return GameManager.IsPassable(row, col, destRow, destCol, fenceBoard.tiles, tileBoard.tiles);
    }

    private int GetDistance(int row, int col, int destRow, int destCol)
    {
        return GameManager.GetDistance(row, col, destRow, destCol);
    }

    private bool IsTileInBounds(int row, int col)
    {
        return GameManager.IsTileInBounds(row, col);
    }

    private bool IsFenceInBounds(int row, int col)
    {
        return GameManager.IsFenceInBounds(row, col);
    }

    private bool IsUnblocked(int row, int col, int player)
    {
        return GameManager.IsUnblocked(row, col, tileBoard.tiles, player);
    }

    private bool IsDestination(int row, int col, int destRow, int destCol)
    {
        return GameManager.IsDestination(row, col, destRow, destCol);
    }

}
