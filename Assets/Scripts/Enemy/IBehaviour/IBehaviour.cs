using UnityEngine.AI;

namespace CSSD {
    public interface IBehaviour
    {
        NavMeshAgent Agent { get; set; }
        NetworkPlayer Target { get; set; }
        float WaveModifier { get; set; }
        float Evaluate();
        void Behave();
    } 
}
