using UnityEngine;
using UnityEngine.AI;
using Fusion;

namespace CSSD
{
    public class WaitBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private float _evaluationPriority = 0.25f;
        //Other components
        private HPHandler _hpHandler;

        private float _waveModifier;
        public NavMeshAgent Agent { get; set; }
        public NetworkPlayer Target { get; set; }
        public float WaveModifier { get; set; }

        private void OnValidate()
        {
            if (_evaluationPriority < 0) _evaluationPriority = 0;
        }

        public override void Spawned()
        {
            _hpHandler = GetComponent<HPHandler>();
        }

        public float Evaluate()
        {
            if (Agent == null)
                return 0;

            if (_hpHandler.IsDead)
                return float.MaxValue;
            else
                return _evaluationPriority;
        }

        public void Behave()
        {
            Agent.destination = gameObject.transform.position;
            //Debug.Log("Wait");
        }
    }
}