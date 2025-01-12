using UnityEngine;
using UnityEngine.AI;
using Fusion;

namespace CSSD
{
    public class ChargeBehaviour : NetworkBehaviour, IBehaviour
    {
        [SerializeField] private float _cooldownTime = 1;
        [SerializeField] private float _chargeDistance = 1;
        [SerializeField] private float _chargeMoveSpeed = 0.01f;
        [SerializeField] private float _evaluationPriority = 0.7f;

        [Networked] private TickTimer _cooldown { get; set; }
        public NavMeshAgent Agent { get; set; }
        public NetworkPlayer Target { get; set; }
        public float WaveModifier { get; set; }

        public float Evaluate()
        {
            if (Target == null)
            {
                Agent.destination = gameObject.transform.position;
                return 0;
            }
            else
            {
                float dist = Vector2.Distance(transform.position, Target.transform.position) / WaveModifier;

                if (dist < _chargeDistance)
                {
                    return _evaluationPriority;
                }

                return 0;
            }
        }

        public void Behave()
        {
            Agent.destination = Target.transform.position;
            Agent.speed = _chargeMoveSpeed;
            _cooldown = TickTimer.CreateFromSeconds(Runner, _cooldownTime);
            Debug.Log("Charge");            
        }
    }
}