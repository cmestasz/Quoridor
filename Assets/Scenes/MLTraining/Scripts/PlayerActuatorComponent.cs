using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

public class PlayerActuatorComponent : ActuatorComponent
{
    [SerializeField] private PlayerAgent playerAgent;
    private const int TOTAL_ACTIONS = 209;

    public override ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { TOTAL_ACTIONS });

    public override IActuator[] CreateActuators()
    {
        IActuator[] actuators = new IActuator[] { new PlayerActuator(TOTAL_ACTIONS, playerAgent) };
        return actuators;
    }

    public class PlayerActuator : IActuator
    {
        private ActionSpec actionSpec;
        private PlayerAgent playerAgent;

        public PlayerActuator(int totalActions, PlayerAgent playerAgent)
        {
            actionSpec = ActionSpec.MakeDiscrete(new int[] { totalActions });
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
