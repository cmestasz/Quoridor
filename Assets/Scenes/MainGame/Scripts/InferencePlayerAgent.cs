using System;
using System.Collections.Generic;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using UnityEngine;
using static Constants;

// only for playing!! the training one is in the MLTraining folder
public class InferencePlayerAgent : Agent
{
    private GameManager gameManager;
    private int player;
    public enum BoardState { Empty, PlayerPos, EnemyPos, PlayerDest, EnemyDest, Fence };
    public const int BOARD_STATES = 6;
    private BoardState[,] fullBoard;

    public void Init(int player)
    {
        this.player = player;
        int fullSize = TILE_BOARD_SIZE + FENCE_BOARD_SIZE;
        fullBoard = new BoardState[fullSize, fullSize];
        gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        fullBoard = BuildBoard();
        for (int i = fullBoard.GetLength(0) - 1; i >= 0; i--)
        {
            for (int j = 0; j < fullBoard.GetLength(1); j++)
            {
                sensor.AddOneHotObservation((int)fullBoard[j, i], BOARD_STATES);
            }
        }

        float playerFences = gameManager.GetPlayerStatus(player).fences;
        playerFences = Normalize(playerFences, 0, PlayerStatus.MAX_FENCES);
        float enemyFences = gameManager.GetPlayerStatus(1 - player).fences;
        enemyFences = Normalize(enemyFences, 0, PlayerStatus.MAX_FENCES);
        sensor.AddObservation(playerFences);
        sensor.AddObservation(enemyFences);
    }

    private BoardState[,] BuildBoard()
    {
        int size = TILE_BOARD_SIZE + FENCE_BOARD_SIZE;
        BoardState[,] board = new BoardState[size, size];

        Vector2Int playerPos = gameManager.GetPlayerPosition(player);
        int playerX = player == 0 ? playerPos.x : ReverseValue(playerPos.x, TILE_BOARD_SIZE);
        int playerY = player == 0 ? playerPos.y : ReverseValue(playerPos.y, TILE_BOARD_SIZE);
        board[playerX * 2, playerY * 2] = BoardState.PlayerPos;

        Vector2Int playerDest = gameManager.GetPlayerDestination(player);
        int playerDestX = player == 0 ? playerDest.x : ReverseValue(playerDest.x, TILE_BOARD_SIZE);
        int playerDestY = player == 0 ? playerDest.y : ReverseValue(playerDest.y, TILE_BOARD_SIZE);
        board[playerDestX * 2, playerDestY * 2] = BoardState.PlayerDest;

        Vector2Int enemyPos = gameManager.GetPlayerPosition(1 - player);
        int enemyX = player == 0 ? enemyPos.x : ReverseValue(enemyPos.x, TILE_BOARD_SIZE);
        int enemyY = player == 0 ? enemyPos.y : ReverseValue(enemyPos.y, TILE_BOARD_SIZE);
        board[enemyX * 2, enemyY * 2] = BoardState.EnemyPos;

        Vector2Int enemyDest = gameManager.GetPlayerDestination(1 - player);
        int enemyDestX = player == 0 ? enemyDest.x : ReverseValue(enemyDest.x, TILE_BOARD_SIZE);
        int enemyDestY = player == 0 ? enemyDest.y : ReverseValue(enemyDest.y, TILE_BOARD_SIZE);
        board[enemyDestX * 2, enemyDestY * 2] = BoardState.EnemyDest;

        Fence[,] fences = gameManager.fenceBoard.tiles;
        for (int i = 0; i < FENCE_BOARD_SIZE; i++)
        {
            for (int j = 0; j < FENCE_BOARD_SIZE; j++)
            {
                if (fences[i, j].active)
                {
                    int x = player == 0 ? i : ReverseValue(i, FENCE_BOARD_SIZE);
                    int y = player == 0 ? j : ReverseValue(j, FENCE_BOARD_SIZE);
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
            Vector2Int pos = IndexToVector2Int(action, FENCE_BOARD_SIZE);
            pos = player == 0 ? pos : ReverseVector(pos, FENCE_BOARD_SIZE);
            valid = gameManager.Build(pos, player, true, false);
        }
        else if (action <= 127)
        {
            Vector2Int pos = IndexToVector2Int(action - 64, FENCE_BOARD_SIZE);
            pos = player == 0 ? pos : ReverseVector(pos, FENCE_BOARD_SIZE);
            valid = gameManager.Build(pos, player, false, false);
        }
        else
        {
            Vector2Int pos;
            if (player == 0)
                pos = MOVES[action - 128] + gameManager.GetPlayerPosition(player);
            else
                pos = InvertVector(MOVES[action - 128]) + gameManager.GetPlayerPosition(player);
            valid = gameManager.Move(pos, player);
        }

        if (!valid)
            Debug.LogError($"Player {player} made an invalid move: {action}");
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        HashSet<Tuple<Vector2Int, bool>> validBuilds = gameManager.GetValidBuilds(player);

        HashSet<Vector2Int> validMoveSet = gameManager.GetValidMoves(player);
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
            Vector2Int pos = IndexToVector2Int(i, FENCE_BOARD_SIZE);
            if (!validVBuildSet.Contains(pos))
            {
                int idx = player == 0 ? i : Vector2IntToIndex(ReverseVector(pos, FENCE_BOARD_SIZE), FENCE_BOARD_SIZE);
                // Debug.Log($"Disabling vertical fence at {pos} (action {idx}) for player {player}");
                actionMask.SetActionEnabled(-1, idx, false);
            }
        }
        for (int i = 64; i < 128; i++)
        {
            Vector2Int pos = IndexToVector2Int(i - 64, FENCE_BOARD_SIZE);
            if (!validHBuildSet.Contains(pos))
            {
                int idx = player == 0 ? i : 64 + Vector2IntToIndex(ReverseVector(pos, FENCE_BOARD_SIZE), FENCE_BOARD_SIZE);
                // Debug.Log($"Disabling horizontal fence at {pos} (action {idx}) for player {player}");
                actionMask.SetActionEnabled(-1, idx, false);
            }
        }
        int movesLength = MOVES.Length;
        for (int i = 0; i < movesLength; i++)
        {
            Vector2Int move;
            if (player == 0)
                move = MOVES[i] + gameManager.GetPlayerPosition(player);
            else
                move = MOVES[movesLength - i - 1] + gameManager.GetPlayerPosition(player);
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