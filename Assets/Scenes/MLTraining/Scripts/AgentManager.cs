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
    private int turn;

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    public void StartGame()
    {
        gameManager.StartGame();
        agent1.Init();
        agent2.Init();
        turn = 0;
        StartCoroutine(HandleEpisodes());
    }

    public void EndGame()
    {
        agent1.EndGame();
        agent2.EndGame();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        
    }

    public IEnumerator HandleEpisodes()
    {
        Debug.Log("Starting game");
        yield return new WaitForSeconds(1f);
        while (gameManager.playing && turn < maxTurns)
        {
            if (turn % 2 == 0)
            {
                agent1.RequestDecision();
            }
            else
            {
                agent2.RequestDecision();
            }
            turn++;
            yield return new WaitForSeconds(0.5f);
        }
        EndGame();
        yield return new WaitForSeconds(1f);
        StartGame();
    }
}
