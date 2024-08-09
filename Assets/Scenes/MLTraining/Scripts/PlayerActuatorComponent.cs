using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

public class PlayerActuatorComponent : ActuatorComponent
{
    // moves, they look like a 5x5 diamond around the player
    private PlayerAgent playerAgent;
    private int totalActions = 13 + FenceBoard.SIZE * FenceBoard.SIZE * 2;

    public override ActionSpec ActionSpec => ActionSpec.MakeDiscrete(totalActions);

    void Start()
    {
        playerAgent = GetComponent<PlayerAgent>();
    }

    public override IActuator[] CreateActuators()
    {
        return new IActuator[] { new PlayerActuator(totalActions, playerAgent) };
    }

    public class PlayerActuator : IActuator
    {
        private ActionSpec actionSpec;
        private PlayerAgent playerAgent;

        public PlayerActuator(int totalActions, PlayerAgent playerAgent)
        {
            actionSpec = ActionSpec.MakeDiscrete(totalActions);
            this.playerAgent = playerAgent;
        }

        public ActionSpec ActionSpec => actionSpec;

        public string Name => "PlayerActuator";

        public void Heuristic(in ActionBuffers actionBuffersOut)
        {
            throw new System.NotImplementedException();
        }

        public void OnActionReceived(ActionBuffers actionBuffers)
        {
            playerAgent.Act(actionBuffers.DiscreteActions[0]);
        }

        public void ResetData()
        {
            // :3
        }

        public void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            // :3
        }
    }
}
