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

    public void Init()
    {
        totalTiles = tileBoardSize * tileBoardSize;
        totalFences = fenceBoardSize * fenceBoardSize;
    }

    public override void OnEpisodeBegin()
    {
    }

    // flattened fence board, playernumber, p1pos, p2pos, p1dest, p2dest, p1fences left, p2fences left
    // 8*8*3 + 9*9 + 9*9 + 9*9 + 9*9 + 1 + 1 = 518
    // turns out they were 520, good job
    public override void CollectObservations(VectorSensor sensor)
    {
        Fence[,] fences = gameManager.fenceBoard.tiles;
        for (int i = 0; i < fenceBoardSize; i++)
        {
            for (int j = 0; j < fenceBoardSize; j++)
            {
                if (fences[i, j].active)
                    sensor.AddOneHotObservation(fences[i, j].vertical ? (int)FenceStates.Vertical : (int)FenceStates.Horizontal, 3);
                else
                    sensor.AddOneHotObservation((int)FenceStates.Empty, 3);
            }
        }
        sensor.AddOneHotObservation(player, 2);
        sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerPosition(0), tileBoardSize), totalTiles);
        sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerPosition(1), tileBoardSize), totalTiles);
        sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerDestination(0), tileBoardSize), totalTiles);
        sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerDestination(1), tileBoardSize), totalTiles);
        sensor.AddObservation(gameManager.GetPlayerStatus(0).fences);
        sensor.AddObservation(gameManager.GetPlayerStatus(1).fences);
    }

    // THE ACTIONS ARE AS FOLLOWS:
    // 0                -   TOTALFENCES:            build vertical fence at position
    // TOTALFENCES      -   TOTALFENCES * 2:        build horizontal fence at position
    // TOTALFENCES * 2  -   TOTALFENCES * 2 + 13:   move to position
    public void Act(int action)
    {
        Debug.Log($"Player {player} acting with action {action}");
        bool valid;
        if (action < totalFences)
        {
            Vector2Int pos = IndexToVector2Int(action, fenceBoardSize);
            valid = gameManager.Build(pos, player, true, false);
            Debug.Log($"Player {player} Building vertical fence at {pos} {valid}");
        }
        else if (action < totalFences * 2)
        {
            Vector2Int pos = IndexToVector2Int(action - totalFences, fenceBoardSize);
            valid = gameManager.Build(pos, player, false, false);
            Debug.Log($"Player {player} Building horizontal fence at {pos} {valid}");
        }
        else
        {
            Vector2Int pos = TrainingConstants.moves[action - totalFences * 2] + gameManager.GetPlayerPosition(player);
            valid = gameManager.Move(pos, player);
            Debug.Log($"Player {player} Moving to {pos} {valid}");
        }
        if (!valid)
            Debug.LogError("Invalid action, something must've gone really wrong");
    }

    public void EndGame()
    {
        if (player == gameManager.winner)
            AddReward(1);
        else if (gameManager.winner == -1)
            AddReward(0);
        else
            AddReward(-1);
        EndEpisode();
    }

    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        // WHY ARE THE MASKS -1 INDEXED
        List<Vector2Int> validMoves = gameManager.GetValidMoves(player);
        List<Tuple<Vector2Int, bool>> validBuilds = gameManager.GetValidBuilds(player);

        HashSet<Vector2Int> validMovesSet = new(validMoves);
        HashSet<Vector2Int> validVerticalBuildsSet = new();
        HashSet<Vector2Int> validHorizontalBuildsSet = new();

        foreach (Tuple<Vector2Int, bool> build in validBuilds)
        {
            if (build.Item2)
                validVerticalBuildsSet.Add(build.Item1);
            else
                validHorizontalBuildsSet.Add(build.Item1);
        }

        for (int i = 0; i < fenceBoardSize; i++)
        {
            for (int j = 0; j < fenceBoardSize; j++)
            {
                Vector2Int pos = new(i, j);
                int index = Vector2IntToIndex(pos, fenceBoardSize);
                if (!validVerticalBuildsSet.Contains(pos))
                {
                    actionMask.SetActionEnabled(-1, index, false);
                    Debug.Log($"Vertical build at {pos} for player {player} is {validVerticalBuildsSet.Contains(pos)}, so we will block action {index}");
                }
                if (!validHorizontalBuildsSet.Contains(pos))
                {
                    actionMask.SetActionEnabled(-1, index + totalFences, false);
                    Debug.Log($"Horizontal build at {pos} for player {player} is {validHorizontalBuildsSet.Contains(pos)}, so we will block action {index + totalFences}");
                }
            }
        }

        for (int i = 0; i < TrainingConstants.moves.Length; i++)
        {
            Vector2Int move = TrainingConstants.moves[i] + gameManager.GetPlayerPosition(player);
            int index = totalFences * 2 + i;
            if (!validMovesSet.Contains(move))
            {
                actionMask.SetActionEnabled(-1, index, false);
                Debug.Log($"Move to {move} for player {player} is {validMovesSet.Contains(move)}, so we will block action {index}");
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
}
