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
    private int maxTurns;
    private int tileBoardSize = TileBoard.SIZE;
    private int fenceBoardSize = FenceBoard.SIZE;
    private int totalTiles;
    private int totalFences;
    private enum FenceStates { Empty, Vertical, Horizontal };

    public void Init(int maxTurns)
    {
        this.maxTurns = maxTurns;
        totalTiles = tileBoardSize * tileBoardSize;
        totalFences = fenceBoardSize * fenceBoardSize;
    }

    public override void OnEpisodeBegin()
    {
    }

    // flattened fence board, p1pos, p2pos, p1dest, p2dest, p1fences left, p2fences left
    // 8*8*3 + 9*9 + 9*9 + 9*9 + 9*9 + 1 + 1 = 518
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
        bool valid;
        if (action < totalFences)
        {
            Vector2Int pos = IndexToVector2Int(action, fenceBoardSize);
            valid = gameManager.Build(pos, player, true);
        }
        else if (action < totalFences * 2)
        {
            Vector2Int pos = IndexToVector2Int(action - totalFences, fenceBoardSize);
            valid = gameManager.Build(pos, player, false);
        }
        else
        {
            Vector2Int pos = TrainingConstants.moves[action - totalFences * 2] + gameManager.GetPlayerPosition(player);
            valid = gameManager.Move(pos, player);
        }
        maxTurns--;
        if (maxTurns == 0)
            gameManager.EndGame(-1);
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
                actionMask.SetActionEnabled(0, index, validVerticalBuildsSet.Contains(pos));
                actionMask.SetActionEnabled(0, index + totalFences, validHorizontalBuildsSet.Contains(pos));
            }
        }

        for (int i = 0; i < TrainingConstants.moves.Length; i++)
        {
            Vector2Int move = TrainingConstants.moves[i] + gameManager.GetPlayerPosition(player);
            int index = totalFences * 2 + i;
            actionMask.SetActionEnabled(0, index, validMovesSet.Contains(move));
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
