using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using UnityEngine;

public class PlayerAgent : Agent
{
    // Start is called before the first frame update
    void Start()
    {

    }

    public override void OnEpisodeBegin()
    {
        // Reset the player's position
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // Add observations
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // Apply actions
    }
}
