using Fusion;
using System;
using UnityEngine;
using UnityEngine.AI;

namespace CSSD
{
    public class PursueBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private float _pursueSpeed = 3;
        [SerializeField] private float _evaluationPriority = 0.5f;

        public NavMeshAgent Agent { get; set; }
        public NetworkPlayer Target { get; set; }
        public float WaveModifier { get; set; }


        private void OnValidate()
        {
            if (_pursueSpeed < 0) _pursueSpeed = 0;
            if (_evaluationPriority < 0) _evaluationPriority = 0;
        }

        public float Evaluate()
        {
            if (Agent == null)
                return 0;

            if (Target == null)
            {
                Agent.destination = gameObject.transform.position;
                return 0;
            }
            return _evaluationPriority;
        }

        public void Behave()
        {
            if (Agent == null)
                return;

            Agent.destination = Target.transform.position;
            Agent.speed = _pursueSpeed * WaveModifier;
            //Debug.Log("Pursue");
        }
    }
}