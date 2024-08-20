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

    public override void CollectObservations(VectorSensor sensor)
    {
        if (player == 0)
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

            int p0pos = Vector2IntToIndex(gameManager.GetPlayerPosition(0), tileBoardSize);
            int p1pos = Vector2IntToIndex(gameManager.GetPlayerPosition(1), tileBoardSize);
            int p0dest = Vector2IntToIndex(gameManager.GetPlayerDestination(0), tileBoardSize);
            int p1dest = Vector2IntToIndex(gameManager.GetPlayerDestination(1), tileBoardSize);
            float p0fences = Normalize(gameManager.GetPlayerStatus(0).fences, 0, PlayerStatus.MAX_FENCES);
            float p1fences = Normalize(gameManager.GetPlayerStatus(1).fences, 0, PlayerStatus.MAX_FENCES);


            sensor.AddOneHotObservation(p0pos, totalTiles);
            sensor.AddOneHotObservation(p1pos, totalTiles);
            sensor.AddOneHotObservation(p0dest, totalTiles);
            sensor.AddOneHotObservation(p1dest, totalTiles);
            sensor.AddObservation(p0fences);
            sensor.AddObservation(p1fences);
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

            int p0pos = Vector2IntToIndex(ReverseVector(gameManager.GetPlayerPosition(0), tileBoardSize), tileBoardSize);
            int p1pos = Vector2IntToIndex(ReverseVector(gameManager.GetPlayerPosition(1), tileBoardSize), tileBoardSize);
            int p0dest = Vector2IntToIndex(ReverseVector(gameManager.GetPlayerDestination(0), tileBoardSize), tileBoardSize);
            int p1dest = Vector2IntToIndex(ReverseVector(gameManager.GetPlayerDestination(1), tileBoardSize), tileBoardSize);
            float p0fences = Normalize(gameManager.GetPlayerStatus(0).fences, 0, PlayerStatus.MAX_FENCES);
            float p1fences = Normalize(gameManager.GetPlayerStatus(1).fences, 0, PlayerStatus.MAX_FENCES);

            sensor.AddOneHotObservation(p0pos, totalTiles);
            sensor.AddOneHotObservation(p1pos, totalTiles);
            sensor.AddOneHotObservation(p0dest, totalTiles);
            sensor.AddOneHotObservation(p1dest, totalTiles);
            sensor.AddObservation(p0fences);
            sensor.AddObservation(p1fences);
        }
    }

    // THE ACTIONS ARE AS FOLLOWS (inclusive - exclusive):
    // 0       -   64:            build vertical fence at position
    // 64      -   64 * 2:        build horizontal fence at position
    // 64 * 2  -   64 * 2 + 13:   move to position
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
        AddReward(-0.025f);
    }

    public void EndGame()
    {
        //string winner = gameManager.winner == -1 ? "It's a draw" : gameManager.winner == player ? "I won" : "I lost";
        //Debug.Log($"I am player {player}, {winner}");
        if (gameManager.winner == -1)
            SetReward(-1f);
        else if (player == gameManager.winner)
            AddReward(1f);
        else
            SetReward(-1f);
        EndEpisode();
    }

    // 0       -   64:            build vertical fence at position
    // 64      -   64 * 2:        build horizontal fence at position
    // 64 * 2  -   64 * 2 + 13:   move to position
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
                    // Debug.Log($"Vertical build at {pos} for player {player} is {validVerticalBuildsSet.Contains(pos)}, so we will block action {index}");
                }
                if (!validHorizontalBuildsSet.Contains(pos))
                {
                    actionMask.SetActionEnabled(-1, index + totalFences, false);
                    // Debug.Log($"Horizontal build at {pos} for player {player} is {validHorizontalBuildsSet.Contains(pos)}, so we will block action {index + totalFences}");
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
                // Debug.Log($"Move to {move} for player {player} is {validMovesSet.Contains(move)}, so we will block action {index}");
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

    private float Normalize(float value, float min, float max)
    {
        return (value - min) / (max - min);
    }

    private void Put<T>(T[,] array, T value, int i, int j)
    {
        if (i >= 0 && i < array.GetLength(0) && j >= 0 && j < array.GetLength(1))
        {
            array[i, j] = value;
        }
    }

    private void AddOneHot(List<float> obs, int value, int size)
    {
        for (int i = 0; i < size; i++)
        {
            obs.Add(i == value ? 1 : 0);
        }
    }

}
