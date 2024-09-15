using Unity.MLAgents.Sensors;
using UnityEngine;
using static Constants;

public class InferencePlayerSensorComponenet : SensorComponent
{
    [SerializeField] private InferencePlayerAgent playerAgent;

    public override ISensor[] CreateSensors()
    {
        ISensor[] sensors = new ISensor[] { new PlayerVisualSensor(playerAgent), new PlayerVectorSensor(playerAgent) };
        return sensors;
    }

    public class PlayerVisualSensor : ISensor
    {
        private readonly InferencePlayerAgent playerAgent;
        
        public PlayerVisualSensor(InferencePlayerAgent playerAgent)
        {
            this.playerAgent = playerAgent;
        }

        public byte[] GetCompressedObservation()
        {
            throw new System.NotImplementedException();
        }

        public CompressionSpec GetCompressionSpec()
        {
            return CompressionSpec.Default();
        }

        public string Name => "PlayerVisualSensor";

        public ObservationSpec GetObservationSpec()
        {
            int size = FENCE_BOARD_SIZE + TILE_BOARD_SIZE;
            return ObservationSpec.Visual(InferencePlayerAgent.BOARD_STATES, size, size);
        }

        public void Reset()
        {
        }

        public void Update()
        {
            playerAgent.UpdateVisualObservation();
        }

        public int Write(ObservationWriter writer)
        {
            return playerAgent.FillVisualObservation(writer);
        }
    }

    public class PlayerVectorSensor : ISensor
    {
        private readonly InferencePlayerAgent playerAgent;

        public PlayerVectorSensor(InferencePlayerAgent playerAgent)
        {
            this.playerAgent = playerAgent;
        }

        public byte[] GetCompressedObservation()
        {
            throw new System.NotImplementedException();
        }

        public CompressionSpec GetCompressionSpec()
        {
            return CompressionSpec.Default();
        }

        public string Name => "PlayerVectorSensor";

        public ObservationSpec GetObservationSpec()
        {
            return ObservationSpec.Vector(2);
        }

        public void Reset()
        {
        }

        public void Update()
        {
            
        }

        public int Write(ObservationWriter writer)
        {
            return playerAgent.FillVectorObservation(writer);
        }
    }
}