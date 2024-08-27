using System;
using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;

public class PlayerAgent : Agent
{
    [SerializeField] private ContinuousGameManager gameManager;
    [SerializeField] private int player;
    private int tileBoardSize = TileBoard.SIZE;
    private int fenceBoardSize = FenceBoard.SIZE;
    private int totalTiles;
    private int totalFences;
    public enum BoardState { Empty, PlayerPos, EnemyPos, PlayerDest, EnemyDest, Fence };
    public const int BOARD_STATES = 5;
    private BoardState[,] fullBoard;


    public void Init()
    {
        totalTiles = tileBoardSize * tileBoardSize;
        totalFences = fenceBoardSize * fenceBoardSize;

        int fullSize = tileBoardSize + fenceBoardSize;
        fullBoard = new BoardState[fullSize, fullSize];
    }

    public override void OnEpisodeBegin()
    {
    }

    public void EndGame()
    {
        if (player == gameManager.winner)
            SetReward(1f);
        else
            SetReward(-1f);
        EndEpisode();
    }

    public void UpdateVisualObservation()
    {
        fullBoard = BuildBoard();
    }

    public int FillVectorObservation(ObservationWriter writer)
    {
        float playerFences = gameManager.GetPlayerStatus(player).fences;
        playerFences = Normalize(playerFences, 0, PlayerStatus.MAX_FENCES);
        float enemyFences = gameManager.GetPlayerStatus(1 - player).fences;
        enemyFences = Normalize(enemyFences, 0, PlayerStatus.MAX_FENCES);

        List<float> obs = new() { playerFences, enemyFences };
        writer.AddList(obs);
        return obs.Count;
    }

    private void DebugBoard()
    {
        string board = "";
        for (int i = fullBoard.GetLength(0) - 1; i >= 0 ; i--)
        {
            for (int j = 0; j < fullBoard.GetLength(1); j++)
            {
                board += fullBoard[j, i] switch
                {
                    BoardState.PlayerPos => "P",
                    BoardState.EnemyPos => "E",
                    BoardState.PlayerDest => "D",
                    BoardState.EnemyDest => "X",
                    BoardState.Fence => "F",
                    BoardState.Empty => "O",
                    _ => "/",
                };
            }
            board += "\n";
        }
        Debug.Log($"Player {player} board:");
        Debug.Log(board);
    }

    public int FillVisualObservation(ObservationWriter writer)
    {
        // DebugBoard();
        int written = 0;
        for (int i = 0; i < fullBoard.GetLength(0); i++)
        {
            for (int j = 0; j < fullBoard.GetLength(1); j++)
            {
                for (int c = 0; c < BOARD_STATES; c++)
                {
                    float value = fullBoard[i, j] == (BoardState)c + 1 ? 1f : 0f;
                    writer[c, j, i] = value;
                    written++;
                }
            }

        }
        return written;
    }

    private BoardState[,] BuildBoard()
    {
        int size = tileBoardSize + fenceBoardSize;
        BoardState[,] board = new BoardState[size, size];

        Vector2Int playerPos = gameManager.GetPlayerPosition(player);
        int playerX = player == 0 ? playerPos.x : ReverseValue(playerPos.x, tileBoardSize);
        int playerY = player == 0 ? playerPos.y : ReverseValue(playerPos.y, tileBoardSize);
        board[playerX * 2, playerY * 2] = BoardState.PlayerPos;

        Vector2Int playerDest = gameManager.GetPlayerDestination(player);
        int playerDestX = player == 0 ? playerDest.x : ReverseValue(playerDest.x, tileBoardSize);
        int playerDestY = player == 0 ? playerDest.y : ReverseValue(playerDest.y, tileBoardSize);
        board[playerDestX * 2, playerDestY * 2] = BoardState.PlayerDest;

        Vector2Int enemyPos = gameManager.GetPlayerPosition(1 - player);
        int enemyX = player == 0 ? enemyPos.x : ReverseValue(enemyPos.x, tileBoardSize);
        int enemyY = player == 0 ? enemyPos.y : ReverseValue(enemyPos.y, tileBoardSize);
        board[enemyX * 2, enemyY * 2] = BoardState.EnemyPos;

        Vector2Int enemyDest = gameManager.GetPlayerDestination(1 - player);
        int enemyDestX = player == 0 ? enemyDest.x : ReverseValue(enemyDest.x, tileBoardSize);
        int enemyDestY = player == 0 ? enemyDest.y : ReverseValue(enemyDest.y, tileBoardSize);
        board[enemyDestX * 2, enemyDestY * 2] = BoardState.EnemyDest;

        Fence[,] fences = gameManager.fenceBoard.tiles;
        for (int i = 0; i < fenceBoardSize; i++)
        {
            for (int j = 0; j < fenceBoardSize; j++)
            {
                if (fences[i, j].active)
                {
                    int x = player == 0 ? i : ReverseValue(i, fenceBoardSize);
                    int y = player == 0 ? j : ReverseValue(j, fenceBoardSize);
                    int ox = x * 2 + 1;
                    int oy = y * 2 + 1;

                    if (fences[i, j].vertical)
                    {
                        board[ox, oy] = BoardState.Fence;
                        board[ox, oy + 1] = BoardState.Fence;
                        board[ox, oy - 1] = BoardState.Fence;
                    }
                    else
                    {
                        board[ox, oy] = BoardState.Fence;
                        board[ox + 1, oy] = BoardState.Fence;
                        board[ox - 1, oy] = BoardState.Fence;
                    }
                }
            }
        }

        return board;
    }

