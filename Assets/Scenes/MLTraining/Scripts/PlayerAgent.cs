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

    // flattened fence board, p1pos, p2pos, p1dest, p2dest, p1fences left, p2fences left
    // 8*8*3 + 9*9 + 9*9 + 9*9 + 9*9 + 1 + 1 = 518
    public override void CollectObservations(VectorSensor sensor)
    {

        if (player == 0)
        {
            bool[,] vertical = new bool[fenceBoardSize, fenceBoardSize - 1];
            bool[,] horizontal = new bool[fenceBoardSize - 1, fenceBoardSize];
            Fence[,] fences = gameManager.fenceBoard.tiles;
            for (int i = 0; i < fenceBoardSize; i++)
            {
                for (int j = 0; j < fenceBoardSize; j++)
                {
                    if (fences[i, j].active)
                    {
                        sensor.AddOneHotObservation(fences[i, j].vertical ? (int)FenceStates.Vertical : (int)FenceStates.Horizontal, 3);
                    }
                    else
                    {
                        vertical[i, j] = false;
                        horizontal[i, j] = false;
                    }
                }
            }
            sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerPosition(0), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerPosition(1), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerDestination(0), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(gameManager.GetPlayerDestination(1), tileBoardSize), totalTiles);
            sensor.AddObservation(gameManager.GetPlayerStatus(0).fences);
            sensor.AddObservation(gameManager.GetPlayerStatus(1).fences);
        }
        else if (player == 1)
        {
            Fence[,] fences = gameManager.fenceBoard.tiles;
            for (int i = fenceBoardSize - 1; i >= 0; i--)
            {
                for (int j = fenceBoardSize - 1; j >= 0; j--)
                {
                    if (fences[i, j].active)
                        sensor.AddOneHotObservation(fences[i, j].vertical ? (int)FenceStates.Vertical : (int)FenceStates.Horizontal, 3);
                    else
                        sensor.AddOneHotObservation((int)FenceStates.Empty, 3);
                }
            }
            sensor.AddOneHotObservation(Vector2IntToIndex(ReverseVector(gameManager.GetPlayerPosition(1), tileBoardSize), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(ReverseVector(gameManager.GetPlayerPosition(0), tileBoardSize), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(ReverseVector(gameManager.GetPlayerDestination(1), tileBoardSize), tileBoardSize), totalTiles);
            sensor.AddOneHotObservation(Vector2IntToIndex(ReverseVector(gameManager.GetPlayerDestination(0), tileBoardSize), tileBoardSize), totalTiles);
            sensor.AddObservation(gameManager.GetPlayerStatus(1).fences);
            sensor.AddObservation(gameManager.GetPlayerStatus(0).fences);
        }
    }

    // THE ACTIONS ARE AS FOLLOWS:
    // 0                -   TOTALFENCES:            build vertical fence at position
    // TOTALFENCES      -   TOTALFENCES * 2:        build horizontal fence at position
    // TOTALFENCES * 2  -   TOTALFENCES * 2 + 13:   move to position
    public void Act(int action)
    {
        //Debug.Log($"Player {player} acting with action {action}");
        bool valid;
        if (action < totalFences)
        {
            Vector2Int pos = IndexToVector2Int(action, fenceBoardSize);
            if (player == 1)
                pos = ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, true, false);
            //Debug.Log($"Player {player} Building vertical fence at {pos} {valid}");
        }
        else if (action < totalFences * 2)
        {
            Vector2Int pos = IndexToVector2Int(action - totalFences, fenceBoardSize);
            if (player == 1)
                pos = ReverseVector(pos, fenceBoardSize);
            valid = gameManager.Build(pos, player, false, false);
            //Debug.Log($"Player {player} Building horizontal fence at {pos} {valid}");
        }
        else
        {
            Vector2Int pos;
            if (player == 1)
            {
                pos = InvertVector(TrainingConstants.moves[action - totalFences * 2]) + gameManager.GetPlayerPosition(player);
            }
            else
            {
                pos = TrainingConstants.moves[action - totalFences * 2] + gameManager.GetPlayerPosition(player);
            }
            valid = gameManager.Move(pos, player);
            // Debug.Log($"Player {player} Moving to {pos} {valid}");
        }
        if (!valid)
            Debug.LogError($"Player {player} made an invalid move: {action}");
        AddReward(-0.0015f);
    }

    public void EndGame()
    {
        string winner = gameManager.winner == -1 ? "It's a draw" : gameManager.winner == player ? "I won" : "I lost";
        Debug.Log($"I am player {player}, {winner}");
        if (gameManager.winner == -1)
            AddReward(-0.5f);
        else if (player == gameManager.winner)
            AddReward(1f);
        else
            SetReward(-1f);
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

        if (player == 1)
        {
            validVerticalBuildsSet = ReverseVectors(validVerticalBuildsSet, fenceBoardSize);
            validHorizontalBuildsSet = ReverseVectors(validHorizontalBuildsSet, fenceBoardSize);
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
                    //Debug.Log($"Vertical build at {pos} for player {player} is {validVerticalBuildsSet.Contains(pos)}, so we will block action {index}");
                }
                if (!validHorizontalBuildsSet.Contains(pos))
                {
                    actionMask.SetActionEnabled(-1, index + totalFences, false);
                    //Debug.Log($"Horizontal build at {pos} for player {player} is {validHorizontalBuildsSet.Contains(pos)}, so we will block action {index + totalFences}");
                }
            }
        }

        for (int i = 0; i < TrainingConstants.moves.Length; i++)
        {
            Vector2Int move;
            if (player == 1)
            {
                move = InvertVector(TrainingConstants.moves[i]) + gameManager.GetPlayerPosition(player);
            }
            else
            {
                move = TrainingConstants.moves[i] + gameManager.GetPlayerPosition(player);
            }
            int index = totalFences * 2 + i;
            if (!validMovesSet.Contains(move))
            {
                actionMask.SetActionEnabled(-1, index, false);
                //Debug.Log($"Move to {move} for player {player} is {validMovesSet.Contains(move)}, so we will block action {index}");
            }
        }
        actionMask.SetActionEnabled(-1, totalFences * 2 + 6, true);
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
        return new Vector2Int(size - vector.x - 1, size - vector.y - 1);
    }


    private Vector2Int InvertVector(Vector2Int vector)
    {
        return new Vector2Int(-vector.x, -vector.y);
    }
}
