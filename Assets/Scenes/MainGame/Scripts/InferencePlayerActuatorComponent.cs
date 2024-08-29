using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using UnityEngine;

public class InferentePlayerActuatorComponent : ActuatorComponent
{
    [SerializeField] private InferencePlayerAgent playerAgent;
    private const int TOTAL_ACTIONS = 141;

    public override ActionSpec ActionSpec => ActionSpec.MakeDiscrete(new int[] { TOTAL_ACTIONS });

    public override IActuator[] CreateActuators()
    {
        IActuator[] actuators = new IActuator[] { new PlayerActuator(TOTAL_ACTIONS, playerAgent) };
        return actuators;
    }

    public class PlayerActuator : IActuator
    {
        private ActionSpec actionSpec;
        private readonly InferencePlayerAgent playerAgent;

        public PlayerActuator(int totalActions, InferencePlayerAgent playerAgent)
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