    // 0    -   63  :   build vertical fence at position
    // 64   -   127 :   build horizontal fence at position
    // 128  -   140 :   move to position
    public void Act(int action)
    {
        bool valid;
        if (action <= 63)
        {
            Vector2Int pos = IndexToVector2Int(action, fenceBoardSize);
            pos = player == 0 ? pos : ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, true, false);
        }
        else if (action <= 127)
        {
            Vector2Int pos = IndexToVector2Int(action - 64, fenceBoardSize);
            pos = player == 0 ? pos : ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, false, false);
        }
        else
        {
            Vector2Int pos;
            if (player == 0)
                pos = TrainingConstants.moves[action - 128] + gameManager.GetPlayerPosition(player);
            else
                pos = InvertVector(TrainingConstants.moves[action - 128]) + gameManager.GetPlayerPosition(player);
            valid = gameManager.Move(pos, player);
        }

        if (!valid)
            Debug.LogError($"Player {player} made an invalid move: {action}");
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        List<Vector2Int> validMoves = gameManager.GetValidMoves(player);
        List<Tuple<Vector2Int, bool>> validBuilds = gameManager.GetValidBuilds(player);

        HashSet<Vector2Int> validMoveSet = new(validMoves);
        HashSet<Vector2Int> validVBuildSet = new();
        HashSet<Vector2Int> validHBuildSet = new();
        foreach (Tuple<Vector2Int, bool> build in validBuilds)
        {
            if (build.Item2)
                validVBuildSet.Add(build.Item1);
            else
                validHBuildSet.Add(build.Item1);
        }

        for (int i = 0; i < 64; i++)
        {
            Vector2Int pos = IndexToVector2Int(i, fenceBoardSize);
            if (!validVBuildSet.Contains(pos))
            {
                int idx = player == 0 ? i : Vector2IntToIndex(ReverseVector(pos, fenceBoardSize), fenceBoardSize);
                // Debug.Log($"Disabling vertical fence at {pos} (action {idx}) for player {player}");
                actionMask.SetActionEnabled(-1, idx, false);
            }
        }
        for (int i = 64; i < 128; i++)
        {
            Vector2Int pos = IndexToVector2Int(i - 64, fenceBoardSize);
            if (!validHBuildSet.Contains(pos))
            {
                int idx = player == 0 ? i : 64 + Vector2IntToIndex(ReverseVector(pos, fenceBoardSize), fenceBoardSize);
                // Debug.Log($"Disabling horizontal fence at {pos} (action {idx}) for player {player}");
                actionMask.SetActionEnabled(-1, idx, false);
            }
        }
        int movesLength = TrainingConstants.moves.Length;
        for (int i = 0; i < movesLength; i++)
        {
            Vector2Int move;
            if (player == 0)
                move = TrainingConstants.moves[i] + gameManager.GetPlayerPosition(player);
            else
                move = TrainingConstants.moves[movesLength - i - 1] + gameManager.GetPlayerPosition(player);
            if (!validMoveSet.Contains(move))
            {
                int idx = 128 + i;
                // Debug.Log($"Disabling move to {move} (action {idx}) for player {player}");
                actionMask.SetActionEnabled(-1, idx, false);
            }
        }
    }


    private int Vector2IntToIndex(Vector2Int pos, int height)
    {
        return pos.x * height + pos.y;
    }

    private Vector2Int IndexToVector2Int(int index, int height)
    {
        return new Vector2Int(index / height, index % height);
    }

    private HashSet<Vector2Int> ReverseVectors(HashSet<Vector2Int> vectors, int size)
    {
        HashSet<Vector2Int> reversed = new();
        foreach (Vector2Int vector in vectors)
        {
            reversed.Add(ReverseVector(vector, size));
        }
        return reversed;
    }

    private Vector2Int ReverseVector(Vector2Int vector, int size)
    {
        return new Vector2Int(ReverseValue(vector.x, size), ReverseValue(vector.y, size));
    }

    private int ReverseValue(int value, int size)
    {
        return size - value - 1;
    }


    private Vector2Int InvertVector(Vector2Int vector)
    {
        return new Vector2Int(-vector.x, -vector.y);
    }

    private float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

}
