using System.Collections;
using System.Collections.Generic;
using Unity.MLAgents;
using UnityEngine;

public class AgentManager : MonoBehaviour
{
    [SerializeField] private PlayerAgent agent1;
    [SerializeField] private PlayerAgent agent2;
    [SerializeField] private ContinuousGameManager gameManager;
    [SerializeField] private int maxTurns;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameManager.StartGame();
        agent1.Init(maxTurns);
        agent2.Init(maxTurns);
    }

    public void EndGame()
    {
        agent1.EndGame();
        agent2.EndGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
