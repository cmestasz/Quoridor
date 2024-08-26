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
    private enum FenceStates { Empty, Vertical, Horizontal };
    private BufferSensorComponent bufferSensor;

    public void Init()
    {
        totalTiles = tileBoardSize * tileBoardSize;
        totalFences = fenceBoardSize * fenceBoardSize;
        bufferSensor = GetComponent<BufferSensorComponent>();
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

    // for normal: playerx, playery, playerxdest, playerydest, enemyx, enemyy, enemyxdest, enemyydest, playerfences, enemyfences
    // for buffer: (fencex, fencey, vertical) repeat
    public override void CollectObservations(VectorSensor sensor)
    {
        Fence[,] fences = gameManager.fenceBoard.tiles;
        for (int i = 0; i < fenceBoardSize; i++)
        {
            for (int j = 0; j < fenceBoardSize; j++)
            {
                if (fences[i, j].active)
                {
                    int x = player == 0 ? i : ReverseValue(i, fenceBoardSize);
                    int y = player == 0 ? j : ReverseValue(j, fenceBoardSize);
                    float[] obs = { x, y, fences[i, j].vertical ? 1 : 0 };
                    bufferSensor.AppendObservation(obs);
                }
            }
        }

        Vector2Int playerPos = gameManager.GetPlayerPosition(player);
        int playerX = player == 0 ? playerPos.x : ReverseValue(playerPos.x, tileBoardSize);
        int playerY = player == 0 ? playerPos.y : ReverseValue(playerPos.y, tileBoardSize);
        sensor.AddOneHotObservation(playerX, tileBoardSize);
        sensor.AddOneHotObservation(playerY, tileBoardSize);

        Vector2Int playerDest = gameManager.GetPlayerDestination(player);
        int playerDestX = player == 0 ? playerDest.x : ReverseValue(playerDest.x, tileBoardSize);
        int playerDestY = player == 0 ? playerDest.y : ReverseValue(playerDest.y, tileBoardSize);
        sensor.AddOneHotObservation(playerDestX, tileBoardSize);
        sensor.AddOneHotObservation(playerDestY, tileBoardSize);

        Vector2Int enemyPos = gameManager.GetPlayerPosition(1 - player);
        int enemyX = player == 0 ? enemyPos.x : ReverseValue(enemyPos.x, tileBoardSize);
        int enemyY = player == 0 ? enemyPos.y : ReverseValue(enemyPos.y, tileBoardSize);
        sensor.AddOneHotObservation(enemyX, tileBoardSize);
        sensor.AddOneHotObservation(enemyY, tileBoardSize);

        Vector2Int enemyDest = gameManager.GetPlayerDestination(1 - player);
        int enemyDestX = player == 0 ? enemyDest.x : ReverseValue(enemyDest.x, tileBoardSize);
        int enemyDestY = player == 0 ? enemyDest.y : ReverseValue(enemyDest.y, tileBoardSize);
        sensor.AddOneHotObservation(enemyDestX, tileBoardSize);
        sensor.AddOneHotObservation(enemyDestY, tileBoardSize);

        float playerFences = Normalize(gameManager.GetPlayerStatus(player).fences, 0, PlayerStatus.MAX_FENCES);
        float enemyFences = Normalize(gameManager.GetPlayerStatus(1 - player).fences, 0, PlayerStatus.MAX_FENCES);
        sensor.AddObservation(playerFences);
        sensor.AddObservation(enemyFences);
    }

    // 0    -   63  :   build vertical fence at position
    // 64   -   127 :   build horizontal fence at position
    // 128  -   208 :   move to position
    public void Act(int action)
    {
        bool valid = true;
        if (action <= 63)
        {
            Vector2Int pos = IndexToVector2Int(action, fenceBoardSize);
            pos = player == 0 ? pos : ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, true, false);
        } else if (action <= 127)
        {
            Vector2Int pos = IndexToVector2Int(action - 64, fenceBoardSize);
            pos = player == 0 ? pos : ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, false, false);
        } else if (action <= 208)
        {
            Vector2Int pos = IndexToVector2Int(action - 128, tileBoardSize);
            pos = player == 0 ? pos : ReverseVector(pos, tileBoardSize);
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
        for (int i = 128; i < 209; i++)
        {
            Vector2Int pos = IndexToVector2Int(i - 128, tileBoardSize);
            if (!validMoveSet.Contains(pos))
            {
                int idx = player == 0 ? i : 128 + Vector2IntToIndex(ReverseVector(pos, tileBoardSize), tileBoardSize);
                // Debug.Log($"Disabling move to {pos} (action {idx}) for player {player}");
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
